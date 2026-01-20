namespace ITAMS.Domain.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, int userId, string userName, string? oldValues = null, string? newValues = null);
    Task<IEnumerable<Domain.Entities.AuditEntry>> GetAuditTrailAsync(string? entityType = null, string? entityId = null, int? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
}