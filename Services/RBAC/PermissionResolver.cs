using ITAMS.Domain.Entities;
using ITAMS.Domain.Entities.RBAC;
using ITAMS.Domain.Interfaces;
using ITAMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ITAMS.Services.RBAC;

public interface IPermissionResolver
{
    Task<PermissionResult> HasPermissionAsync(int userId, string permissionCode, string? resourceId = null, int? projectId = null);
    Task<BulkPermissionResult> CheckMultiplePermissionsAsync(BulkPermissionRequest request);
    Task<UserPermissionSummary> GetUserPermissionSummaryAsync(int userId);
    Task InvalidateUserCacheAsync(int userId);
    Task InvalidatePermissionCacheAsync(string permissionCode);
}

public class PermissionResolver : IPermissionResolver
{
    private readonly ITAMSDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IRbacAuditService _auditService;
    private readonly ILogger<PermissionResolver> _logger;
    
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "perm_";

    public PermissionResolver(
        ITAMSDbContext context,
        IMemoryCache cache,
        IRbacAuditService auditService,
        ILogger<PermissionResolver> logger)
    {
        _context = context;
        _cache = cache;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<PermissionResult> HasPermissionAsync(
        int userId, 
        string permissionCode, 
        string? resourceId = null, 
        int? projectId = null)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Step 1: Check cache first
            var cacheKey = GetCacheKey(userId, permissionCode, projectId);
            if (_cache.TryGetValue(cacheKey, out PermissionResult? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Permission check cache hit for user {UserId}, permission {Permission}", userId, permissionCode);
                return cachedResult;
            }

            // Step 2: Check if user is active
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || !user.IsActive)
            {
                var result = DenyAccess(ResolutionMethods.UserInactive, "User account is not active or does not exist");
                await LogAccessAttempt(userId, permissionCode, resourceId, projectId, result);
                return result;
            }

            // Step 3: Check user-specific permission overrides FIRST (Override → Role → Deny)
            var userOverride = await GetUserPermissionOverrideAsync(userId, permissionCode);
            if (userOverride != null)
            {
                if (userOverride.Allowed)
                {
                    // Still need to check scope even with override
                    var scopeCheck = await ValidateScopeAsync(userId, projectId);
                    if (!scopeCheck.Valid)
                    {
                        var scopeResult = DenyAccess(ResolutionMethods.ScopeViolation, scopeCheck.Reason);
                        await LogAccessAttempt(userId, permissionCode, resourceId, projectId, scopeResult);
                        return scopeResult;
                    }
                    
                    var overrideResult = GrantAccess(ResolutionMethods.UserOverride, $"Granted by user override: {userOverride.Reason}");
                    CacheResult(cacheKey, overrideResult);
                    await LogAccessAttempt(userId, permissionCode, resourceId, projectId, overrideResult);
                    return overrideResult;
                }
                else
                {
                    var denyResult = DenyAccess(ResolutionMethods.UserOverride, "Explicitly denied by user override");
                    await LogAccessAttempt(userId, permissionCode, resourceId, projectId, denyResult);
                    return denyResult;
                }
            }

            // Step 4: Check role-based permissions
            var rolePermission = await GetRolePermissionAsync(user.RoleId, permissionCode);
            if (rolePermission != null && rolePermission.Allowed)
            {
                // Validate scope for role-based permissions
                var scopeCheck = await ValidateScopeAsync(userId, projectId);
                if (!scopeCheck.Valid)
                {
                    var scopeResult = DenyAccess(ResolutionMethods.ScopeViolation, scopeCheck.Reason);
                    await LogAccessAttempt(userId, permissionCode, resourceId, projectId, scopeResult);
                    return scopeResult;
                }
                
                var roleResult = GrantAccess(ResolutionMethods.RolePermission, $"Granted by role: {user.Role?.Name}");
                CacheResult(cacheKey, roleResult);
                await LogAccessAttempt(userId, permissionCode, resourceId, projectId, roleResult);
                return roleResult;
            }

            // Step 5: Check if resource is decommissioned/inactive
            if (!string.IsNullOrEmpty(resourceId))
            {
                var resourceStatus = await GetResourceStatusAsync(resourceId);
                if (resourceStatus == "DECOMMISSIONED" || resourceStatus == "INACTIVE")
                {
                    var resourceResult = DenyAccess(ResolutionMethods.ResourceInactive, "Cannot access inactive/decommissioned resource");
                    await LogAccessAttempt(userId, permissionCode, resourceId, projectId, resourceResult);
                    return resourceResult;
                }
            }

            // Step 6: Default deny
            var defaultResult = DenyAccess(ResolutionMethods.DefaultDeny, "No explicit permission granted");
            await LogAccessAttempt(userId, permissionCode, resourceId, projectId, defaultResult);
            return defaultResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during permission resolution for user {UserId}, permission {Permission}", userId, permissionCode);
            var errorResult = DenyAccess(ResolutionMethods.DefaultDeny, "Permission resolution error");
            await LogAccessAttempt(userId, permissionCode, resourceId, projectId, errorResult);
            return errorResult;
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogDebug("Permission resolution completed in {Duration}ms for user {UserId}, permission {Permission}", 
                duration.TotalMilliseconds, userId, permissionCode);
        }
    }

    public async Task<BulkPermissionResult> CheckMultiplePermissionsAsync(BulkPermissionRequest request)
    {
        var results = new Dictionary<string, PermissionResult>();
        
        foreach (var permissionCode in request.PermissionCodes)
        {
            var result = await HasPermissionAsync(request.UserId, permissionCode, request.ResourceId, request.ProjectId);
            results[permissionCode] = result;
        }

        return new BulkPermissionResult
        {
            UserId = request.UserId,
            Results = results,
            AllGranted = results.Values.All(r => r.Granted),
            AnyGranted = results.Values.Any(r => r.Granted)
        };
    }

    public async Task<UserPermissionSummary> GetUserPermissionSummaryAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        // Map from Roles table to RbacRoles table using the role NAME (not alternate key)
        var rbacRole = await _context.Set<RbacRole>()
            .FirstOrDefaultAsync(r => r.RoleName == user.Role.Name);

        if (rbacRole == null)
        {
            throw new ArgumentException($"RBAC role not found for user's role {user.Role?.Name}");
        }

        // Get role permissions using the RbacRoles.Id
        var rolePermissions = await _context.Set<RbacRolePermission>()
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == rbacRole.Id && rp.Status == RolePermissionStatus.Active && rp.Allowed)
            .Select(rp => rp.Permission.PermissionCode)
            .ToListAsync();

        // Get user permission overrides
        var userOverrides = await _context.Set<RbacUserPermission>()
            .Include(up => up.Permission)
            .Where(up => up.UserId == userId && up.Status == UserPermissionStatus.Active)
            .ToListAsync();

        // Get user scopes
        var userScopes = await _context.Set<RbacUserScope>()
            .Include(us => us.Project)
            .Where(us => us.UserId == userId && us.Status == UserScopeStatus.Active)
            .ToListAsync();

        return new UserPermissionSummary
        {
            UserId = userId,
            Username = user.Username,
            RoleName = user.Role?.Name ?? "No Role",
            RolePermissions = rolePermissions,
            UserOverrides = userOverrides.Select(uo => new UserPermissionOverrideInfo
            {
                PermissionCode = uo.Permission.PermissionCode,
                Allowed = uo.Allowed,
                Reason = uo.Reason,
                ExpiresAt = uo.ExpiresAt,
                GrantedBy = uo.GrantedByUser?.Username ?? "System",
                GrantedAt = uo.GrantedAt
            }).ToList(),
            Scopes = userScopes.Select(us => new UserScopeInfo
            {
                ScopeType = us.ScopeType,
                ProjectId = us.ProjectId,
                ProjectName = us.Project?.Name,
                AssignedAt = us.AssignedAt
            }).ToList()
        };
    }

    private async Task<RbacUserPermission?> GetUserPermissionOverrideAsync(int userId, string permissionCode)
    {
        return await _context.Set<RbacUserPermission>()
            .Include(up => up.Permission)
            .Include(up => up.GrantedByUser)
            .FirstOrDefaultAsync(up => 
                up.UserId == userId && 
                up.Permission.PermissionCode == permissionCode && 
                up.Status == UserPermissionStatus.Active &&
                (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow));
    }

    private async Task<RbacRolePermission?> GetRolePermissionAsync(int roleId, string permissionCode)
    {
        // roleId here is from Users.RoleId which points to Roles table
        // We need to map it to RbacRoles.Id using role NAME
        var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null) return null;
        
        var rbacRole = await _context.Set<RbacRole>().FirstOrDefaultAsync(r => r.RoleName == role.Name);
        if (rbacRole == null) return null;

        return await _context.Set<RbacRolePermission>()
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => 
                rp.RoleId == rbacRole.Id && 
                rp.Permission.PermissionCode == permissionCode && 
                rp.Status == RolePermissionStatus.Active);
    }

    private async Task<ScopeValidation> ValidateScopeAsync(int userId, int? projectId)
    {
        var userScopes = await _context.Set<RbacUserScope>()
            .Where(us => us.UserId == userId && us.Status == UserScopeStatus.Active)
            .ToListAsync();

        // If user has global scope, allow access to any project
        if (userScopes.Any(scope => scope.ScopeType == ScopeTypes.Global))
        {
            return new ScopeValidation { Valid = true, Reason = "Global scope granted" };
        }

        // If no project specified and user has project scope, deny
        if (!projectId.HasValue && userScopes.All(scope => scope.ScopeType == ScopeTypes.Project))
        {
            return new ScopeValidation { Valid = false, Reason = "Project-specific user accessing global resource" };
        }

        // If project specified, check if user has access to that project
        if (projectId.HasValue)
        {
            var hasProjectAccess = userScopes.Any(scope => 
                scope.ScopeType == ScopeTypes.Project && 
                scope.ProjectId == projectId.Value);
            
            if (!hasProjectAccess)
            {
                return new ScopeValidation { Valid = false, Reason = $"No access to project {projectId}" };
            }
        }

        return new ScopeValidation { Valid = true, Reason = "Scope validation passed" };
    }

    private async Task<string?> GetResourceStatusAsync(string resourceId)
    {
        // This is a simplified implementation - in reality, you'd check the appropriate table based on resource type
        // For now, we'll assume all resources are active
        return "ACTIVE";
    }

    private async Task LogAccessAttempt(int userId, string permissionCode, string? resourceId, int? projectId, PermissionResult result)
    {
        try
        {
            await _auditService.LogAccessAttemptAsync(new AccessAttemptLog
            {
                UserId = userId,
                PermissionCode = permissionCode,
                ResourceId = resourceId,
                ActionAttempted = "ACCESS",
                AccessGranted = result.Granted,
                DenialReason = result.Reason,
                ResolutionMethod = result.Method,
                ScopeValidated = projectId.HasValue,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log access attempt for user {UserId}, permission {Permission}", userId, permissionCode);
        }
    }

    private PermissionResult GrantAccess(string method, string reason)
    {
        return new PermissionResult
        {
            Granted = true,
            Method = method,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };
    }

    private PermissionResult DenyAccess(string method, string reason)
    {
        return new PermissionResult
        {
            Granted = false,
            Method = method,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };
    }

    private string GetCacheKey(int userId, string permissionCode, int? projectId)
    {
        return $"{CacheKeyPrefix}{userId}:{permissionCode}:{projectId ?? 0}";
    }

    private void CacheResult(string cacheKey, PermissionResult result)
    {
        _cache.Set(cacheKey, result, _cacheExpiry);
    }

    public async Task InvalidateUserCacheAsync(int userId)
    {
        // Remove all cached permissions for this user
        // Note: This is a simplified implementation. In production, you might want to use a more sophisticated cache invalidation strategy
        _logger.LogDebug("Invalidating permission cache for user {UserId}", userId);
        
        // Since MemoryCache doesn't have a way to remove by pattern, we'll need to track keys or use a different caching strategy
        // For now, we'll just log the invalidation
    }

    public async Task InvalidatePermissionCacheAsync(string permissionCode)
    {
        _logger.LogDebug("Invalidating permission cache for permission {Permission}", permissionCode);
        
        // Similar to user cache invalidation, this would need a more sophisticated implementation in production
    }
}

// Supporting classes and interfaces
public class PermissionResult
{
    public bool Granted { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class BulkPermissionRequest
{
    public int UserId { get; set; }
    public List<string> PermissionCodes { get; set; } = new();
    public string? ResourceId { get; set; }
    public int? ProjectId { get; set; }
}

public class BulkPermissionResult
{
    public int UserId { get; set; }
    public Dictionary<string, PermissionResult> Results { get; set; } = new();
    public bool AllGranted { get; set; }
    public bool AnyGranted { get; set; }
}

public class ScopeValidation
{
    public bool Valid { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class UserPermissionSummary
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<string> RolePermissions { get; set; } = new();
    public List<UserPermissionOverrideInfo> UserOverrides { get; set; } = new();
    public List<UserScopeInfo> Scopes { get; set; } = new();
}

public class UserPermissionOverrideInfo
{
    public string PermissionCode { get; set; } = string.Empty;
    public bool Allowed { get; set; }
    public string? Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; }
}

public class UserScopeInfo
{
    public string ScopeType { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class AccessAttemptLog
{
    public int UserId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string ActionAttempted { get; set; } = string.Empty;
    public bool AccessGranted { get; set; }
    public string? DenialReason { get; set; }
    public string? ResolutionMethod { get; set; }
    public bool ScopeValidated { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}