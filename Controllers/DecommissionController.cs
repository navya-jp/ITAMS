using Microsoft.AspNetCore.Mvc;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using ITAMS.Domain.Entities.Workflow;
using ITAMS.Models;
using ITAMS.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DecommissionController : BaseController
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<DecommissionController> _logger;

    public DecommissionController(ITAMSDbContext context, ILogger<DecommissionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ── TASK 1: EOL Candidates ────────────────────────────────────────────────

    [HttpGet("end-of-life-candidates")]
    public async Task<IActionResult> GetEolCandidates(
        [FromQuery] int? projectId,
        [FromQuery] int warrantyExpiredDays = 365,
        [FromQuery] int ageYears = 5)
    {
        var today = DateTimeHelper.Now;
        var warrantyThreshold = today.AddDays(-warrantyExpiredDays);
        var ageThreshold = today.AddYears(-ageYears);

        var decommissionedStatusId = await _context.AssetStatuses
            .Where(s => s.StatusName == "Decommissioned")
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        var repairStatusId = await _context.AssetStatuses
            .Where(s => s.StatusName.ToLower().Contains("repair"))
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        var query = _context.Assets
            .Include(a => a.AssetStatus)
            .Include(a => a.AssetType)
            .Include(a => a.Location)
            .Include(a => a.Project)
            .Where(a => !a.IsDecommissioned && a.AssetStatusId != decommissionedStatusId)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(a => a.ProjectId == projectId.Value);

        var assets = await query.ToListAsync();

        // Get pending decommission request asset IDs
        var pendingAssetIds = await _context.ApprovalRequests
            .Where(r => r.RequestType == "ASSET_DECOMMISSION" && r.Status == "PENDING")
            .Select(r => r.AssetId)
            .ToListAsync();

        var candidates = new List<EolCandidateDto>();

        foreach (var asset in assets)
        {
            var reasons = new List<string>();
            int? daysSinceWarranty = null;
            double? assetAge = null;

            if (asset.WarrantyEndDate.HasValue && asset.WarrantyEndDate.Value < warrantyThreshold)
            {
                daysSinceWarranty = (int)(today - asset.WarrantyEndDate.Value).TotalDays;
                reasons.Add($"Warranty expired {daysSinceWarranty} days ago");
            }

            if (asset.CommissioningDate.HasValue && asset.CommissioningDate.Value < ageThreshold)
            {
                assetAge = (today - asset.CommissioningDate.Value).TotalDays / 365.25;
                reasons.Add($"Asset age: {assetAge:F1} years");
            }

            if (repairStatusId > 0 && asset.AssetStatusId == repairStatusId)
            {
                var daysInRepair = asset.UpdatedAt.HasValue
                    ? (int)(today - asset.UpdatedAt.Value).TotalDays
                    : 0;
                if (daysInRepair >= 30)
                    reasons.Add($"In Repair for {daysInRepair} days");
            }

            if (reasons.Count == 0) continue;

            candidates.Add(new EolCandidateDto
            {
                Id = asset.Id,
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                Make = asset.Make,
                Model = asset.Model,
                AssetType = asset.AssetType?.TypeName,
                Location = asset.Location?.Name,
                Project = asset.Project?.Name,
                WarrantyEndDate = asset.WarrantyEndDate,
                CommissioningDate = asset.CommissioningDate,
                CurrentStatus = asset.AssetStatus?.StatusName,
                EolReason = string.Join("; ", reasons),
                DaysSinceWarrantyExpired = daysSinceWarranty,
                AssetAgeYears = assetAge,
                HasPendingRequest = pendingAssetIds.Contains(asset.Id)
            });
        }

        return Ok(candidates);
    }

    // ── TASK 2: Initiate Decommission ─────────────────────────────────────────

    [HttpPost("initiate")]
    public async Task<IActionResult> InitiateDecommission([FromBody] InitiateDecommissionDto dto)
    {
        var asset = await _context.Assets
            .Include(a => a.AssetStatus)
            .Include(a => a.Location)
            .FirstOrDefaultAsync(a => a.Id == dto.AssetId);

        if (asset == null) return NotFound(new { message = "Asset not found" });
        if (asset.IsDecommissioned) return BadRequest(new { message = "Asset is already decommissioned" });
        if (asset.AssetStatus?.StatusName == "Decommissioned")
            return BadRequest(new { message = "Asset is already decommissioned" });

        var existing = await _context.ApprovalRequests
            .AnyAsync(r => r.AssetId == dto.AssetId && r.RequestType == "ASSET_DECOMMISSION" && r.Status == "PENDING");
        if (existing) return BadRequest(new { message = "A decommission request is already pending for this asset" });

        var workflow = await GetOrCreateDecommissionWorkflow();
        var userId = GetCurrentUserId() ?? 1;
        var user = await _context.Users.FindAsync(userId);

        var request = new ApprovalRequest
        {
            WorkflowId = workflow.Id,
            RequestType = "ASSET_DECOMMISSION",
            RequestedBy = userId,
            RequestedAt = DateTimeHelper.Now,
            Status = "PENDING",
            CurrentLevel = 1,
            AssetId = dto.AssetId,
            RequestDetails = JsonSerializer.Serialize(new
            {
                reason = dto.Reason,
                disposalMethod = dto.DisposalMethod,
                notes = dto.Notes
            })
        };

        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Decommission initiated for asset {AssetId} by user {UserId}", dto.AssetId, userId);

        return Ok(await BuildRequestDto(request, asset));
    }

    // ── TASK 3: List Requests ─────────────────────────────────────────────────

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests([FromQuery] string? status, [FromQuery] int? projectId)
    {
        var query = _context.ApprovalRequests
            .Where(r => r.RequestType == "ASSET_DECOMMISSION")
            .AsQueryable();

        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status.ToUpper());

        var requests = await query.OrderByDescending(r => r.RequestedAt).ToListAsync();
        var assetIds = requests.Where(r => r.AssetId.HasValue).Select(r => r.AssetId!.Value).Distinct().ToList();
        var assets = await _context.Assets.Include(a => a.Location)
            .Where(a => assetIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);

        if (projectId.HasValue)
            requests = requests.Where(r => r.AssetId.HasValue && assets.ContainsKey(r.AssetId.Value) && assets[r.AssetId.Value].ProjectId == projectId.Value).ToList();

        var dtos = new List<DecommissionRequestDto>();
        foreach (var r in requests)
        {
            var asset = r.AssetId.HasValue && assets.ContainsKey(r.AssetId.Value) ? assets[r.AssetId.Value] : null;
            dtos.Add(await BuildRequestDto(r, asset));
        }
        return Ok(dtos);
    }

    [HttpGet("requests/{requestId}")]
    public async Task<IActionResult> GetRequest(int requestId)
    {
        var request = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.RequestType == "ASSET_DECOMMISSION");
        if (request == null) return NotFound();
        var asset = request.AssetId.HasValue ? await _context.Assets.Include(a => a.Location).FirstOrDefaultAsync(a => a.Id == request.AssetId.Value) : null;
        return Ok(await BuildRequestDto(request, asset));
    }

    [HttpGet("requests/my-pending")]
    public async Task<IActionResult> GetMyPending()
    {
        var userId = GetCurrentUserId() ?? 1;
        var requests = await _context.ApprovalRequests
            .Where(r => r.RequestType == "ASSET_DECOMMISSION" && r.Status == "PENDING")
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        var assetIds = requests.Where(r => r.AssetId.HasValue).Select(r => r.AssetId!.Value).Distinct().ToList();
        var assets = await _context.Assets.Include(a => a.Location)
            .Where(a => assetIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);

        var dtos = new List<DecommissionRequestDto>();
        foreach (var r in requests)
        {
            var asset = r.AssetId.HasValue && assets.ContainsKey(r.AssetId.Value) ? assets[r.AssetId.Value] : null;
            dtos.Add(await BuildRequestDto(r, asset));
        }

        return Ok(dtos);
    }

    // ── TASK 3: Approve / Reject ──────────────────────────────────────────────

    [HttpPost("requests/{requestId}/approve")]
    public async Task<IActionResult> Approve(int requestId, [FromBody] ApproveDecommissionDto dto)
    {
        var request = await _context.ApprovalRequests
            .Include(r => r.Workflow).ThenInclude(w => w!.ApprovalLevels)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.RequestType == "ASSET_DECOMMISSION");

        if (request == null) return NotFound();
        if (request.Status != "PENDING") return BadRequest(new { message = "Request is not pending" });

        var userId = GetCurrentUserId() ?? 1;
        var history = new ApprovalHistory
        {
            RequestId = requestId,
            Level = request.CurrentLevel,
            ApproverId = userId,
            Action = "APPROVED",
            ActionAt = DateTimeHelper.Now,
            Comments = dto.Comments
        };
        _context.ApprovalHistories.Add(history);

        var levels = request.Workflow?.ApprovalLevels?.OrderBy(l => l.LevelOrder).ToList() ?? new();
        var nextLevel = levels.FirstOrDefault(l => l.LevelOrder > request.CurrentLevel);

        if (nextLevel != null)
        {
            request.CurrentLevel = nextLevel.LevelOrder;
        }
        else
        {
            request.Status = "APPROVED";
            request.CompletedAt = DateTimeHelper.Now;
            request.CompletedBy = userId;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();
                await ExecuteDecommission(request.AssetId!.Value, userId);
                await FreezeAssetRecords(request.AssetId!.Value, userId);
                await ArchiveAsset(request.AssetId!.Value, requestId, userId, request.RequestDetails);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            return Ok(new { message = "Asset decommissioned and archived successfully" });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Approved — escalated to next level" });
    }

    [HttpPost("requests/{requestId}/reject")]
    public async Task<IActionResult> Reject(int requestId, [FromBody] RejectDecommissionDto dto)
    {
        var request = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.RequestType == "ASSET_DECOMMISSION");

        if (request == null) return NotFound();
        if (request.Status != "PENDING") return BadRequest(new { message = "Request is not pending" });

        var userId = GetCurrentUserId() ?? 1;

        _context.ApprovalHistories.Add(new ApprovalHistory
        {
            RequestId = requestId,
            Level = request.CurrentLevel,
            ApproverId = userId,
            Action = "REJECTED",
            ActionAt = DateTimeHelper.Now,
            Comments = dto.Reason
        });

        request.Status = "REJECTED";
        request.RejectionReason = dto.Reason;
        request.CompletedAt = DateTimeHelper.Now;
        request.CompletedBy = userId;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Decommission request rejected" });
    }

    // ── TASK 6: Archive endpoints ─────────────────────────────────────────────

    [HttpGet("archive")]
    public async Task<IActionResult> GetArchive([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var total = await _context.DecommissionArchives.CountAsync();
        var archives = await _context.DecommissionArchives
            .OrderByDescending(a => a.ArchivedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new DecommissionArchiveDto
            {
                Id = a.Id, AssetId = a.AssetId, AssetTag = a.AssetTag,
                DecommissionReason = a.DecommissionReason, DisposalMethod = a.DisposalMethod,
                Notes = a.Notes, ArchivedAt = a.ArchivedAt, ArchivedByName = a.ArchivedByName,
                AssetSnapshot = a.AssetSnapshot, ApprovalChainSnapshot = a.ApprovalChainSnapshot
            }).ToListAsync();

        return Ok(new { total, page, pageSize, archives });
    }

    [HttpGet("archive/{assetId}")]
    public async Task<IActionResult> GetArchiveByAsset(int assetId)
    {
        var archive = await _context.DecommissionArchives
            .Where(a => a.AssetId == assetId)
            .OrderByDescending(a => a.ArchivedAt)
            .FirstOrDefaultAsync();

        if (archive == null) return NotFound();

        return Ok(new DecommissionArchiveDto
        {
            Id = archive.Id, AssetId = archive.AssetId, AssetTag = archive.AssetTag,
            DecommissionReason = archive.DecommissionReason, DisposalMethod = archive.DisposalMethod,
            Notes = archive.Notes, ArchivedAt = archive.ArchivedAt, ArchivedByName = archive.ArchivedByName,
            AssetSnapshot = archive.AssetSnapshot, ApprovalChainSnapshot = archive.ApprovalChainSnapshot
        });
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task ExecuteDecommission(int assetId, int approvedBy)
    {
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null) return;

        var status = await _context.AssetStatuses.FirstOrDefaultAsync(s => s.StatusName == "Decommissioned");
        if (status != null) asset.AssetStatusId = status.Id;

        asset.UpdatedAt = DateTimeHelper.Now;
        asset.UpdatedBy = approvedBy;
        await _context.SaveChangesAsync();
    }

    private async Task FreezeAssetRecords(int assetId, int decommissionedBy)
    {
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null) return;

        asset.IsDecommissioned = true;
        asset.DecommissionedAt = DateTimeHelper.Now;
        asset.DecommissionedBy = decommissionedBy;
        await _context.SaveChangesAsync();
    }

    private async Task ArchiveAsset(int assetId, int approvalRequestId, int archivedBy, string? requestDetails)
    {
        var asset = await _context.Assets
            .Include(a => a.Project).Include(a => a.Location)
            .Include(a => a.AssetType).Include(a => a.SubType)
            .Include(a => a.Vendor).Include(a => a.AssetStatus)
            .FirstOrDefaultAsync(a => a.Id == assetId);
        if (asset == null) return;

        var approvalHistory = await _context.ApprovalHistories
            .Where(h => h.RequestId == approvalRequestId)
            .OrderBy(h => h.ActionAt)
            .ToListAsync();

        var user = await _context.Users.FindAsync(archivedBy);
        var archivedByName = user != null ? $"{user.FirstName} {user.LastName}" : "System";

        var details = requestDetails != null
            ? JsonSerializer.Deserialize<JsonElement>(requestDetails)
            : default;

        var archive = new DecommissionArchive
        {
            AssetId = assetId,
            AssetTag = asset.AssetTag,
            AssetSnapshot = JsonSerializer.Serialize(new
            {
                asset.AssetId, asset.AssetTag, asset.Make, asset.Model,
                asset.SerialNumber, asset.AssetTypeName,
                Project = asset.Project?.Name,
                Location = asset.Location?.Name,
                Status = asset.AssetStatus?.StatusName,
                asset.WarrantyEndDate, asset.CommissioningDate,
                asset.ProcurementDate, asset.ProcurementCost
            }),
            DecommissionReason = details.ValueKind != JsonValueKind.Undefined
                ? details.GetProperty("reason").GetString() ?? ""
                : "",
            DisposalMethod = details.ValueKind != JsonValueKind.Undefined
                ? details.GetProperty("disposalMethod").GetString() ?? ""
                : "",
            Notes = details.ValueKind != JsonValueKind.Undefined && details.TryGetProperty("notes", out var n)
                ? n.GetString()
                : null,
            ApprovalRequestId = approvalRequestId,
            ApprovalChainSnapshot = JsonSerializer.Serialize(approvalHistory.Select(h => new
            {
                h.Level, h.Action, h.ActionAt, h.Comments,
                ApproverName = _context.Users.Find(h.ApproverId) is { } u ? $"{u.FirstName} {u.LastName}" : "Unknown"
            })),
            ArchivedAt = DateTimeHelper.Now,
            ArchivedBy = archivedBy,
            ArchivedByName = archivedByName
        };

        _context.DecommissionArchives.Add(archive);
        await _context.SaveChangesAsync();
    }

    private async Task<ApprovalWorkflow> GetOrCreateDecommissionWorkflow()
    {
        var workflow = await _context.ApprovalWorkflows
            .Include(w => w.ApprovalLevels)
            .FirstOrDefaultAsync(w => w.WorkflowType == "ASSET_DECOMMISSION" && w.IsActive);

        if (workflow != null) return workflow;

        workflow = new ApprovalWorkflow
        {
            WorkflowName = "Asset Decommission Approval",
            WorkflowType = "ASSET_DECOMMISSION",
            IsActive = true,
            CreatedAt = DateTimeHelper.Now,
            CreatedBy = 1
        };
        _context.ApprovalWorkflows.Add(workflow);
        await _context.SaveChangesAsync();

        _context.ApprovalLevels.AddRange(
            new ApprovalLevel { WorkflowId = workflow.Id, LevelOrder = 1, LevelName = "Admin Approval", RequiredApproverRoles = "[\"Admin\"]", TimeoutHours = 48, ApprovalType = "ANY" },
            new ApprovalLevel { WorkflowId = workflow.Id, LevelOrder = 2, LevelName = "Super Admin Approval", RequiredApproverRoles = "[\"Super Admin\"]", TimeoutHours = 48, ApprovalType = "ANY" }
        );
        await _context.SaveChangesAsync();

        return workflow;
    }

    private async Task<DecommissionRequestDto> BuildRequestDto(ApprovalRequest request, Asset? asset)
    {
        var requestedByUser = await _context.Users.FindAsync(request.RequestedBy);
        var history = await _context.ApprovalHistories
            .Where(h => h.RequestId == request.Id)
            .OrderBy(h => h.ActionAt)
            .ToListAsync();

        JsonElement details = default;
        if (!string.IsNullOrEmpty(request.RequestDetails))
            details = JsonSerializer.Deserialize<JsonElement>(request.RequestDetails);

        return new DecommissionRequestDto
        {
            Id = request.Id,
            AssetId = asset?.Id ?? 0,
            AssetTag = asset?.AssetTag ?? "",
            AssetMake = asset?.Make ?? "",
            AssetModel = asset?.Model ?? "",
            AssetLocation = asset?.Location?.Name,
            RequestType = request.RequestType,
            Status = request.Status,
            CurrentLevel = request.CurrentLevel,
            Reason = details.ValueKind != JsonValueKind.Undefined && details.TryGetProperty("reason", out var r) ? r.GetString() : null,
            DisposalMethod = details.ValueKind != JsonValueKind.Undefined && details.TryGetProperty("disposalMethod", out var d) ? d.GetString() : null,
            Notes = details.ValueKind != JsonValueKind.Undefined && details.TryGetProperty("notes", out var n) ? n.GetString() : null,
            RequestedAt = request.RequestedAt,
            RequestedByName = requestedByUser != null ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" : null,
            RejectionReason = request.RejectionReason,
            CompletedAt = request.CompletedAt,
            ApprovalHistory = history.Select(h => new ApprovalHistoryDto
            {
                Level = h.Level,
                Action = h.Action,
                ActionAt = h.ActionAt,
                Comments = h.Comments
            }).ToList()
        };
    }
}
