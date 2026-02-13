using Microsoft.AspNetCore.Mvc;
using ITAMS.Domain.Interfaces;
using ITAMS.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ITAMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Username and password are required"
                    });
                }

                // First, get the user to check if it's their first login and session status
                var users = await _userService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => 
                    u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) && 
                    u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("User not found or inactive: {Username}", request.Username);
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                // Check if user already has an active session
                if (!string.IsNullOrEmpty(user.ActiveSessionId) && user.SessionStartedAt.HasValue)
                {
                    // Check if session is still valid (within 30 minutes)
                    var sessionAge = DateTime.UtcNow - user.SessionStartedAt.Value;
                    if (sessionAge.TotalMinutes < 30)
                    {
                        _logger.LogWarning("User {Username} attempted login with active session", request.Username);
                        return Unauthorized(new LoginResponse
                        {
                            Success = false,
                            Message = "This account is currently logged in from another location. Please logout from the other session first."
                        });
                    }
                }

                // Check if this is the first login BEFORE authentication (which updates LastLoginAt)
                bool isFirstLogin = user.LastLoginAt == null;

                // Now authenticate the user (this will update LastLoginAt)
                var authenticatedUser = await _userService.AuthenticateAsync(request.Username, request.Password);
                
                if (authenticatedUser == null)
                {
                    _logger.LogWarning("Authentication failed for user: {Username}", request.Username);
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                // Generate session ID and update user
                var sessionId = Guid.NewGuid().ToString();
                await _userService.UpdateSessionAsync(authenticatedUser.Id, sessionId, DateTime.UtcNow);

                // Log the successful authentication
                _logger.LogInformation("Login successful for user: {Username}, Role: {Role}, FirstLogin: {IsFirstLogin}, SessionId: {SessionId}", 
                    authenticatedUser.Username, authenticatedUser.Role?.Name, isFirstLogin, sessionId);

                // Generate JWT token
                var token = GenerateJwtToken(authenticatedUser, sessionId);

                var response = new LoginResponse
                {
                    Success = true,
                    Token = token,
                    User = new AuthUserDto
                    {
                        Id = authenticatedUser.Id,
                        Username = authenticatedUser.Username,
                        Email = authenticatedUser.Email,
                        FirstName = authenticatedUser.FirstName,
                        LastName = authenticatedUser.LastName,
                        RoleId = authenticatedUser.RoleId,
                        RoleName = authenticatedUser.Role?.Name ?? "User",
                        IsActive = authenticatedUser.IsActive,
                        MustChangePassword = authenticatedUser.MustChangePassword,
                        IsFirstLogin = isFirstLogin,
                        LastLoginAt = authenticatedUser.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        SessionId = sessionId
                    }
                };

                _logger.LogInformation("User {Username} logged in successfully", authenticatedUser.Username);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
        {
            try
            {
                if (request != null && request.UserId > 0)
                {
                    // Clear the user's session
                    await _userService.ClearSessionAsync(request.UserId);
                    _logger.LogInformation("User {UserId} logged out successfully", request.UserId);
                }
                
                return Ok(new { success = true, message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { success = false, message = "An error occurred during logout" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // TODO: Implement proper password change logic
                // This is a placeholder implementation
                
                if (string.IsNullOrEmpty(request.NewPassword) || 
                    string.IsNullOrEmpty(request.ConfirmPassword))
                {
                    return BadRequest(new { message = "New password and confirmation are required" });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(new { message = "New password and confirmation do not match" });
                }

                // TODO: Validate password strength
                if (request.NewPassword.Length < 8)
                {
                    return BadRequest(new { message = "Password must be at least 8 characters long" });
                }

                _logger.LogInformation("Password changed successfully");
                return Ok(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(500, new { message = "An error occurred during password change" });
            }
        }

        [HttpGet("validate-session")]
        public IActionResult ValidateSession()
        {
            // TODO: Implement proper session validation
            return Ok(new { valid = true });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            // TODO: Implement token refresh logic
            return Ok(new { token = "refreshed-token" });
        }

        [HttpGet("check-user-status/{username}")]
        public async Task<IActionResult> CheckUserStatus(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Username is required" });
                }

                // Get all users to find the matching one
                var users = await _userService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => 
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                    u.IsActive);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { 
                    exists = true,
                    isFirstLogin = user.LastLoginAt == null,
                    mustChangePassword = user.MustChangePassword,
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user status for username {Username}", username);
                return StatusCode(500, new { message = "An error occurred while checking user status" });
            }
        }

        [HttpGet("security-settings")]
        public IActionResult GetSecuritySettings()
        {
            // TODO: Load from configuration or database
            var settings = new SecuritySettings
            {
                MaxLoginAttempts = 5,
                LockoutDurationMinutes = 30,
                SessionTimeoutMinutes = 30,
                PasswordExpiryDays = 90,
                RequirePasswordChange = true,
                AllowMultipleSessions = false,
                AutoLogoutWarningMinutes = 5
            };

            return Ok(settings);
        }

        private string GenerateJwtToken(Domain.Entities.User user, string sessionId)
        {
            // TODO: Move to configuration
            var secretKey = "your-super-secret-key-that-should-be-in-configuration-and-at-least-32-characters-long";
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
                    new Claim("SessionId", sessionId)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // DTOs for authentication
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public int UserId { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public AuthUserDto? User { get; set; }
        public string? Message { get; set; }
        public bool RequiresPasswordChange { get; set; }
        public bool IsFirstLogin { get; set; }
        public LockoutInfo? LockoutInfo { get; set; }
    }

    public class AuthUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool MustChangePassword { get; set; }
        public bool IsFirstLogin { get; set; }
        public string? LastLoginAt { get; set; }
        public string? SessionId { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LockoutInfo
    {
        public bool IsLocked { get; set; }
        public string? LockoutEnd { get; set; }
        public int? AttemptsRemaining { get; set; }
    }

    public class SecuritySettings
    {
        public int MaxLoginAttempts { get; set; }
        public int LockoutDurationMinutes { get; set; }
        public int SessionTimeoutMinutes { get; set; }
        public int PasswordExpiryDays { get; set; }
        public bool RequirePasswordChange { get; set; }
        public bool AllowMultipleSessions { get; set; }
        public int AutoLogoutWarningMinutes { get; set; }
    }
}