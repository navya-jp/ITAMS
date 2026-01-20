using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ITAMS.Data;

namespace ITAMS.Services;

public class PermissionService : IPermissionService
{
    private readonly ITAMSDbContext _context;
    private readonly IAuditService _auditService;

    public PermissionService(ITAMSDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByModuleAsync(string module)
    {
        return await _context.Permissions
            .Where(p => p.Module == module)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId)
    {
        // Get permissions from user's role
        var rolePermissions = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Role.RolePermissions)
            .Where(rp => rp.IsGranted)
            .Select(rp => rp.Permission)
            .ToListAsync();

        return rolePermissions.Distinct();
    }

    public async Task<IEnumerable<Permission>> GetUserProjectPermissionsAsync(int userId, int projectId)
    {
        // Get project-specific permissions for the user
        var projectPermissions = await _context.UserProjects
            .Where(up => up.UserId == userId && up.ProjectId == projectId && up.IsActive)
            .SelectMany(up => up.UserProjectPermissions)
            .Where(upp => upp.IsGranted)
            .Select(upp => upp.Permission)
            .ToListAsync();

        return projectPermissions.Distinct();
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.IsGranted)
            .Select(rp => rp.Permission)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
    {
        // Check if user has permission through their role
        var hasRolePermission = await _context.Users
            .Where(u => u.Id == userId && u.IsActive)
            .SelectMany(u => u.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Code == permissionCode && rp.IsGranted);

        return hasRolePermission;
    }

    public async Task<bool> HasProjectPermissionAsync(int userId, int projectId, string permissionCode)
    {
        // Check if user has project-specific permission
        var hasProjectPermission = await _context.UserProjects
            .Where(up => up.UserId == userId && up.ProjectId == projectId && up.IsActive)
            .SelectMany(up => up.UserProjectPermissions)
            .AnyAsync(upp => upp.Permission.Code == permissionCode && upp.IsGranted);

        // Also check role-based permissions as fallback
        if (!hasProjectPermission)
        {
            hasProjectPermission = await HasPermissionAsync(userId, permissionCode);
        }

        return hasProjectPermission;
    }

    public async Task UpdateRolePermissionsAsync(int roleId, int[] permissionIds)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        // Remove existing permissions
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingPermissions);

        // Add new permissions
        var newPermissions = permissionIds.Select(permissionId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            IsGranted = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.RolePermissions.AddRange(newPermissions);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("ROLE_PERMISSIONS_UPDATED", "RolePermission", 
            roleId.ToString(), 1, "superadmin");
    }
}