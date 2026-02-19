using Microsoft.AspNetCore.Mvc;
using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;
using ITAMS.Data;
using ITAMS.Models;
using ITAMS.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SuperAdminController : BaseController
{
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;
    private readonly ILocationService _locationService;
    private readonly IPermissionService _permissionService;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;
    private readonly ITAMSDbContext _context;
    private readonly IAccessControlService _accessControlService;

    public SuperAdminController(
        IUserService userService,
        IProjectService projectService,
        ILocationService locationService,
        IPermissionService permissionService,
        IRoleRepository roleRepository,
        IAuditService auditService,
        ITAMSDbContext context,
        IAccessControlService accessControlService)
    {
        _userService = userService;
        _projectService = projectService;
        _locationService = locationService;
        _permissionService = permissionService;
        _roleRepository = roleRepository;
        _auditService = auditService;
        _context = context;
        _accessControlService = accessControlService;
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
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "Unknown",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                LastActivityAt = u.LastActivityAt,
                MustChangePassword = u.MustChangePassword,
                IsLocked = u.LockedUntil.HasValue && u.LockedUntil > DateTime.UtcNow,
                PasswordResetRequested = u.PasswordResetRequested,
                PasswordResetRequestedAt = u.PasswordResetRequestedAt,
                ProjectId = u.ProjectId,
                RestrictedRegion = u.RestrictedRegion,
                RestrictedState = u.RestrictedState,
                RestrictedPlaza = u.RestrictedPlaza,
                RestrictedOffice = u.RestrictedOffice
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
                ProjectId = user.ProjectId,
                RestrictedRegion = user.RestrictedRegion,
                RestrictedState = user.RestrictedState,
                RestrictedPlaza = user.RestrictedPlaza,
                RestrictedOffice = user.RestrictedOffice
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
                MustChangePassword = createUserDto.MustChangePassword,
                ProjectId = createUserDto.ProjectId,
                RestrictedRegion = createUserDto.RestrictedRegion,
                RestrictedState = createUserDto.RestrictedState,
                RestrictedPlaza = createUserDto.RestrictedPlaza,
                RestrictedOffice = createUserDto.RestrictedOffice
            };

            var user = await _userService.CreateUserAsync(request);

            var userDto = new UserDto
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
                IsLocked = false,
                ProjectId = user.ProjectId,
                RestrictedRegion = user.RestrictedRegion,
                RestrictedState = user.RestrictedState,
                RestrictedPlaza = user.RestrictedPlaza,
                RestrictedOffice = user.RestrictedOffice
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
                IsActive = updateUserDto.IsActive,
                ProjectId = updateUserDto.ProjectId,
                RestrictedRegion = updateUserDto.RestrictedRegion,
                RestrictedState = updateUserDto.RestrictedState,
                RestrictedPlaza = updateUserDto.RestrictedPlaza,
                RestrictedOffice = updateUserDto.RestrictedOffice
            };

            var user = await _userService.UpdateUserAsync(id, request);

            var userDto = new UserDto
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
                ProjectId = user.ProjectId,
                RestrictedRegion = user.RestrictedRegion,
                RestrictedState = user.RestrictedState,
                RestrictedPlaza = user.RestrictedPlaza,
                RestrictedOffice = user.RestrictedOffice
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
                ProjectId = p.ProjectId,
                Name = p.Name,
                PreferredName = p.PreferredName,
                SpvName = p.SpvName,
                Description = p.Description,
                Code = p.Code,
                States = p.States,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                LocationCount = p.Locations.Count,
                UserCount = p.Users.Count(u => u.IsActive)
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
                PreferredName = createProjectDto.PreferredName,
                SpvName = createProjectDto.SpvName,
                Description = createProjectDto.Description ?? string.Empty,
                Code = createProjectDto.Code,
                States = createProjectDto.States
            };

            var project = await _projectService.CreateProjectAsync(request);

            var projectDto = new ProjectSummaryDto
            {
                Id = project.Id,
                Name = project.Name,
                PreferredName = project.PreferredName,
                SpvName = project.SpvName,
                Description = project.Description,
                Code = project.Code,
                States = project.States,
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

    [HttpPut("projects/{id}")]
    public async Task<ActionResult<ProjectSummaryDto>> UpdateProject(int id, [FromBody] UpdateProjectDto updateProjectDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new UpdateProjectRequest
            {
                Name = updateProjectDto.Name,
                Description = updateProjectDto.Description,
                IsActive = updateProjectDto.IsActive
            };

            var project = await _projectService.UpdateProjectAsync(id, request);

            var projectDto = new ProjectSummaryDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Code = project.Code,
                IsActive = project.IsActive,
                CreatedAt = project.CreatedAt,
                LocationCount = project.Locations.Count,
                UserCount = project.Users.Count(u => u.IsActive)
            };

            return Ok(projectDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the project", error = ex.Message });
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
            // For SuperAdmin, return all locations without access control filtering
            var locations = await _locationService.GetAllLocationsAsync();
            
            var locationDtos = locations.Select(l => new LocationSummaryDto
            {
                Id = l.Id,
                Name = l.Name,
                Region = l.Region,
                State = l.State,
                Site = l.Site,
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
            // For SuperAdmin, return all locations for the project without access control filtering
            var locations = await _locationService.GetLocationsByProjectAsync(projectId);
            
            var locationDtos = locations.Select(l => new LocationSummaryDto
            {
                Id = l.Id,
                Name = l.Name,
                Region = l.Region,
                State = l.State,
                Site = l.Site,
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
                Site = createLocationDto.Site,
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
                Site = location.Site,
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

    [HttpPut("locations/{id}")]
    public async Task<ActionResult<LocationSummaryDto>> UpdateLocation(int id, [FromBody] UpdateLocationDto updateLocationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new UpdateLocationRequest
            {
                Name = updateLocationDto.Name,
                Region = updateLocationDto.Region,
                State = updateLocationDto.State,
                Site = updateLocationDto.Site,
                Lane = updateLocationDto.Lane,
                Office = updateLocationDto.Office,
                Address = updateLocationDto.Address,
                IsActive = updateLocationDto.IsActive
            };

            var location = await _locationService.UpdateLocationAsync(id, request);

            var locationDto = new LocationSummaryDto
            {
                Id = location.Id,
                Name = location.Name,
                Region = location.Region,
                State = location.State,
                Site = location.Site,
                Lane = location.Lane,
                Office = location.Office,
                Address = location.Address,
                IsActive = location.IsActive,
                ProjectId = location.ProjectId,
                AssetCount = location.Assets.Count
            };

            return Ok(locationDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the location", error = ex.Message });
        }
    }

    [HttpDelete("locations/{id}")]
    public async Task<ActionResult> DeleteLocation(int id)
    {
        try
        {
            await _locationService.DeleteLocationAsync(id);
            return Ok(new { message = "Location deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the location", error = ex.Message });
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
                UserCount = p.Users.Count(u => u.IsActive)
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


    [HttpGet("login-audit")]
    public async Task<ActionResult<IEnumerable<LoginAuditDto>>> GetLoginAudit(
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageSize = 100)
    {
        try
        {
            var query = _context.LoginAudits.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(la => la.UserId == userId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(la => la.LoginTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(la => la.LoginTime <= toDate.Value);
            }

            var loginAudits = await query
                .OrderByDescending(la => la.LoginTime)
                .Take(pageSize)
                .Select(la => new LoginAuditDto
                {
                    Id = la.Id,
                    UserId = la.UserId,
                    Username = la.Username,
                    LoginTime = la.LoginTime,
                    LogoutTime = la.LogoutTime,
                    IpAddress = la.IpAddress,
                    BrowserType = la.BrowserType,
                    OperatingSystem = la.OperatingSystem,
                    Status = la.Status
                })
                .ToListAsync();

            return Ok(loginAudits);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving login audit", error = ex.Message });
        }
    }
    #endregion
}
