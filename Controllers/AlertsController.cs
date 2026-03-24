using Microsoft.AspNetCore.Mvc;
using ITAMS.Data;
using ITAMS.Models;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : BaseController
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(ITAMSDbContext context, ILogger<AlertsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET /api/alerts — all unresolved alerts (paginated)
    [HttpGet]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] bool includeResolved = false,
        [FromQuery] string? severity = null,
        [FromQuery] string? alertType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.SystemAlerts.AsQueryable();

        if (!includeResolved)
            query = query.Where(a => !a.IsResolved);

        if (!string.IsNullOrEmpty(severity))
            query = query.Where(a => a.Severity == severity);

        if (!string.IsNullOrEmpty(alertType))
            query = query.Where(a => a.AlertType == alertType);

        var total = await query.CountAsync();
        var alerts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AlertDto
            {
                Id = a.Id,
                AlertType = a.AlertType,
                Severity = a.Severity,
                Title = a.Title,
                Message = a.Message,
                AssetId = a.AssetId,
                EntityType = a.EntityType,
                EntityIdentifier = a.EntityIdentifier,
                IsAcknowledged = a.IsAcknowledged,
                IsResolved = a.IsResolved,
                EscalationLevel = a.EscalationLevel,
                CreatedAt = a.CreatedAt,
                AcknowledgedAt = a.AcknowledgedAt
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, alerts });
    }

    // GET /api/alerts/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _context.SystemAlerts
            .CountAsync(a => !a.IsAcknowledged && !a.IsResolved);

        var summary = new AlertSummaryDto
        {
            TotalUnread = count,
            Critical = await _context.SystemAlerts.CountAsync(a => !a.IsAcknowledged && !a.IsResolved && a.Severity == "Critical"),
            High = await _context.SystemAlerts.CountAsync(a => !a.IsAcknowledged && !a.IsResolved && a.Severity == "High"),
            Medium = await _context.SystemAlerts.CountAsync(a => !a.IsAcknowledged && !a.IsResolved && a.Severity == "Medium"),
            Low = await _context.SystemAlerts.CountAsync(a => !a.IsAcknowledged && !a.IsResolved && a.Severity == "Low")
        };

        return Ok(summary);
    }

    // POST /api/alerts/{id}/acknowledge
    [HttpPost("{id}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var alert = await _context.SystemAlerts.FindAsync(id);
        if (alert == null) return NotFound();

        alert.IsAcknowledged = true;
        alert.AcknowledgedBy = GetCurrentUserId();
        alert.AcknowledgedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Alert acknowledged" });
    }

    // POST /api/alerts/{id}/resolve
    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        var alert = await _context.SystemAlerts.FindAsync(id);
        if (alert == null) return NotFound();

        alert.IsResolved = true;
        alert.IsAcknowledged = true;
        alert.ResolvedAt = DateTime.UtcNow;
        if (alert.AcknowledgedBy == null)
        {
            alert.AcknowledgedBy = GetCurrentUserId();
            alert.AcknowledgedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();

        return Ok(new { message = "Alert resolved" });
    }

    // GET /api/alerts/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var today = DateTime.Today;
        var in30Days = today.AddDays(30);

        var warrantyExpiring = await _context.Assets
            .Include(a => a.Project)
            .Include(a => a.Location)
            .Where(a => a.WarrantyEndDate != null && a.WarrantyEndDate >= today && a.WarrantyEndDate <= in30Days)
            .OrderBy(a => a.WarrantyEndDate)
            .Select(a => new ExpiringItemDto
            {
                Id = a.Id,
                Identifier = a.AssetTag,
                Name = a.Make + " " + a.Model,
                Project = a.Project != null ? a.Project.Name : null,
                Location = a.Location != null ? a.Location.Name : null,
                ExpiryDate = a.WarrantyEndDate,
                DaysRemaining = (int)(a.WarrantyEndDate!.Value - today).TotalDays,
                Severity = (int)(a.WarrantyEndDate.Value - today).TotalDays <= 7 ? "Critical" : "High"
            })
            .ToListAsync();

        var licenseExpiring = await _context.LicensingAssets
            .Where(l => l.ValidityEndDate >= today && l.ValidityEndDate <= in30Days)
            .OrderBy(l => l.ValidityEndDate)
            .Select(l => new ExpiringItemDto
            {
                Id = l.Id,
                Identifier = l.LicenseKey ?? "N/A",
                Name = l.LicenseName,
                ExpiryDate = l.ValidityEndDate,
                DaysRemaining = (int)(l.ValidityEndDate - today).TotalDays,
                Severity = (int)(l.ValidityEndDate - today).TotalDays <= 7 ? "Critical" : "High"
            })
            .ToListAsync();

        var contractExpiring = await _context.ServiceAssets
            .Where(s => s.ContractEndDate >= today && s.ContractEndDate <= in30Days)
            .OrderBy(s => s.ContractEndDate)
            .Select(s => new ExpiringItemDto
            {
                Id = s.Id,
                Identifier = s.ServiceName,
                Name = s.ServiceName,
                ExpiryDate = s.ContractEndDate,
                DaysRemaining = (int)(s.ContractEndDate - today).TotalDays,
                Severity = (int)(s.ContractEndDate - today).TotalDays <= 7 ? "Critical" : "High"
            })
            .ToListAsync();

        var repairStatusId = await _context.AssetStatuses
            .Where(s => s.StatusName.ToLower().Contains("repair"))
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        var assetsInRepair = repairStatusId > 0
            ? await _context.Assets
                .Include(a => a.Project)
                .Include(a => a.Location)
                .Where(a => a.AssetStatusId == repairStatusId)
                .Select(a => new AssetInRepairDto
                {
                    Id = a.Id,
                    AssetTag = a.AssetTag,
                    Make = a.Make,
                    Model = a.Model,
                    Project = a.Project != null ? a.Project.Name : null,
                    Location = a.Location != null ? a.Location.Name : null,
                    DaysInRepair = 0
                })
                .ToListAsync()
            : new List<AssetInRepairDto>();

        var recentAlerts = await _context.SystemAlerts
            .Where(a => !a.IsResolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new AlertDto
            {
                Id = a.Id,
                AlertType = a.AlertType,
                Severity = a.Severity,
                Title = a.Title,
                EntityIdentifier = a.EntityIdentifier,
                IsAcknowledged = a.IsAcknowledged,
                IsResolved = a.IsResolved,
                EscalationLevel = a.EscalationLevel,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(new DashboardReportDto
        {
            WarrantyExpiring = warrantyExpiring,
            LicenseExpiring = licenseExpiring,
            ContractExpiring = contractExpiring,
            AssetsInRepair = assetsInRepair,
            RecentAlerts = recentAlerts
        });
    }
}
