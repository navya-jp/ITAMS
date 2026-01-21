using Microsoft.AspNetCore.Mvc;
using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;
using ITAMS.Data;
using System.ComponentModel.DataAnnotations;

namespace ITAMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SuperAdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;
    private readonly ILocationService _locationService;
    private readonly IPermissionService _permissionService;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;
    private readonly ITAMSDbContext _context;

    public SuperAdminController(
        IUserService userService,
        IProjectService projectService,
        ILocationService locationService,
        IPermissionService permissionService,
        IRoleRepository roleRepository,
        IAuditService auditService,
        ITAMSDbContext context)
    {
        _userService = userService;
        _projectService = projectService;
        _locationService = locationService;
        _permissionService = permissionService;
        _roleRepository = roleRepository;
        _auditService = auditService;
        _context = context;
    }

    #region Users

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "Unknown",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                MustChangePassword = u.MustChangePassword,
                IsLocked = u.LockedUntil.HasValue && u.LockedUntil > DateTime.UtcNow
            });

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving users", error = ex.Message });
        }
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "Unknown",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                MustChangePassword = user.MustChangePassword,
                IsLocked = user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the user", error = ex.Message });
        }
    }

    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "Unknown",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                MustChangePassword = user.MustChangePassword,
                IsLocked = false
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the user", error = ex.Message });
        }
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "Unknown",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                MustChangePassword = user.MustChangePassword,
                IsLocked = user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow
            };

            return Ok(userDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the user", error = ex.Message });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the user", error = ex.Message });
        }
    }

    [HttpPost("users/{id}/reset-password")]
    public async Task<ActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.ResetPasswordAsync(id, resetPasswordDto.NewPassword);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while resetting the password", error = ex.Message });
        }
    }

    [HttpPost("users/{id}/lock")]
    public async Task<ActionResult> LockUser(int id)
    {
        try
        {
            await _userService.LockUserAsync(id);
            return Ok(new { message = "User locked successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while locking the user", error = ex.Message });
        }
    }

    [HttpPost("users/{id}/unlock")]
    public async Task<ActionResult> UnlockUser(int id)
    {
        try
        {
            await _userService.UnlockUserAsync(id);
            return Ok(new { message = "User unlocked successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while unlocking the user", error = ex.Message });
        }
    }

    #endregion

    #region Roles

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllRoles()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            var roleData = roles.Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.IsSystemRole,
                r.IsActive,
                r.CreatedAt
            });
            return Ok(roleData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving roles", error = ex.Message });
        }
    }

    [HttpGet("roles/{id}")]
    public async Task<ActionResult<object>> GetRole(int id)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            var roleData = new
            {
                role.Id,
                role.Name,
                role.Description,
                role.IsSystemRole,
                role.IsActive,
                role.CreatedAt
            };

            return Ok(roleData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the role", error = ex.Message });
        }
    }

    [HttpPost("roles")]
    public async Task<ActionResult<object>> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                IsSystemRole = createRoleDto.IsSystemRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("ROLE_CREATED", "Role", role.Id.ToString(), 1, "superadmin");

            var roleData = new
            {
                role.Id,
                role.Name,
                role.Description,
                role.IsSystemRole,
                role.IsActive,
                role.CreatedAt
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, roleData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the role", error = ex.Message });
        }
    }

    [HttpPut("roles/{id}")]
    public async Task<ActionResult<object>> UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            if (role.IsSystemRole && !updateRoleDto.IsActive.GetValueOrDefault(true))
            {
                return BadRequest(new { message = "System roles cannot be deactivated" });
            }

            role.Name = updateRoleDto.Name ?? role.Name;
            role.Description = updateRoleDto.Description ?? role.Description;
            role.IsActive = updateRoleDto.IsActive ?? role.IsActive;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("ROLE_UPDATED", "Role", role.Id.ToString(), 1, "superadmin");

            var roleData = new
            {
                role.Id,
                role.Name,
                role.Description,
                role.IsSystemRole,
                role.IsActive,
                role.CreatedAt
            };

            return Ok(roleData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the role", error = ex.Message });
        }
    }

    [HttpGet("roles/{roleId}/permissions")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetRolePermissions(int roleId)
    {
        try
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving role permissions", error = ex.Message });
        }
    }

    [HttpPut("roles/{roleId}/permissions")]
    public async Task<ActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _permissionService.UpdateRolePermissionsAsync(roleId, updateDto.PermissionIds);
            return Ok(new { message = "Role permissions updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating role permissions", error = ex.Message });
        }
    }

    #endregion

    #region Projects

    [HttpGet("projects")]
    public async Task<ActionResult<IEnumerable<ProjectSummaryDto>>> GetAllProjects()
    {
        try
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var projectDtos = projects.Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Code = p.Code,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                LocationCount = p.Locations.Count,
                UserCount = p.UserProjects.Count(up => up.IsActive)
            });

            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving projects", error = ex.Message });
        }
    }

    [HttpPost("projects")]
    public async Task<ActionResult<ProjectSummaryDto>> CreateProject([FromBody] CreateProjectDto createProjectDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new CreateProjectRequest
            {
                Name = createProjectDto.Name,
                Description = createProjectDto.Description ?? string.Empty,
                Code = createProjectDto.Code
            };

            var project = await _projectService.CreateProjectAsync(request);

            var projectDto = new ProjectSummaryDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Code = project.Code,
                IsActive = project.IsActive,
                CreatedAt = project.CreatedAt,
                LocationCount = 0,
                UserCount = 0
            };

            return CreatedAtAction(nameof(GetAllProjects), new { id = project.Id }, projectDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the project", error = ex.Message });
        }
    }

    [HttpDelete("projects/{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        try
        {
            await _projectService.DeleteProjectAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the project", error = ex.Message });
        }
    }

    #endregion

    #region Locations

    [HttpGet("locations")]
    public async Task<ActionResult<IEnumerable<LocationSummaryDto>>> GetAllLocations()
    {
        try
        {
            var locations = await _locationService.GetAllLocationsAsync();
            var locationDtos = locations.Select(l => new LocationSummaryDto
            {
                Id = l.Id,
                Name = l.Name,
                Region = l.Region,
                State = l.State,
                Plaza = l.Plaza,
                Lane = l.Lane,
                Office = l.Office,
                Address = l.Address,
                IsActive = l.IsActive,
                ProjectId = l.ProjectId,
                AssetCount = l.Assets.Count
            });

            return Ok(locationDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving locations", error = ex.Message });
        }
    }

    [HttpGet("projects/{projectId}/locations")]
    public async Task<ActionResult<IEnumerable<LocationSummaryDto>>> GetProjectLocations(int projectId)
    {
        try
        {
            var locations = await _locationService.GetLocationsByProjectAsync(projectId);
            var locationDtos = locations.Select(l => new LocationSummaryDto
            {
                Id = l.Id,
                Name = l.Name,
                Region = l.Region,
                State = l.State,
                Plaza = l.Plaza,
                Lane = l.Lane,
                Office = l.Office,
                Address = l.Address,
                IsActive = l.IsActive,
                ProjectId = l.ProjectId,
                AssetCount = l.Assets.Count
            });

            return Ok(locationDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving locations", error = ex.Message });
        }
    }

    [HttpPost("locations")]
    public async Task<ActionResult<LocationSummaryDto>> CreateLocation([FromBody] CreateLocationDto createLocationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new CreateLocationRequest
            {
                Name = createLocationDto.Name,
                Region = createLocationDto.Region,
                State = createLocationDto.State,
                Plaza = createLocationDto.Plaza,
                Lane = createLocationDto.Lane,
                Office = createLocationDto.Office,
                Address = createLocationDto.Address,
                ProjectId = createLocationDto.ProjectId
            };

            var location = await _locationService.CreateLocationAsync(request);

            var locationDto = new LocationSummaryDto
            {
                Id = location.Id,
                Name = location.Name,
                Region = location.Region,
                State = location.State,
                Plaza = location.Plaza,
                Lane = location.Lane,
                Office = location.Office,
                Address = location.Address,
                IsActive = location.IsActive,
                ProjectId = location.ProjectId,
                AssetCount = 0
            };

            return CreatedAtAction(nameof(GetProjectLocations), new { projectId = location.ProjectId }, locationDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the location", error = ex.Message });
        }
    }

    #endregion

    #region User Project Assignments

    [HttpPost("users/{userId}/projects/{projectId}/assign")]
    public async Task<ActionResult> AssignUserToProject(int userId, int projectId, [FromBody] AssignUserProjectDto assignDto)
    {
        try
        {
            await _projectService.AssignUserToProjectAsync(userId, projectId, assignDto.PermissionIds, 1); // TODO: Get current user ID
            return Ok(new { message = "User assigned to project successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while assigning user to project", error = ex.Message });
        }
    }

    [HttpDelete("users/{userId}/projects/{projectId}")]
    public async Task<ActionResult> RemoveUserFromProject(int userId, int projectId)
    {
        try
        {
            await _projectService.RemoveUserFromProjectAsync(userId, projectId);
            return Ok(new { message = "User removed from project successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while removing user from project", error = ex.Message });
        }
    }

    [HttpGet("users/{userId}/projects")]
    public async Task<ActionResult<IEnumerable<ProjectSummaryDto>>> GetUserProjects(int userId)
    {
        try
        {
            var projects = await _projectService.GetUserProjectsAsync(userId);
            var projectDtos = projects.Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Code = p.Code,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                LocationCount = p.Locations.Count,
                UserCount = p.UserProjects.Count(up => up.IsActive)
            });

            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user projects", error = ex.Message });
        }
    }

    #endregion

    #region Permissions

    [HttpGet("permissions")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetAllPermissions()
    {
        try
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving permissions", error = ex.Message });
        }
    }

    [HttpGet("permissions/modules/{module}")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetPermissionsByModule(string module)
    {
        try
        {
            var permissions = await _permissionService.GetPermissionsByModuleAsync(module);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving permissions", error = ex.Message });
        }
    }

    #endregion

    #region Audit Trail

    [HttpGet("audit-trail")]
    public async Task<ActionResult<IEnumerable<AuditEntryDto>>> GetAuditTrail(
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var auditEntries = await _auditService.GetAuditTrailAsync(entityType, entityId, userId, fromDate, toDate);
            var auditDtos = auditEntries.Select(a => new AuditEntryDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                Timestamp = a.Timestamp,
                UserName = a.UserName,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent
            });

            return Ok(auditDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving audit trail", error = ex.Message });
        }
    }

    #endregion
}

// DTOs
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsLocked { get; set; }
}

public class CreateUserDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid role")]
    public int RoleId { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
    public string Password { get; set; } = string.Empty;
    
    public bool MustChangePassword { get; set; } = true;
}

public class CreateProjectDto
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Project name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Project code is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Project code must be between 2 and 50 characters")]
    [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Project code can only contain uppercase letters, numbers, hyphens, and underscores")]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class CreateLocationDto
{
    [Required(ErrorMessage = "Location name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Location name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Project is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid project")]
    public int ProjectId { get; set; }
    
    [Required(ErrorMessage = "Region is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Region must be between 2 and 100 characters")]
    public string Region { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "State is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "State must be between 2 and 100 characters")]
    public string State { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Plaza cannot exceed 100 characters")]
    public string? Plaza { get; set; }
    
    [StringLength(100, ErrorMessage = "Lane cannot exceed 100 characters")]
    public string? Lane { get; set; }
    
    [StringLength(100, ErrorMessage = "Office cannot exceed 100 characters")]
    public string? Office { get; set; }
    
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
}

public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
}

public class ProjectSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LocationCount { get; set; }
    public int UserCount { get; set; }
}

public class LocationSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Plaza { get; set; }
    public string? Lane { get; set; }
    public string? Office { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public int ProjectId { get; set; }
    public int AssetCount { get; set; }
}

public class AssignUserProjectDto
{
    public int[] PermissionIds { get; set; } = Array.Empty<int>();
    public int AssignedBy { get; set; }
}

public class UpdateProjectDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
}

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Plaza { get; set; }
    public string? Lane { get; set; }
    public string? Office { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int AssetCount { get; set; }
}

public class CreateRoleDto
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool IsSystemRole { get; set; } = false;
}

public class UpdateRoleDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters")]
    public string? Name { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool? IsActive { get; set; }
}

public class UpdateRolePermissionsDto
{
    [Required]
    public int[] PermissionIds { get; set; } = Array.Empty<int>();
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class AuditEntryDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}