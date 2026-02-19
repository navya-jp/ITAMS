using Microsoft.AspNetCore.Mvc;
using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;
using ITAMS.Models;
using ITAMS.Services;
using ITAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly IAccessControlService _accessControlService;
    private readonly ITAMSDbContext _context;

    public UsersController(
        IUserService userService, 
        IAuditService auditService,
        IAccessControlService accessControlService,
        ITAMSDbContext context)
    {
        _userService = userService;
        _auditService = auditService;
        _accessControlService = accessControlService;
        _context = context;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = users.Select(u => MapToUserDto(u));

            return Ok(new ApiResponse<IEnumerable<UserDto>>
            {
                Success = true,
                Data = userDtos,
                Message = "Users retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<UserDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving users",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Data = MapToUserDto(user),
                Message = "User retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the user",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var request = new CreateUserRequest
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                RoleId = createUserDto.RoleId,
                Password = createUserDto.Password,
                MustChangePassword = createUserDto.MustChangePassword
            };

            var user = await _userService.CreateUserAsync(request);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new ApiResponse<UserDto>
            {
                Success = true,
                Data = MapToUserDto(user),
                Message = "User created successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<UserDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "An error occurred while creating the user",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var request = new UpdateUserRequest
            {
                Email = updateUserDto.Email,
                FirstName = updateUserDto.FirstName,
                LastName = updateUserDto.LastName,
                RoleId = updateUserDto.RoleId,
                IsActive = updateUserDto.IsActive
            };

            var user = await _userService.UpdateUserAsync(id, request);

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Data = MapToUserDto(user),
                Message = "User updated successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<UserDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "An error occurred while updating the user",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Deactivate a user (soft delete)
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(int id)
    {
        try
        {
            var request = new UpdateUserRequest { IsActive = false };
            await _userService.UpdateUserAsync(id, request);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "User deactivated successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deactivating the user",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Activate a user
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> ActivateUser(int id)
    {
        try
        {
            var request = new UpdateUserRequest { IsActive = true };
            await _userService.UpdateUserAsync(id, request);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "User activated successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while activating the user",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Reset user password
    /// </summary>
    [HttpPost("{id}/reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(int id, [FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            await _userService.ResetPasswordAsync(id, resetPasswordDto.NewPassword);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Password reset successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while resetting the password",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Lock a user account
    /// </summary>
    [HttpPost("{id}/lock")]
    public async Task<ActionResult<ApiResponse<object>>> LockUser(int id)
    {
        try
        {
            await _userService.LockUserAsync(id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "User locked successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while locking the user",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Unlock a user account
    /// </summary>
    [HttpPost("{id}/unlock")]
    public async Task<ActionResult<ApiResponse<object>>> UnlockUser(int id)
    {
        try
        {
            await _userService.UnlockUserAsync(id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "User unlocked successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while unlocking the user",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get users by role
    /// </summary>
    [HttpGet("by-role/{roleId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetUsersByRole(int roleId)
    {
        try
        {
            var users = await _userService.GetUsersByRoleIdAsync(roleId);
            var userDtos = users.Select(u => MapToUserDto(u));

            return Ok(new ApiResponse<IEnumerable<UserDto>>
            {
                Success = true,
                Data = userDtos,
                Message = "Users retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<UserDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving users",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Check if username is available
    /// </summary>
    [HttpGet("check-username/{username}")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckUsernameAvailability(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Username cannot be empty"
                });
            }

            var isAvailable = await _userService.IsUsernameAvailableAsync(username);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = isAvailable,
                Message = isAvailable ? "Username is available" : "Username is already taken"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while checking username availability",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Check if email is available
    /// </summary>
    [HttpGet("check-email/{email}")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckEmailAvailability(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Email cannot be empty"
                });
            }

            var isAvailable = await _userService.IsEmailAvailableAsync(email);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = isAvailable,
                Message = isAvailable ? "Email is available" : "Email is already taken"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while checking email availability",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get current user's assigned project
    /// </summary>
    [HttpGet("my-project")]
    public async Task<ActionResult<ApiResponse<object>>> GetMyProject()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var user = await _context.Users
                .Include(u => u.Project)
                    .ThenInclude(p => p.Locations)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            if (user.Project == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No project assigned to user"
                });
            }

            // Apply location filtering
            var locations = user.Project.Locations.AsQueryable();
            var filteredLocations = await _accessControlService.ApplyLocationFilter(locations, userId.Value);

            var projectData = new
            {
                id = user.Project.Id,
                projectId = user.Project.ProjectId,
                name = user.Project.Name,
                preferredName = user.Project.PreferredName,
                spvName = user.Project.SpvName,
                code = user.Project.Code,
                description = user.Project.Description,
                states = user.Project.States,
                isActive = user.Project.IsActive,
                createdAt = user.Project.CreatedAt,
                locationCount = filteredLocations.Count(),
                userRole = user.Role.Name,
                accessLevel = new
                {
                    region = user.RestrictedRegion,
                    state = user.RestrictedState,
                    plaza = user.RestrictedPlaza,
                    office = user.RestrictedOffice,
                    level = !string.IsNullOrEmpty(user.RestrictedOffice) ? "Office" :
                           !string.IsNullOrEmpty(user.RestrictedPlaza) ? "Plaza" :
                           !string.IsNullOrEmpty(user.RestrictedState) ? "State" :
                           !string.IsNullOrEmpty(user.RestrictedRegion) ? "Region" : "Full Project Access"
                }
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = projectData,
                Message = "Project retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving project",
                Error = ex.Message
            });
        }
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? "Unknown",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            LastActivityAt = user.LastActivityAt,
            MustChangePassword = user.MustChangePassword,
            IsLocked = user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow,
            PasswordResetRequested = user.PasswordResetRequested,
            PasswordResetRequestedAt = user.PasswordResetRequestedAt,
            ProjectId = user.ProjectId,
            RestrictedRegion = user.RestrictedRegion,
            RestrictedState = user.RestrictedState,
            RestrictedPlaza = user.RestrictedPlaza,
            RestrictedOffice = user.RestrictedOffice
        };
    }
}