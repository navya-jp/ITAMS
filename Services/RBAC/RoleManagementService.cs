using ITAMS.Domain.Entities;
using ITAMS.Domain.Entities.RBAC;
using ITAMS.Domain.Interfaces;
using ITAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services.RBAC;

public interface IRoleManagementService
{
    Task<List<RbacRole>> GetAllRolesAsync();
    Task<RbacRole?> GetRoleByIdAsync(int roleId);
    Task<RbacRole> CreateRoleAsync(CreateRoleRequest request, int createdBy);
    Task<RbacRole> UpdateRoleAsync(int roleId, UpdateRoleRequest request, int updatedBy);
    Task DeactivateRoleAsync(int roleId, int deactivatedBy, string reason);
    Task ReactivateRoleAsync(int roleId, int reactivatedBy, string reason);
    Task<List<RbacRolePermission>> GetRolePermissionsAsync(int roleId);
    Task UpdateRolePermissionsAsync(int roleId, List<RolePermissionUpdate> updates, int updatedBy);
    Task<RolePermissionMatrix> GetRolePermissionMatrixAsync(int roleId);
}

public class RoleManagementService : IRoleManagementService
{
    private readonly ITAMSDbContext _context;
    private readonly IRbacAuditService _auditService;
    private readonly ILogger<RoleManagementService> _logger;

    public RoleManagementService(
        ITAMSDbContext context,
        IRbacAuditService auditService,
        ILogger<RoleManagementService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<List<RbacRole>> GetAllRolesAsync()
    {
        return await _context.Set<RbacRole>()
            .Include(r => r.CreatedByUser)
            .Include(r => r.DeactivatedByUser)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
    }

    public async Task<RbacRole?> GetRoleByIdAsync(int roleId)
    {
        return await _context.Set<RbacRole>()
            .Include(r => r.CreatedByUser)
            .Include(r => r.DeactivatedByUser)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<RbacRole> CreateRoleAsync(CreateRoleRequest request, int createdBy)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.RoleName))
            throw new ArgumentException("Role name is required");

        // Check if role name already exists
        var existingRole = await _context.Set<RbacRole>()
            .FirstOrDefaultAsync(r => r.RoleName == request.RoleName);

        if (existingRole != null)
            throw new InvalidOperationException($"Role with name '{request.RoleName}' already exists");

        var role = new RbacRole
        {
            RoleName = request.RoleName,
            Description = request.Description,
            IsSystemRole = false, // Only system can create system roles
            Status = RbacRoleStatus.Active,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<RbacRole>().Add(role);
        await _context.SaveChangesAsync();

        // Log the creation
        await _auditService.LogPermissionChangeAsync(new PermissionChangeLog
        {
            ActorUserId = createdBy,
            RoleId = role.RoleId,
            ActionType = AuditActionTypes.RoleCreate,
            EntityType = AuditEntityTypes.Role,
            NewValue = role.RoleName,
            Reason = $"Created new role: {role.RoleName}"
        });

        _logger.LogInformation("Role '{RoleName}' created by user {UserId}", role.RoleName, createdBy);

        return role;
    }

    public async Task<RbacRole> UpdateRoleAsync(int roleId, UpdateRoleRequest request, int updatedBy)
    {
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
            throw new ArgumentException($"Role with ID {roleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be modified");

        var oldValue = $"Name: {role.RoleName}, Description: {role.Description}";

        // Update properties
        if (!string.IsNullOrWhiteSpace(request.RoleName) && request.RoleName != role.RoleName)
        {
            // Check if new name already exists
            var existingRole = await _context.Set<RbacRole>()
                .FirstOrDefaultAsync(r => r.RoleName == request.RoleName && r.RoleId != roleId);

            if (existingRole != null)
                throw new InvalidOperationException($"Role with name '{request.RoleName}' already exists");

            role.RoleName = request.RoleName;
        }

        if (request.Description != null)
            role.Description = request.Description;

        await _context.SaveChangesAsync();

        var newValue = $"Name: {role.RoleName}, Description: {role.Description}";

        // Log the update
        await _auditService.LogPermissionChangeAsync(new PermissionChangeLog
        {
            ActorUserId = updatedBy,
            RoleId = role.RoleId,
            ActionType = AuditActionTypes.RoleUpdate,
            EntityType = AuditEntityTypes.Role,
            OldValue = oldValue,
            NewValue = newValue,
            Reason = $"Updated role: {role.RoleName}"
        });

        _logger.LogInformation("Role '{RoleName}' updated by user {UserId}", role.RoleName, updatedBy);

        return role;
    }

    public async Task DeactivateRoleAsync(int roleId, int deactivatedBy, string reason)
    {
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
            throw new ArgumentException($"Role with ID {roleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be deactivated");

        if (role.Status == RbacRoleStatus.Inactive)
            throw new InvalidOperationException("Role is already deactivated");

        // Check if there are active users with this role
        var activeUsersCount = await _context.Users
            .CountAsync(u => u.RoleId == roleId && u.IsActive);

        if (activeUsersCount > 0)
            throw new InvalidOperationException($"Cannot deactivate role. {activeUsersCount} active users are assigned to this role");

        role.Status = RbacRoleStatus.Inactive;
        role.DeactivatedAt = DateTime.UtcNow;
        role.DeactivatedBy = deactivatedBy;

        await _context.SaveChangesAsync();

        // Log the deactivation
        await _auditService.LogPermissionChangeAsync(new PermissionChangeLog
        {
            ActorUserId = deactivatedBy,
            RoleId = role.RoleId,
            ActionType = AuditActionTypes.RoleDeactivate,
            EntityType = AuditEntityTypes.Role,
            OldValue = RbacRoleStatus.Active,
            NewValue = RbacRoleStatus.Inactive,
            Reason = reason
        });

        _logger.LogInformation("Role '{RoleName}' deactivated by user {UserId}. Reason: {Reason}", 
            role.RoleName, deactivatedBy, reason);
    }

    public async Task ReactivateRoleAsync(int roleId, int reactivatedBy, string reason)
    {
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
            throw new ArgumentException($"Role with ID {roleId} not found");

        if (role.Status == RbacRoleStatus.Active)
            throw new InvalidOperationException("Role is already active");

        role.Status = RbacRoleStatus.Active;
        role.DeactivatedAt = null;
        role.DeactivatedBy = null;

        await _context.SaveChangesAsync();

        // Log the reactivation
        await _auditService.LogPermissionChangeAsync(new PermissionChangeLog
        {
            ActorUserId = reactivatedBy,
            RoleId = role.RoleId,
            ActionType = AuditActionTypes.RoleUpdate,
            EntityType = AuditEntityTypes.Role,
            OldValue = RbacRoleStatus.Inactive,
            NewValue = RbacRoleStatus.Active,
            Reason = reason
        });

        _logger.LogInformation("Role '{RoleName}' reactivated by user {UserId}. Reason: {Reason}", 
            role.RoleName, reactivatedBy, reason);
    }

    public async Task<List<RbacRolePermission>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.Set<RbacRolePermission>()
            .Include(rp => rp.Permission)
            .Include(rp => rp.GrantedByUser)
            .Include(rp => rp.RevokedByUser)
            .Where(rp => rp.RoleId == roleId)
            .OrderBy(rp => rp.Permission.Module)
            .ThenBy(rp => rp.Permission.PermissionCode)
            .ToListAsync();
    }

    public async Task UpdateRolePermissionsAsync(int roleId, List<RolePermissionUpdate> updates, int updatedBy)
    {
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
            throw new ArgumentException($"Role with ID {roleId} not found");

        if (role.IsSystemRole)
            throw new InvalidOperationException("System role permissions cannot be modified");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var update in updates)
            {
                await UpdateSingleRolePermissionAsync(roleId, update, updatedBy);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Updated {Count} permissions for role '{RoleName}' by user {UserId}", 
                updates.Count, role.RoleName, updatedBy);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<RolePermissionMatrix> GetRolePermissionMatrixAsync(int roleId)
    {
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
            throw new ArgumentException($"Role with ID {roleId} not found");

        var allPermissions = await _context.Set<RbacPermission>()
            .Where(p => p.Status == PermissionStatus.Active)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.PermissionCode)
            .ToListAsync();

        var rolePermissions = await GetRolePermissionsAsync(roleId);
        var rolePermissionDict = rolePermissions
            .Where(rp => rp.Status == RolePermissionStatus.Active)
            .ToDictionary(rp => rp.PermissionId, rp => rp.Allowed);

        var matrix = new RolePermissionMatrix
        {
            RoleId = roleId,
            RoleName = role.RoleName,
            Permissions = allPermissions.Select(p => new PermissionMatrixItem
            {
                PermissionId = p.PermissionId,
                PermissionCode = p.PermissionCode,
                Module = p.Module,
                Description = p.Description,
                Allowed = rolePermissionDict.GetValueOrDefault(p.PermissionId, false),
                IsExplicitlySet = rolePermissionDict.ContainsKey(p.PermissionId)
            }).ToList()
        };

        return matrix;
    }

    private async Task UpdateSingleRolePermissionAsync(int roleId, RolePermissionUpdate update, int updatedBy)
    {
        var existingPermission = await _context.Set<RbacRolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == update.PermissionId);

        if (existingPermission == null)
        {
            // Create new role permission
            var newRolePermission = new RbacRolePermission
            {
                RoleId = roleId,
                PermissionId = update.PermissionId,
                Allowed = update.Allowed,
                GrantedBy = updatedBy,
                GrantedAt = DateTime.UtcNow,
                Status = RolePermissionStatus.Active
            };

            _context.Set<RbacRolePermission>().Add(newRolePermission);

            // Log the grant
            await _auditService.LogPermissionChangeAsync(new PermissionChangeLog
            {
                ActorUserId = updatedBy,
                RoleId = roleId,
                PermissionId = update.PermissionId,
                ActionType = update.Allowed ? AuditActionTypes.Grant : AuditActionTypes.Revoke,
                EntityType = AuditEntityTypes.RolePermission,
                NewValue = update.Allowed.ToString(),
                Reason = update.Reason
            });
        }
        else if (existingPermission.Allowed != update.Allowed)
        {
            // Update existing permission
            var oldValue = existingPermission.Allowed.ToString();
            
            existingPermission.Allowed = update.Allowed;
            existingPermission.GrantedBy = updatedBy;
            existingPermission.GrantedAt = DateTime.UtcNow;
            existingPermission.Status = RolePermissionStatus.Active;

            // Log the change
            await _auditService.LogPermissionChangeAsync(new PermissionChangeLog
            {
                ActorUserId = updatedBy,
                RoleId = roleId,
                PermissionId = update.PermissionId,
                ActionType = update.Allowed ? AuditActionTypes.Grant : AuditActionTypes.Revoke,
                EntityType = AuditEntityTypes.RolePermission,
                OldValue = oldValue,
                NewValue = update.Allowed.ToString(),
                Reason = update.Reason
            });
        }
    }
}

// Supporting classes
public class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateRoleRequest
{
    public string? RoleName { get; set; }
    public string? Description { get; set; }
}

public class RolePermissionUpdate
{
    public int PermissionId { get; set; }
    public bool Allowed { get; set; }
    public string? Reason { get; set; }
}

public class RolePermissionMatrix
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionMatrixItem> Permissions { get; set; } = new();
}

public class PermissionMatrixItem
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Allowed { get; set; }
    public bool IsExplicitlySet { get; set; }
}