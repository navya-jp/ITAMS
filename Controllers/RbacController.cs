using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAMS.Data;
using ITAMS.Domain.Entities.RBAC;

namespace ITAMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RbacController : ControllerBase
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<RbacController> _logger;

    public RbacController(ITAMSDbContext context, ILogger<RbacController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Get all RBAC roles
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<object>>> GetRbacRoles()
    {
        try
        {
            // Query the RBAC tables directly using raw SQL since EF entities are disabled
            var roles = await _context.Database.SqlQueryRaw<RbacRoleDto>(@"
                SELECT 
                    Id as RoleId,
                    Name as RoleName,
                    Description,
                    IsSystemRole,
                    Status,
                    CreatedAt
                FROM RbacRoles 
                WHERE Status = 'ACTIVE'
                ORDER BY Name
            ").ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving RBAC roles");
            return StatusCode(500, new { message = "An error occurred while retrieving roles", error = ex.Message });
        }
    }

    // Get all RBAC permissions
    [HttpGet("permissions")]
    public async Task<ActionResult<IEnumerable<object>>> GetRbacPermissions()
    {
        try
        {
            var permissions = await _context.Database.SqlQueryRaw<RbacPermissionDto>(@"
                SELECT 
                    Id as PermissionId,
                    Code as PermissionCode,
                    Module,
                    Description,
                    ResourceType,
                    Action,
                    Status
                FROM RbacPermissions 
                WHERE Status = 'ACTIVE'
                ORDER BY Module, Code
            ").ToListAsync();

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving RBAC permissions");
            return StatusCode(500, new { message = "An error occurred while retrieving permissions", error = ex.Message });
        }
    }

    // Get permissions for a specific role
    [HttpGet("roles/{roleId}/permissions")]
    public async Task<ActionResult<IEnumerable<object>>> GetRolePermissions(int roleId)
    {
        try
        {
            var permissions = await _context.Database.SqlQueryRaw<RbacPermissionDto>(@"
                SELECT 
                    p.Id as PermissionId,
                    p.Code as PermissionCode,
                    p.Module,
                    p.Description,
                    p.ResourceType,
                    p.Action,
                    p.Status
                FROM RbacPermissions p
                INNER JOIN RbacRolePermissions rp ON p.Id = rp.PermissionId
                WHERE rp.RoleId = {0} 
                  AND rp.Status = 'ACTIVE' 
                  AND rp.Allowed = 1
                  AND p.Status = 'ACTIVE'
                ORDER BY p.Module, p.Code
            ", roleId).ToListAsync();

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role permissions for role {RoleId}", roleId);
            return StatusCode(500, new { message = "An error occurred while retrieving role permissions", error = ex.Message });
        }
    }

    // Update role permissions
    [HttpPut("roles/{roleId}/permissions")]
    public async Task<ActionResult> UpdateRolePermissions(int roleId, [FromBody] RbacUpdateRolePermissionsRequest request)
    {
        try
        {
            _logger.LogInformation("Updating permissions for role {RoleId} with {Count} permissions", roleId, request.PermissionIds.Count);

            // Get all existing active permissions for this role
            var existingPermissionsSql = @"
                SELECT PermissionId 
                FROM RbacRolePermissions 
                WHERE RoleId = @p0 AND Status = 'ACTIVE'";
            
            var existingCommand = _context.Database.GetDbConnection().CreateCommand();
            existingCommand.CommandText = existingPermissionsSql;
            existingCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", roleId));
            
            await _context.Database.OpenConnectionAsync();
            var reader = await existingCommand.ExecuteReaderAsync();
            var existingPermissions = new List<int>();
            while (await reader.ReadAsync())
            {
                existingPermissions.Add(reader.GetInt32(0));
            }
            await reader.CloseAsync();
            await _context.Database.CloseConnectionAsync();

            // Find permissions to revoke (existing but not in new list)
            var permissionsToRevoke = existingPermissions.Except(request.PermissionIds).ToList();
            
            // Find permissions to add (in new list but not existing)
            var permissionsToAdd = request.PermissionIds.Except(existingPermissions).ToList();

            _logger.LogInformation("Role {RoleId}: Revoking {RevokeCount} permissions, Adding {AddCount} permissions", 
                roleId, permissionsToRevoke.Count, permissionsToAdd.Count);

            // Revoke permissions that are no longer needed
            foreach (var permissionId in permissionsToRevoke)
            {
                var revokeSql = @"
                    UPDATE rp
                    SET rp.Status = 'REVOKED', rp.RevokedAt = GETUTCDATE(), rp.RevokedBy = 1, rp.RevokedByRef = u.UserId
                    FROM RbacRolePermissions rp, Users u
                    WHERE rp.RoleId = @p0 AND rp.PermissionId = @p1 AND rp.Status = 'ACTIVE' AND u.Id = 1";
                
                await _context.Database.ExecuteSqlRawAsync(revokeSql, roleId, permissionId);
            }

            // Add new permissions
            foreach (var permissionId in permissionsToAdd)
            {
                // Check if this role-permission combination existed before (was revoked)
                var checkSql = @"
                    SELECT COUNT(*) 
                    FROM RbacRolePermissions 
                    WHERE RoleId = @p0 AND PermissionId = @p1";
                
                var checkCommand = _context.Database.GetDbConnection().CreateCommand();
                checkCommand.CommandText = checkSql;
                checkCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", roleId));
                checkCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p1", permissionId));
                
                await _context.Database.OpenConnectionAsync();
                var count = (int)await checkCommand.ExecuteScalarAsync();
                await _context.Database.CloseConnectionAsync();

                if (count > 0)
                {
                    // Reactivate existing record - include GrantedByRef
                    var reactivateSql = @"
                        UPDATE rp
                        SET rp.Status = 'ACTIVE', rp.GrantedAt = GETUTCDATE(), rp.GrantedBy = 1, rp.GrantedByRef = u.UserId, rp.RevokedAt = NULL, rp.RevokedBy = NULL, rp.RevokedByRef = NULL
                        FROM RbacRolePermissions rp, Users u
                        WHERE rp.RoleId = @p0 AND rp.PermissionId = @p1 AND u.Id = 1";
                    
                    await _context.Database.ExecuteSqlRawAsync(reactivateSql, roleId, permissionId);
                }
                else
                {
                    // Insert new record - lookup alternate keys for RoleIdRef, PermissionIdRef, and GrantedByRef
                    var insertSql = @"
                        INSERT INTO RbacRolePermissions (RoleId, PermissionId, RoleIdRef, PermissionIdRef, GrantedBy, GrantedByRef, Allowed, GrantedAt, Status)
                        SELECT @p0, @p1, r.RbacRoleId, p.RbacPermissionId, 1, u.UserId, 1, GETUTCDATE(), 'ACTIVE'
                        FROM RbacRoles r, RbacPermissions p, Users u
                        WHERE r.Id = @p0 AND p.Id = @p1 AND u.Id = 1";
                    
                    await _context.Database.ExecuteSqlRawAsync(insertSql, roleId, permissionId);
                }
            }

            _logger.LogInformation("Successfully updated permissions for role {RoleId}", roleId);
            return Ok(new { message = "Role permissions updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role permissions for role {RoleId}", roleId);
            return StatusCode(500, new { message = "An error occurred while updating role permissions", error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // Get permissions grouped by module
    [HttpGet("permissions/grouped")]
    public async Task<ActionResult<IEnumerable<object>>> GetPermissionsGrouped()
    {
        try
        {
            var permissions = await _context.Database.SqlQueryRaw<RbacPermissionDto>(@"
                SELECT 
                    Id as PermissionId,
                    Code as PermissionCode,
                    Module,
                    Description,
                    ResourceType,
                    Action,
                    Status
                FROM RbacPermissions 
                WHERE Status = 'ACTIVE'
                ORDER BY Module, Code
            ").ToListAsync();

            var grouped = permissions
                .GroupBy(p => p.Module)
                .Select(g => new
                {
                    Module = g.Key,
                    Permissions = g.ToList()
                })
                .OrderBy(g => g.Module)
                .ToList();

            return Ok(grouped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving grouped permissions");
            return StatusCode(500, new { message = "An error occurred while retrieving grouped permissions", error = ex.Message });
        }
    }
}

// DTOs for raw SQL queries
public class RbacRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RbacPermissionDto
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class RbacUpdateRolePermissionsRequest
{
    public List<int> PermissionIds { get; set; } = new List<int>();
}