using ITAMS.Domain.Entities.RBAC;
using ITAMS.Domain.Interfaces;
using ITAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services.RBAC;

public interface IRbacAuditService
{
    Task LogPermissionChangeAsync(PermissionChangeLog log);
    Task LogAccessAttemptAsync(AccessAttemptLog log);
    Task<List<RbacPermissionAuditLog>> GetPermissionAuditLogAsync(PermissionAuditFilter filter);
    Task<List<RbacAccessAuditLog>> GetAccessAuditLogAsync(AccessAuditFilter filter);
    Task<ComplianceReport> GenerateComplianceReportAsync(ComplianceReportRequest request);
}

public class RbacAuditService : IRbacAuditService
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<RbacAuditService> _logger;

    public RbacAuditService(ITAMSDbContext context, ILogger<RbacAuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogPermissionChangeAsync(PermissionChangeLog log)
    {
        try
        {
            var auditEntry = new RbacPermissionAuditLog
            {
                ActorUserId = log.ActorUserId,
                TargetUserId = log.TargetUserId,
                RoleId = log.RoleId,
                PermissionId = log.PermissionId,
                ActionType = log.ActionType,
                EntityType = log.EntityType,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                Reason = log.Reason,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.Set<RbacPermissionAuditLog>().Add(auditEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission change logged: {ActionType} by user {ActorUserId} on {EntityType}", 
                log.ActionType, log.ActorUserId, log.EntityType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log permission change: {ActionType} by user {ActorUserId}", 
                log.ActionType, log.ActorUserId);
            throw;
        }
    }

    public async Task LogAccessAttemptAsync(AccessAttemptLog log)
    {
        try
        {
            var auditEntry = new RbacAccessAuditLog
            {
                UserId = log.UserId,
                PermissionCode = log.PermissionCode,
                ResourceId = log.ResourceId,
                ResourceType = GetResourceTypeFromId(log.ResourceId),
                ActionAttempted = log.ActionAttempted,
                AccessGranted = log.AccessGranted,
                DenialReason = log.DenialReason,
                ResolutionMethod = log.ResolutionMethod,
                ScopeValidated = log.ScopeValidated,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Timestamp = log.Timestamp
            };

            _context.Set<RbacAccessAuditLog>().Add(auditEntry);
            await _context.SaveChangesAsync();

            if (!log.AccessGranted)
            {
                _logger.LogWarning("Access denied for user {UserId} to permission {Permission}: {Reason}", 
                    log.UserId, log.PermissionCode, log.DenialReason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log access attempt for user {UserId}, permission {Permission}", 
                log.UserId, log.PermissionCode);
            // Don't throw here as this shouldn't break the main flow
        }
    }

    public async Task<List<RbacPermissionAuditLog>> GetPermissionAuditLogAsync(PermissionAuditFilter filter)
    {
        var query = _context.Set<RbacPermissionAuditLog>()
            .Include(log => log.ActorUser)
            .Include(log => log.TargetUser)
            .Include(log => log.Role)
            .Include(log => log.Permission)
            .AsQueryable();

        // Apply filters
        if (filter.ActorUserId.HasValue)
            query = query.Where(log => log.ActorUserId == filter.ActorUserId.Value);

        if (filter.TargetUserId.HasValue)
            query = query.Where(log => log.TargetUserId == filter.TargetUserId.Value);

        if (!string.IsNullOrEmpty(filter.ActionType))
            query = query.Where(log => log.ActionType == filter.ActionType);

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(log => log.EntityType == filter.EntityType);

        if (filter.StartDate.HasValue)
            query = query.Where(log => log.Timestamp >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(log => log.Timestamp <= filter.EndDate.Value);

        // Order by timestamp descending (most recent first)
        query = query.OrderByDescending(log => log.Timestamp);

        // Apply pagination
        if (filter.Skip.HasValue)
            query = query.Skip(filter.Skip.Value);

        if (filter.Take.HasValue)
            query = query.Take(filter.Take.Value);

        return await query.ToListAsync();
    }

    public async Task<List<RbacAccessAuditLog>> GetAccessAuditLogAsync(AccessAuditFilter filter)
    {
        var query = _context.Set<RbacAccessAuditLog>()
            .Include(log => log.User)
            .AsQueryable();

        // Apply filters
        if (filter.UserId.HasValue)
            query = query.Where(log => log.UserId == filter.UserId.Value);

        if (!string.IsNullOrEmpty(filter.PermissionCode))
            query = query.Where(log => log.PermissionCode == filter.PermissionCode);

        if (filter.AccessGranted.HasValue)
            query = query.Where(log => log.AccessGranted == filter.AccessGranted.Value);

        if (!string.IsNullOrEmpty(filter.ResourceType))
            query = query.Where(log => log.ResourceType == filter.ResourceType);

        if (filter.StartDate.HasValue)
            query = query.Where(log => log.Timestamp >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(log => log.Timestamp <= filter.EndDate.Value);

        // Order by timestamp descending (most recent first)
        query = query.OrderByDescending(log => log.Timestamp);

        // Apply pagination
        if (filter.Skip.HasValue)
            query = query.Skip(filter.Skip.Value);

        if (filter.Take.HasValue)
            query = query.Take(filter.Take.Value);

        return await query.ToListAsync();
    }

    public async Task<ComplianceReport> GenerateComplianceReportAsync(ComplianceReportRequest request)
    {
        var report = new ComplianceReport
        {
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = request.RequestedBy,
            Period = new ReportPeriod
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate
            },
            Sections = new List<ComplianceReportSection>()
        };

        // Permission Changes Section
        var permissionChanges = await GetPermissionChangesForReport(request.StartDate, request.EndDate);
        report.Sections.Add(new ComplianceReportSection
        {
            Title = "Permission Changes",
            Data = permissionChanges,
            Summary = new Dictionary<string, object>
            {
                ["TotalChanges"] = permissionChanges.Count,
                ["GrantsCount"] = permissionChanges.Count(c => c.ActionType == AuditActionTypes.Grant),
                ["RevocationsCount"] = permissionChanges.Count(c => c.ActionType == AuditActionTypes.Revoke),
                ["RoleChanges"] = permissionChanges.Count(c => c.ActionType == AuditActionTypes.RoleAssign),
                ["UniqueUsers"] = permissionChanges.Select(c => c.TargetUserId).Distinct().Count()
            }
        });

        // Access Violations Section
        var violations = await GetAccessViolationsForReport(request.StartDate, request.EndDate);
        report.Sections.Add(new ComplianceReportSection
        {
            Title = "Access Violations",
            Data = violations,
            Summary = new Dictionary<string, object>
            {
                ["TotalViolations"] = violations.Count,
                ["UniqueUsers"] = violations.Select(v => v.UserId).Distinct().Count(),
                ["MostCommonReason"] = violations.GroupBy(v => v.DenialReason)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            }
        });

        // Super Admin Actions Section
        var superAdminActions = await GetSuperAdminActionsForReport(request.StartDate, request.EndDate);
        report.Sections.Add(new ComplianceReportSection
        {
            Title = "Super Admin Actions",
            Data = superAdminActions,
            Summary = new Dictionary<string, object>
            {
                ["TotalActions"] = superAdminActions.Count,
                ["UniqueAdmins"] = superAdminActions.Select(a => a.ActorUserId).Distinct().Count(),
                ["PermissionOverrides"] = superAdminActions.Count(a => a.ActionType == AuditActionTypes.Grant && a.EntityType == AuditEntityTypes.UserPermission)
            }
        });

        // Data Integrity Section
        var integrityIssues = await ValidateDataIntegrityAsync();
        report.Sections.Add(new ComplianceReportSection
        {
            Title = "Data Integrity",
            Data = integrityIssues,
            Summary = new Dictionary<string, object>
            {
                ["IssuesFound"] = integrityIssues.Count,
                ["HighSeverityIssues"] = integrityIssues.Count(i => i.Severity == "HIGH"),
                ["MediumSeverityIssues"] = integrityIssues.Count(i => i.Severity == "MEDIUM")
            }
        });

        return report;
    }

    private async Task<List<RbacPermissionAuditLog>> GetPermissionChangesForReport(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<RbacPermissionAuditLog>()
            .Include(log => log.ActorUser)
            .Include(log => log.TargetUser)
            .Include(log => log.Role)
            .Include(log => log.Permission)
            .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    private async Task<List<RbacAccessAuditLog>> GetAccessViolationsForReport(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<RbacAccessAuditLog>()
            .Include(log => log.User)
            .Where(log => !log.AccessGranted && log.Timestamp >= startDate && log.Timestamp <= endDate)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    private async Task<List<RbacPermissionAuditLog>> GetSuperAdminActionsForReport(DateTime startDate, DateTime endDate)
    {
        // Get Super Admin role ID
        var superAdminRole = await _context.Set<RbacRole>()
            .FirstOrDefaultAsync(r => r.RoleName == "Super Admin" && r.Status == RbacRoleStatus.Active);

        if (superAdminRole == null)
            return new List<RbacPermissionAuditLog>();

        // Get users with Super Admin role
        var superAdminUserIds = await _context.Users
            .Where(u => u.RoleId == superAdminRole.RoleId && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        return await _context.Set<RbacPermissionAuditLog>()
            .Include(log => log.ActorUser)
            .Include(log => log.TargetUser)
            .Include(log => log.Role)
            .Include(log => log.Permission)
            .Where(log => superAdminUserIds.Contains(log.ActorUserId) && 
                         log.Timestamp >= startDate && log.Timestamp <= endDate)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    private async Task<List<IntegrityIssue>> ValidateDataIntegrityAsync()
    {
        var issues = new List<IntegrityIssue>();

        // Check for orphaned user permissions
        var orphanedUserPermissions = await _context.Set<RbacUserPermission>()
            .Where(up => !_context.Users.Any(u => u.Id == up.UserId) ||
                        !_context.Set<RbacPermission>().Any(p => p.PermissionId == up.PermissionId))
            .CountAsync();

        if (orphanedUserPermissions > 0)
        {
            issues.Add(new IntegrityIssue
            {
                Type = "ORPHANED_USER_PERMISSIONS",
                Severity = "HIGH",
                Count = orphanedUserPermissions,
                Description = "User permissions referencing non-existent users or permissions"
            });
        }

        // Check for users without roles
        var usersWithoutRoles = await _context.Users
            .Where(u => !_context.Set<RbacRole>().Any(r => r.RoleId == u.RoleId) && u.IsActive)
            .CountAsync();

        if (usersWithoutRoles > 0)
        {
            issues.Add(new IntegrityIssue
            {
                Type = "USERS_WITHOUT_ROLES",
                Severity = "MEDIUM",
                Count = usersWithoutRoles,
                Description = "Active users without valid role assignments"
            });
        }

        // Check for expired user permissions that haven't been marked as expired
        var expiredPermissions = await _context.Set<RbacUserPermission>()
            .Where(up => up.ExpiresAt.HasValue && up.ExpiresAt.Value <= DateTime.UtcNow && up.Status == UserPermissionStatus.Active)
            .CountAsync();

        if (expiredPermissions > 0)
        {
            issues.Add(new IntegrityIssue
            {
                Type = "EXPIRED_PERMISSIONS_NOT_MARKED",
                Severity = "MEDIUM",
                Count = expiredPermissions,
                Description = "Expired user permissions that haven't been marked as expired"
            });
        }

        return issues;
    }

    private string? GetResourceTypeFromId(string? resourceId)
    {
        if (string.IsNullOrEmpty(resourceId))
            return null;

        // Simple heuristic to determine resource type from ID
        // In a real implementation, you might have a more sophisticated way to determine this
        if (resourceId.StartsWith("user_"))
            return "USER";
        if (resourceId.StartsWith("asset_"))
            return "ASSET";
        if (resourceId.StartsWith("project_"))
            return "PROJECT";

        return "UNKNOWN";
    }
}

// Supporting classes
public class PermissionChangeLog
{
    public int ActorUserId { get; set; }
    public int? TargetUserId { get; set; }
    public int? RoleId { get; set; }
    public int? PermissionId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class PermissionAuditFilter
{
    public int? ActorUserId { get; set; }
    public int? TargetUserId { get; set; }
    public string? ActionType { get; set; }
    public string? EntityType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}

public class AccessAuditFilter
{
    public int? UserId { get; set; }
    public string? PermissionCode { get; set; }
    public bool? AccessGranted { get; set; }
    public string? ResourceType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}

public class ComplianceReportRequest
{
    public int RequestedBy { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> Sections { get; set; } = new();
}

public class ComplianceReport
{
    public DateTime GeneratedAt { get; set; }
    public int GeneratedBy { get; set; }
    public ReportPeriod Period { get; set; } = new();
    public List<ComplianceReportSection> Sections { get; set; } = new();
}

public class ReportPeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ComplianceReportSection
{
    public string Title { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public Dictionary<string, object> Summary { get; set; } = new();
}

public class IntegrityIssue
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Description { get; set; } = string.Empty;
}