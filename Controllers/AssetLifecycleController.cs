using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using ITAMS.Models;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/assets/{assetId:int}/lifecycle")]
public class AssetLifecycleController : BaseController
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<AssetLifecycleController> _logger;

    public AssetLifecycleController(ITAMSDbContext context, ILogger<AssetLifecycleController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET /api/assets/{assetId}/lifecycle
    [HttpGet]
    public async Task<ActionResult<AssetLifecycleSummaryDto>> GetLifecycleSummary(int assetId)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null) return NotFound(new { message = "Asset not found" });

            var assignments = await _context.AssetAssignmentHistories
                .Where(h => h.AssetId == assetId)
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new AssignmentHistoryDto
                {
                    Id = h.Id, AssetId = h.AssetId,
                    PreviousUserId = h.PreviousUserId, PreviousUserName = h.PreviousUserName,
                    NewUserId = h.NewUserId, NewUserName = h.NewUserName,
                    PreviousLocationId = h.PreviousLocationId, PreviousLocationName = h.PreviousLocationName,
                    NewLocationId = h.NewLocationId, NewLocationName = h.NewLocationName,
                    Reason = h.Reason, ChangedAt = h.ChangedAt, ChangedByName = h.ChangedByName
                }).ToListAsync();

            var transfers = await _context.AssetTransferRequests
                .Include(t => t.FromLocation).Include(t => t.ToLocation)
                .Include(t => t.FromUser).Include(t => t.ToUser)
                .Where(t => t.AssetId == assetId)
                .OrderByDescending(t => t.TransferDate)
                .Select(t => new TransferRequestDto
                {
                    Id = t.Id, AssetId = t.AssetId,
                    FromLocationId = t.FromLocationId, FromLocationName = t.FromLocation.Name,
                    ToLocationId = t.ToLocationId, ToLocationName = t.ToLocation.Name,
                    FromUserId = t.FromUserId,
                    FromUserName = t.FromUser != null ? t.FromUser.FirstName + " " + t.FromUser.LastName : null,
                    ToUserId = t.ToUserId,
                    ToUserName = t.ToUser != null ? t.ToUser.FirstName + " " + t.ToUser.LastName : null,
                    Reason = t.Reason, Status = t.Status, Notes = t.Notes,
                    TransferDate = t.TransferDate, RequestedByName = t.RequestedByName
                }).ToListAsync();

            var maintenance = await _context.MaintenanceRequests
                .Where(m => m.AssetId == assetId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MaintenanceRequestDto
                {
                    Id = m.Id, AssetId = m.AssetId, RequestType = m.RequestType,
                    Description = m.Description, OldSpecifications = m.OldSpecifications,
                    NewSpecifications = m.NewSpecifications, VendorName = m.VendorName,
                    Cost = m.Cost, ScheduledDate = m.ScheduledDate, CompletedDate = m.CompletedDate,
                    Status = m.Status, Resolution = m.Resolution, Remarks = m.Remarks,
                    CreatedAt = m.CreatedAt, CreatedByName = m.CreatedByName
                }).ToListAsync();

            var compliance = await _context.ComplianceChecks
                .Where(c => c.AssetId == assetId)
                .OrderByDescending(c => c.CheckedAt)
                .Select(c => new ComplianceCheckDto
                {
                    Id = c.Id, AssetId = c.AssetId, CheckType = c.CheckType,
                    Result = c.Result, Details = c.Details, Remediation = c.Remediation,
                    Status = c.Status, CheckedAt = c.CheckedAt, CheckedByName = c.CheckedByName,
                    ResolvedAt = c.ResolvedAt
                }).ToListAsync();

            return Ok(new AssetLifecycleSummaryDto
            {
                AssetId = assetId,
                AssetTag = asset.AssetTag,
                AssignmentHistory = assignments,
                TransferHistory = transfers,
                MaintenanceRequests = maintenance,
                ComplianceChecks = compliance,
                OpenMaintenanceCount = maintenance.Count(m => m.Status == "Open" || m.Status == "In Progress"),
                FailedComplianceCount = compliance.Count(c => c.Result == "Fail" && c.Status == "Open")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lifecycle for asset {AssetId}", assetId);
            return StatusCode(500, new { message = "Error retrieving lifecycle data" });
        }
    }

    // ── Transfer ──────────────────────────────────────────────────────────────

    // POST /api/assets/{assetId}/lifecycle/transfer
    [HttpPost("transfer")]
    public async Task<IActionResult> TransferAsset(int assetId, [FromBody] CreateTransferDto dto)
    {
        try
        {
            var asset = await _context.Assets
                .Include(a => a.AssignedUser)
                .Include(a => a.Location)
                .FirstOrDefaultAsync(a => a.Id == assetId);
            if (asset == null) return NotFound(new { message = "Asset not found" });

            var toLocation = await _context.Locations.FindAsync(dto.ToLocationId);
            if (toLocation == null) return BadRequest(new { message = "Target location not found" });

            var userId = GetCurrentUserId() ?? 1;
            var currentUser = await _context.Users.FindAsync(userId);
            var currentUserName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "System";

            // Log assignment history if user or location changed
            var history = new AssetAssignmentHistory
            {
                AssetId = assetId,
                PreviousUserId = asset.AssignedUserId,
                PreviousUserName = asset.AssignedUser != null ? $"{asset.AssignedUser.FirstName} {asset.AssignedUser.LastName}" : null,
                NewUserId = dto.ToUserId,
                PreviousLocationId = asset.LocationId,
                PreviousLocationName = asset.Location?.Name,
                NewLocationId = dto.ToLocationId,
                NewLocationName = toLocation.Name,
                Reason = dto.Reason,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = userId,
                ChangedByName = currentUserName
            };

            if (dto.ToUserId.HasValue)
            {
                var toUser = await _context.Users.FindAsync(dto.ToUserId.Value);
                history.NewUserName = toUser != null ? $"{toUser.FirstName} {toUser.LastName}" : null;
            }

            // Create transfer record
            var transfer = new AssetTransferRequest
            {
                AssetId = assetId,
                FromLocationId = asset.LocationId,
                ToLocationId = dto.ToLocationId,
                FromUserId = asset.AssignedUserId,
                ToUserId = dto.ToUserId,
                Reason = dto.Reason,
                Notes = dto.Notes,
                Status = "Completed",
                TransferDate = DateTime.UtcNow,
                RequestedBy = userId,
                RequestedByName = currentUserName
            };

            // Update asset
            asset.LocationId = dto.ToLocationId;
            if (dto.ToUserId.HasValue) asset.AssignedUserId = dto.ToUserId;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = userId;

            _context.AssetAssignmentHistories.Add(history);
            _context.AssetTransferRequests.Add(transfer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Asset {AssetId} transferred to location {LocationId} by user {UserId}", assetId, dto.ToLocationId, userId);
            return Ok(new { message = "Asset transferred successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring asset {AssetId}", assetId);
            return StatusCode(500, new { message = "Error transferring asset" });
        }
    }

    // ── Maintenance ───────────────────────────────────────────────────────────

    // POST /api/assets/{assetId}/lifecycle/maintenance
    [HttpPost("maintenance")]
    public async Task<ActionResult<MaintenanceRequestDto>> CreateMaintenance(int assetId, [FromBody] CreateMaintenanceDto dto)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null) return NotFound(new { message = "Asset not found" });

            var userId = GetCurrentUserId() ?? 1;
            var currentUser = await _context.Users.FindAsync(userId);

            var request = new MaintenanceRequest
            {
                AssetId = assetId,
                RequestType = dto.RequestType,
                Description = dto.Description,
                OldSpecifications = dto.OldSpecifications,
                NewSpecifications = dto.NewSpecifications,
                VendorName = dto.VendorName,
                Cost = dto.Cost,
                ScheduledDate = dto.ScheduledDate,
                Remarks = dto.Remarks,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                CreatedByName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "System"
            };

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new MaintenanceRequestDto
            {
                Id = request.Id, AssetId = request.AssetId, RequestType = request.RequestType,
                Description = request.Description, OldSpecifications = request.OldSpecifications,
                NewSpecifications = request.NewSpecifications, VendorName = request.VendorName,
                Cost = request.Cost, ScheduledDate = request.ScheduledDate,
                Status = request.Status, Remarks = request.Remarks,
                CreatedAt = request.CreatedAt, CreatedByName = request.CreatedByName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating maintenance request for asset {AssetId}", assetId);
            return StatusCode(500, new { message = "Error creating maintenance request" });
        }
    }

    // PUT /api/assets/{assetId}/lifecycle/maintenance/{id}
    [HttpPut("maintenance/{id:int}")]
    public async Task<IActionResult> UpdateMaintenance(int assetId, int id, [FromBody] UpdateMaintenanceDto dto)
    {
        try
        {
            var request = await _context.MaintenanceRequests.FirstOrDefaultAsync(m => m.Id == id && m.AssetId == assetId);
            if (request == null) return NotFound(new { message = "Maintenance request not found" });

            if (!string.IsNullOrEmpty(dto.Status)) request.Status = dto.Status;
            if (dto.Resolution != null) request.Resolution = dto.Resolution;
            if (dto.CompletedDate.HasValue) request.CompletedDate = dto.CompletedDate;
            if (dto.Cost.HasValue) request.Cost = dto.Cost;
            if (dto.VendorName != null) request.VendorName = dto.VendorName;
            if (dto.NewSpecifications != null) request.NewSpecifications = dto.NewSpecifications;
            if (dto.Remarks != null) request.Remarks = dto.Remarks;
            request.UpdatedAt = DateTime.UtcNow;
            request.UpdatedBy = GetCurrentUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating maintenance request {Id}", id);
            return StatusCode(500, new { message = "Error updating maintenance request" });
        }
    }

    // ── Compliance ────────────────────────────────────────────────────────────

    // POST /api/assets/{assetId}/lifecycle/compliance
    [HttpPost("compliance")]
    public async Task<ActionResult<ComplianceCheckDto>> CreateComplianceCheck(int assetId, [FromBody] CreateComplianceCheckDto dto)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null) return NotFound(new { message = "Asset not found" });

            var userId = GetCurrentUserId() ?? 1;
            var currentUser = await _context.Users.FindAsync(userId);

            var check = new ComplianceCheck
            {
                AssetId = assetId,
                CheckType = dto.CheckType,
                Result = dto.Result,
                Details = dto.Details,
                Remediation = dto.Remediation,
                Status = dto.Result == "Pass" ? "Resolved" : "Open",
                CheckedAt = DateTime.UtcNow,
                CheckedBy = userId,
                CheckedByName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "System"
            };

            _context.ComplianceChecks.Add(check);
            await _context.SaveChangesAsync();

            return Ok(new ComplianceCheckDto
            {
                Id = check.Id, AssetId = check.AssetId, CheckType = check.CheckType,
                Result = check.Result, Details = check.Details, Remediation = check.Remediation,
                Status = check.Status, CheckedAt = check.CheckedAt, CheckedByName = check.CheckedByName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating compliance check for asset {AssetId}", assetId);
            return StatusCode(500, new { message = "Error creating compliance check" });
        }
    }

    // PUT /api/assets/{assetId}/lifecycle/compliance/{id}/resolve
    [HttpPut("compliance/{id:int}/resolve")]
    public async Task<IActionResult> ResolveCompliance(int assetId, int id, [FromBody] ResolveComplianceDto dto)
    {
        try
        {
            var check = await _context.ComplianceChecks.FirstOrDefaultAsync(c => c.Id == id && c.AssetId == assetId);
            if (check == null) return NotFound(new { message = "Compliance check not found" });

            check.Status = "Resolved";
            check.ResolvedAt = DateTime.UtcNow;
            check.ResolvedBy = GetCurrentUserId();
            if (dto.Resolution != null) check.Remediation = dto.Resolution;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving compliance check {Id}", id);
            return StatusCode(500, new { message = "Error resolving compliance check" });
        }
    }

    // POST /api/assets/{assetId}/lifecycle/compliance/run-auto
    [HttpPost("compliance/run-auto")]
    public async Task<ActionResult<List<ComplianceCheckDto>>> RunAutoComplianceCheck(int assetId)
    {
        try
        {
            var asset = await _context.Assets
                .Include(a => a.PatchStatus)
                .Include(a => a.USBBlockingStatus)
                .FirstOrDefaultAsync(a => a.Id == assetId);
            if (asset == null) return NotFound(new { message = "Asset not found" });

            var userId = GetCurrentUserId() ?? 1;
            var currentUser = await _context.Users.FindAsync(userId);
            var checkerName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "System";
            var results = new List<ComplianceCheck>();

            // Check 1: Patch Status
            var patchResult = asset.PatchStatus?.Name?.ToLower() switch
            {
                "patched" or "up to date" => "Pass",
                "pending" => "Warning",
                _ => "Fail"
            };
            results.Add(new ComplianceCheck
            {
                AssetId = assetId, CheckType = "PatchStatus",
                Result = patchResult,
                Details = $"Patch status: {asset.PatchStatus?.Name ?? "Not set"}",
                Remediation = patchResult != "Pass" ? "Apply latest OS and DB patches" : null,
                Status = patchResult == "Pass" ? "Resolved" : "Open",
                CheckedAt = DateTime.UtcNow, CheckedBy = userId, CheckedByName = checkerName
            });

            // Check 2: USB Blocking
            var usbResult = asset.USBBlockingStatus?.Name?.ToLower() switch
            {
                "enabled" => "Pass",
                "not applicable" => "Pass",
                _ => "Fail"
            };
            results.Add(new ComplianceCheck
            {
                AssetId = assetId, CheckType = "USBBlocking",
                Result = usbResult,
                Details = $"USB blocking status: {asset.USBBlockingStatus?.Name ?? "Not set"}",
                Remediation = usbResult == "Fail" ? "Enable USB blocking via endpoint security policy" : null,
                Status = usbResult == "Pass" ? "Resolved" : "Open",
                CheckedAt = DateTime.UtcNow, CheckedBy = userId, CheckedByName = checkerName
            });

            // Check 3: Warranty Expiry
            string warrantyResult = "Pass";
            string warrantyDetails = "Warranty information not available";
            if (asset.WarrantyEndDate.HasValue)
            {
                var daysLeft = (asset.WarrantyEndDate.Value - DateTime.UtcNow).TotalDays;
                warrantyResult = daysLeft < 0 ? "Fail" : daysLeft < 90 ? "Warning" : "Pass";
                warrantyDetails = daysLeft < 0
                    ? $"Warranty expired on {asset.WarrantyEndDate.Value:dd MMM yyyy}"
                    : $"Warranty valid until {asset.WarrantyEndDate.Value:dd MMM yyyy} ({(int)daysLeft} days remaining)";
            }
            results.Add(new ComplianceCheck
            {
                AssetId = assetId, CheckType = "WarrantyExpiry",
                Result = warrantyResult,
                Details = warrantyDetails,
                Remediation = warrantyResult == "Fail" ? "Renew warranty or plan for asset replacement" : null,
                Status = warrantyResult == "Pass" ? "Resolved" : "Open",
                CheckedAt = DateTime.UtcNow, CheckedBy = userId, CheckedByName = checkerName
            });

            _context.ComplianceChecks.AddRange(results);
            await _context.SaveChangesAsync();

            return Ok(results.Select(c => new ComplianceCheckDto
            {
                Id = c.Id, AssetId = c.AssetId, CheckType = c.CheckType,
                Result = c.Result, Details = c.Details, Remediation = c.Remediation,
                Status = c.Status, CheckedAt = c.CheckedAt, CheckedByName = c.CheckedByName
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running auto compliance check for asset {AssetId}", assetId);
            return StatusCode(500, new { message = "Error running compliance check" });
        }
    }
}
