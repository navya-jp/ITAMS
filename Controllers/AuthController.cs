using Microsoft.AspNetCore.Mvc;
using ITAMS.Domain.Interfaces;
using ITAMS.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly ITAMSDbContext _context;

        public AuthController(
            IUserService userService,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            ITAMSDbContext context)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
            _context = context;
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

                _logger.LogInformation("Login attempt for user: {Username}, ActiveSessionId: {SessionId}, LastActivity: {LastActivity}", 
                    user.Username, user.ActiveSessionId, user.LastActivityAt);

                // Check if user already has an active session
                if (!string.IsNullOrEmpty(user.ActiveSessionId) && user.SessionStartedAt.HasValue)
                {
                    // Check if session is still valid based on last activity (within 30 minutes)
                    var lastActivity = user.LastActivityAt ?? user.SessionStartedAt.Value;
                    var timeSinceActivity = DateTime.UtcNow - lastActivity;
                    
                    _logger.LogInformation("Session check: User {Username}, Time since activity: {Minutes} minutes", 
                        user.Username, timeSinceActivity.TotalMinutes);
                    
                    if (timeSinceActivity.TotalMinutes < 30)
                    {
                        _logger.LogWarning("BLOCKING LOGIN: User {Username} attempted login with active session (last activity: {LastActivity})", 
                            request.Username, lastActivity);
                        return Unauthorized(new LoginResponse
                        {
                            Success = false,
                            Message = "This account is currently logged in from another location. Please logout from the other session first."
                        });
                    }
                    else
                    {
                        // Session expired due to inactivity, clear it
                        _logger.LogInformation("Clearing expired session for user {Username}", request.Username);
                        await _userService.ClearSessionAsync(user.Id);
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

                // Capture login audit details
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();
                var browserType = ExtractBrowserType(userAgent);
                var operatingSystem = ExtractOperatingSystem(userAgent);

                var loginAudit = new LoginAudit
                {
                    UserId = authenticatedUser.Id,
                    Username = authenticatedUser.Username,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    BrowserType = browserType,
                    OperatingSystem = operatingSystem,
                    Status = "ACTIVE"
                };

                _context.LoginAudits.Add(loginAudit);
                await _context.SaveChangesAsync();

                // Log the successful authentication
                _logger.LogInformation("Login successful for user: {Username}, Role: {Role}, FirstLogin: {IsFirstLogin}, SessionId: {SessionId}, IP: {IpAddress}", 
                    authenticatedUser.Username, authenticatedUser.Role?.Name, isFirstLogin, sessionId, ipAddress);

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
                int userId = 0;
                string? logoutType = null;
                
                // Try to get data from JSON body first
                if (request != null && request.UserId > 0)
                {
                    userId = request.UserId;
                    logoutType = request.LogoutType;
                }
                // If not in body, try form data (for sendBeacon)
                else if (Request.HasFormContentType)
                {
                    var form = await Request.ReadFormAsync();
                    if (form.ContainsKey("userId") && int.TryParse(form["userId"], out int formUserId))
                    {
                        userId = formUserId;
                        logoutType = form.ContainsKey("logoutType") ? form["logoutType"].ToString() : null;
                    }
                }
                
                _logger.LogInformation("Logout request received: UserId={UserId}, LogoutType={LogoutType}", 
                    userId, logoutType);
                
                if (userId > 0)
                {
                    // Get user to find active session
                    var user = await _userService.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        // Update the most recent active login audit record for this user
                        var loginAudit = _context.LoginAudits
                            .Where(la => la.UserId == user.Id && la.Status == "ACTIVE")
                            .OrderByDescending(la => la.LoginTime)
                            .FirstOrDefault();

                        if (loginAudit != null)
                        {
                            loginAudit.LogoutTime = DateTime.UtcNow;
                            // Set status based on logout type
                            loginAudit.Status = logoutType switch
                            {
                                "SESSION_TIMEOUT" => "SESSION_TIMEOUT",
                                "FORCED_LOGOUT" => "FORCED_LOGOUT",
                                _ => "LOGGED_OUT"
                            };
                            await _context.SaveChangesAsync();
                            
                            _logger.LogInformation("Login audit updated: AuditId={AuditId}, Status={Status}", 
                                loginAudit.Id, loginAudit.Status);
                        }
                        else
                        {
                            _logger.LogWarning("No active login audit found for UserId={UserId}", user.Id);
                        }
                    }

                    // Clear the user's session
                    await _userService.ClearSessionAsync(userId);
                    _logger.LogInformation("User {UserId} logged out with type: {LogoutType}", userId, logoutType ?? "LOGGED_OUT");
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

        private string ExtractBrowserType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            if (userAgent.Contains("Edg/")) return "Microsoft Edge";
            if (userAgent.Contains("Chrome/") && !userAgent.Contains("Edg/")) return "Google Chrome";
            if (userAgent.Contains("Firefox/")) return "Mozilla Firefox";
            if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome/")) return "Safari";
            if (userAgent.Contains("Opera/") || userAgent.Contains("OPR/")) return "Opera";
            if (userAgent.Contains("MSIE") || userAgent.Contains("Trident/")) return "Internet Explorer";

            return "Other";
        }

        private string ExtractOperatingSystem(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            if (userAgent.Contains("Windows NT 10.0")) return "Windows 10/11";
            if (userAgent.Contains("Windows NT 6.3")) return "Windows 8.1";
            if (userAgent.Contains("Windows NT 6.2")) return "Windows 8";
            if (userAgent.Contains("Windows NT 6.1")) return "Windows 7";
            if (userAgent.Contains("Windows")) return "Windows";
            
            if (userAgent.Contains("Mac OS X")) return "macOS";
            if (userAgent.Contains("Linux")) return "Linux";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iOS") || userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";

            return "Other";
        }

        private string GetClientIpAddress()
        {
            // Check for forwarded IP (when behind proxy/load balancer)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',');
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            // Check for real IP header
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Get remote IP address
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            if (remoteIp != null)
            {
                // If it's localhost, get the actual network IP of the server
                if (remoteIp.ToString() == "::1" || remoteIp.ToString() == "127.0.0.1")
                {
                    try
                    {
                        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                        var localIp = host.AddressList
                            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                        
                        if (localIp != null)
                        {
                            return $"{localIp} (localhost)";
                        }
                    }
                    catch
                    {
                        // If we can't get the network IP, fall back to localhost
                    }
                    
                    return "127.0.0.1 (localhost)";
                }
                
                // If it's IPv4-mapped IPv6, extract the IPv4 part
                if (remoteIp.IsIPv4MappedToIPv6)
                {
                    return remoteIp.MapToIPv4().ToString();
                }
                
                return remoteIp.ToString();
            }

            return "Unknown";
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
        public string? LogoutType { get; set; } // LOGGED_OUT, SESSION_TIMEOUT, FORCED_LOGOUT
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