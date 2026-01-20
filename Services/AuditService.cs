using Microsoft.EntityFrameworkCore;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using ITAMS.Domain.Interfaces;

namespace ITAMS.Services;

public class AuditService : IAuditService
{
    private readonly ITAMSDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ITAMSDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityType, string entityId, int userId, string userName, string? oldValues = null, string? newValues = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        var auditEntry = new AuditEntry
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString()
        };

        _context.AuditEntries.Add(auditEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditEntry>> GetAuditTrailAsync(string? entityType = null, string? entityId = null, int? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.AuditEntries.AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            query = query.Where(a => a.EntityId == entityId);
        }

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Take(1000) // Limit to prevent performance issues
            .ToListAsync();
    }
}