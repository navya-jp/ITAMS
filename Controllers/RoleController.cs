using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ITAMS.Services.RBAC;
using ITAMS.Domain.Entities.RBAC;
using ITAMS.Models;
using System.Security.Claims;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/rbac/[controller]")]
[Authorize] // Require authentication for all endpoints
public class RoleController : ControllerBase
{
    private readonly IRoleManagementService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IRoleManagementService roleService,
        ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles with their permissions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            var roleResponses = roles.Select(r => new RoleResponse
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedByUser?.Username ?? "System",
                DeactivatedAt = r.DeactivatedAt,
                DeactivatedBy = r.DeactivatedByUser?.Username,
                PermissionCount = r.RolePermissions?.Count(rp => rp.Status == RolePermissionStatus.Active && rp.Allowed) ?? 0
            }).ToList();

            return Ok(new ApiResponse<List<RoleResponse>>
            {
                Success = true,
                Data = roleResponses,
                Message = $"Retrieved {roleResponses.Count} roles"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving roles",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get a specific role by ID
    /// </summary>
    [HttpGet("{roleId}")]
    public async Task<IActionResult> GetRole(int roleId)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Role with ID {roleId} not found"
                });
            }

            var roleResponse = new RoleDetailResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                Status = role.Status,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedByUser?.Username ?? "System",
                DeactivatedAt = role.DeactivatedAt,
                DeactivatedBy = role.DeactivatedByUser?.Username,
                Permissions = role.RolePermissions?
                    .Where(rp => rp.Status == RolePermissionStatus.Active)
                    .Select(rp => new RolePermissionInfo
                    {
                        PermissionId = rp.PermissionId,
                        PermissionCode = rp.Permission.PermissionCode,
                        Module = rp.Permission.Module,
                        Description = rp.Permission.Description,
                        Allowed = rp.Allowed,
                        GrantedAt = rp.GrantedAt,
                        GrantedBy = rp.GrantedByUser?.Username ?? "System"
                    }).ToList() ?? new List<RolePermissionInfo>()
            };

            return Ok(new ApiResponse<RoleDetailResponse>
            {
                Success = true,
                Data = roleResponse,
                Message = "Role retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", roleId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the role",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var role = await _roleService.CreateRoleAsync(request, currentUserId);

            var roleResponse = new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                Status = role.Status,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedByUser?.Username ?? "System",
                PermissionCount = 0
            };

            _logger.LogInformation("Role '{RoleName}' created successfully by user {UserId}", role.RoleName, currentUserId);

            return CreatedAtAction(
                nameof(GetRole),
                new { roleId = role.RoleId },
                new ApiResponse<RoleResponse>
                {
                    Success = true,
                    Data = roleResponse,
                    Message = "Role created successfully"
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating role");
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while creating the role",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{roleId}")]
    public async Task<IActionResult> UpdateRole(int roleId, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var role = await _roleService.UpdateRoleAsync(roleId, request, currentUserId);

            var roleResponse = new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                Status = role.Status,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedByUser?.Username ?? "System",
                DeactivatedAt = role.DeactivatedAt,
                DeactivatedBy = role.DeactivatedByUser?.Username,
                PermissionCount = role.RolePermissions?.Count(rp => rp.Status == RolePermissionStatus.Active && rp.Allowed) ?? 0
            };

            _logger.LogInformation("Role '{RoleName}' updated successfully by user {UserId}", role.RoleName, currentUserId);

            return Ok(new ApiResponse<RoleResponse>
            {
                Success = true,
                Data = roleResponse,
                Message = "Role updated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while updating role {RoleId}", roleId);
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating role {RoleId}", roleId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", roleId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the role",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Deactivate a role
    /// </summary>
    [HttpPatch("{roleId}/deactivate")]
    public async Task<IActionResult> DeactivateRole(int roleId, [FromBody] DeactivateRoleRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Reason is required for role deactivation"
                });
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            await _roleService.DeactivateRoleAsync(roleId, currentUserId, request.Reason);

            _logger.LogInformation("Role {RoleId} deactivated by user {UserId}. Reason: {Reason}", 
                roleId, currentUserId, request.Reason);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role deactivated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while deactivating role {RoleId}", roleId);
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while deactivating role {RoleId}", roleId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating role {RoleId}", roleId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deactivating the role",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Reactivate a role
    /// </summary>
    [HttpPatch("{roleId}/reactivate")]
    public async Task<IActionResult> ReactivateRole(int roleId, [FromBody] ReactivateRoleRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Reason is required for role reactivation"
                });
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            await _roleService.ReactivateRoleAsync(roleId, currentUserId, request.Reason);

            _logger.LogInformation("Role {RoleId} reactivated by user {UserId}. Reason: {Reason}", 
                roleId, currentUserId, request.Reason);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role reactivated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while reactivating role {RoleId}", roleId);
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while reactivating role {RoleId}", roleId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating role {RoleId}", roleId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while reactivating the role",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get role permission matrix
    /// </summary>
    [HttpGet("{roleId}/permissions/matrix")]
    public async Task<IActionResult> GetRolePermissionMatrix(int roleId)
    {
        try
        {
            var matrix = await _roleService.GetRolePermissionMatrixAsync(roleId);

            return Ok(new ApiResponse<RolePermissionMatrix>
            {
                Success = true,
                Data = matrix,
                Message = "Role permission matrix retrieved successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while retrieving role permission matrix for role {RoleId}", roleId);
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role permission matrix for role {RoleId}", roleId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the role permission matrix",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Update role permissions in bulk
    /// </summary>
    [HttpPut("{roleId}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsRequest request)
    {
        try
        {
            if (!ModelState.IsValid || request.Updates == null || !request.Updates.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data or no permission updates provided"
                });
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            await _roleService.UpdateRolePermissionsAsync(roleId, request.Updates, currentUserId);

            _logger.LogInformation("Updated {Count} permissions for role {RoleId} by user {UserId}", 
                request.Updates.Count, roleId, currentUserId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Successfully updated {request.Updates.Count} role permissions"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while updating role permissions for role {RoleId}", roleId);
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating role permissions for role {RoleId}", roleId);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role permissions for role {RoleId}", roleId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating role permissions",
                Error = ex.Message
            });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
    }
}

// DTOs for API requests and responses
public class RoleResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? DeactivatedAt { get; set; }
    public string? DeactivatedBy { get; set; }
    public int PermissionCount { get; set; }
}

public class RoleDetailResponse : RoleResponse
{
    public List<RolePermissionInfo> Permissions { get; set; } = new();
}

public class RolePermissionInfo
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Allowed { get; set; }
    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
}

public class DeactivateRoleRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ReactivateRoleRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class UpdateRolePermissionsRequest
{
    public List<RolePermissionUpdate> Updates { get; set; } = new();
}