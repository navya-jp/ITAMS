using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using ITAMS.Models;
using ITAMS.Services;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceAssetsController : BaseController
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<ServiceAssetsController> _logger;
    private readonly IAssetIdGeneratorService _assetIdGeneratorService;

    public ServiceAssetsController(
        ITAMSDbContext context,
        ILogger<ServiceAssetsController> logger,
        IAssetIdGeneratorService assetIdGeneratorService)
    {
        _context = context;
        _logger = logger;
        _assetIdGeneratorService = assetIdGeneratorService;
    }

    private static string CalculateStatus(ServiceAsset a)
    {
        if (a.Status == "Cancelled") return "Cancelled";
        var today = DateTime.UtcNow.Date;
        if (a.ContractEndDate.Date < today) return "Expired";
        if (a.ContractEndDate.Date <= today.AddDays(a.RenewalReminderDays)) return "Expiring";
        return "Active";
    }

    private ServiceAssetDto MapToDto(ServiceAsset a) => new()
    {
        Id = a.Id,
        AssetId = a.AssetId,
        ServiceName = a.ServiceName,
        ServiceTypeId = a.ServiceTypeId,
        ServiceTypeName = a.ServiceType?.TypeName,
        ProjectId = a.ProjectId,
        LocationId = a.LocationId,
        LocationName = a.Location?.Name,
        VendorId = a.VendorId,
        VendorName = a.VendorName,
        ContractNumber = a.ContractNumber,
        ContractStartDate = a.ContractStartDate,
        ContractEndDate = a.ContractEndDate,
        RenewalCycleMonths = a.RenewalCycleMonths,
        RenewalReminderDays = a.RenewalReminderDays,
        ContractCost = a.ContractCost,
        BillingCycle = a.BillingCycle,
        Currency = a.Currency,
        SLAType = a.SLAType,
        ResponseTime = a.ResponseTime,
        CoverageDetails = a.CoverageDetails,
        ContactPerson = a.ContactPerson,
        SupportContactNumber = a.SupportContactNumber,
        Description = a.Description,
        Remarks = a.Remarks,
        UsageCategory = a.UsageCategory,
        Status = CalculateStatus(a),
        LastRenewalDate = a.LastRenewalDate,
        NextRenewalDate = a.NextRenewalDate,
        AutoRenewEnabled = a.AutoRenewEnabled,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        Renewals = a.Renewals.OrderByDescending(r => r.RenewalDate).Select(r => new ServiceRenewalDto
        {
            Id = r.Id,
            PreviousEndDate = r.PreviousEndDate,
            NewStartDate = r.NewStartDate,
            NewEndDate = r.NewEndDate,
            RenewalCost = r.RenewalCost,
            RenewedBy = r.RenewedBy,
            RenewalDate = r.RenewalDate,
            Remarks = r.Remarks
        }).ToList()
    };

    private IQueryable<ServiceAsset> BaseQuery() =>
        _context.ServiceAssets
            .Include(a => a.ServiceType)
            .Include(a => a.Location)
            .Include(a => a.Renewals);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceAssetDto>>> GetServiceAssets()
    {
        try
        {
            var assets = await BaseQuery().OrderByDescending(a => a.CreatedAt).ToListAsync();
            return Ok(assets.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service assets");
            return StatusCode(500, new { message = "Error retrieving service assets" });
        }
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<IEnumerable<ServiceAssetDto>>> GetExpiringServices()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var assets = await BaseQuery()
                .Where(a => a.Status != "Cancelled" && a.ContractEndDate.Date >= today &&
                            a.ContractEndDate.Date <= today.AddDays(a.RenewalReminderDays))
                .OrderBy(a => a.ContractEndDate)
                .ToListAsync();
            return Ok(assets.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expiring services");
            return StatusCode(500, new { message = "Error retrieving expiring services" });
        }
    }

    [HttpGet("expired")]
    public async Task<ActionResult<IEnumerable<ServiceAssetDto>>> GetExpiredServices()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var assets = await BaseQuery()
                .Where(a => a.ContractEndDate.Date < today && a.Status != "Cancelled")
                .OrderByDescending(a => a.ContractEndDate)
                .ToListAsync();
            return Ok(assets.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expired services");
            return StatusCode(500, new { message = "Error retrieving expired services" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceAssetDto>> GetServiceAsset(int id)
    {
        try
        {
            var asset = await BaseQuery().FirstOrDefaultAsync(a => a.Id == id);
            if (asset == null) return NotFound(new { message = "Service asset not found" });
            return Ok(MapToDto(asset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service asset {Id}", id);
            return StatusCode(500, new { message = "Error retrieving service asset" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ServiceAssetDto>> CreateServiceAsset([FromBody] CreateServiceAssetDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.ServiceName))
                return BadRequest(new { message = "Service name is required" });
            if (dto.ServiceTypeId <= 0)
                return BadRequest(new { message = "Service type is required" });
            if (string.IsNullOrWhiteSpace(dto.VendorName))
                return BadRequest(new { message = "Vendor name is required" });
            if (dto.ContractEndDate <= dto.ContractStartDate)
                return BadRequest(new { message = "Contract end date must be after start date" });

            var assetId = await _assetIdGeneratorService.GenerateServiceAssetIdAsync();

            var asset = new ServiceAsset
            {
                AssetId = assetId,
                ServiceName = dto.ServiceName,
                ServiceTypeId = dto.ServiceTypeId,
                ProjectId = dto.ProjectId,
                LocationId = dto.LocationId,
                VendorId = dto.VendorId,
                VendorName = dto.VendorName,
                ContractNumber = dto.ContractNumber,
                ContractStartDate = dto.ContractStartDate,
                ContractEndDate = dto.ContractEndDate,
                RenewalCycleMonths = dto.RenewalCycleMonths,
                RenewalReminderDays = dto.RenewalReminderDays,
                ContractCost = dto.ContractCost,
                BillingCycle = dto.BillingCycle,
                Currency = dto.Currency,
                SLAType = dto.SLAType,
                ResponseTime = dto.ResponseTime,
                CoverageDetails = dto.CoverageDetails,
                ContactPerson = dto.ContactPerson,
                SupportContactNumber = dto.SupportContactNumber,
                Description = dto.Description,
                Remarks = dto.Remarks,
                UsageCategory = dto.UsageCategory,
                AutoRenewEnabled = dto.AutoRenewEnabled,
                NextRenewalDate = dto.ContractEndDate,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId() ?? 1
            };

            _context.ServiceAssets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service asset {AssetId} created by user {UserId}", asset.AssetId, GetCurrentUserId());

            var created = await BaseQuery().FirstAsync(a => a.Id == asset.Id);
            return CreatedAtAction(nameof(GetServiceAsset), new { id = asset.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service asset");
            return StatusCode(500, new { message = "Error creating service asset: " + ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServiceAsset(int id, [FromBody] UpdateServiceAssetDto dto)
    {
        try
        {
            var asset = await _context.ServiceAssets.FindAsync(id);
            if (asset == null) return NotFound(new { message = "Service asset not found" });

            if (!string.IsNullOrEmpty(dto.ServiceName)) asset.ServiceName = dto.ServiceName;
            if (dto.ServiceTypeId.HasValue && dto.ServiceTypeId > 0) asset.ServiceTypeId = dto.ServiceTypeId.Value;
            if (dto.LocationId.HasValue) asset.LocationId = dto.LocationId.Value;
            if (dto.VendorId.HasValue) asset.VendorId = dto.VendorId.Value;
            if (!string.IsNullOrEmpty(dto.VendorName)) asset.VendorName = dto.VendorName;
            if (dto.ContractNumber != null) asset.ContractNumber = dto.ContractNumber;
            if (dto.ContractStartDate.HasValue) asset.ContractStartDate = dto.ContractStartDate.Value;
            if (dto.ContractEndDate.HasValue) { asset.ContractEndDate = dto.ContractEndDate.Value; asset.NextRenewalDate = dto.ContractEndDate.Value; }
            if (dto.RenewalCycleMonths.HasValue) asset.RenewalCycleMonths = dto.RenewalCycleMonths.Value;
            if (dto.RenewalReminderDays.HasValue) asset.RenewalReminderDays = dto.RenewalReminderDays.Value;
            if (dto.ContractCost.HasValue) asset.ContractCost = dto.ContractCost.Value;
            if (dto.BillingCycle != null) asset.BillingCycle = dto.BillingCycle;
            if (!string.IsNullOrEmpty(dto.Currency)) asset.Currency = dto.Currency;
            if (dto.SLAType != null) asset.SLAType = dto.SLAType;
            if (dto.ResponseTime != null) asset.ResponseTime = dto.ResponseTime;
            if (dto.CoverageDetails != null) asset.CoverageDetails = dto.CoverageDetails;
            if (dto.ContactPerson != null) asset.ContactPerson = dto.ContactPerson;
            if (dto.SupportContactNumber != null) asset.SupportContactNumber = dto.SupportContactNumber;
            if (dto.Description != null) asset.Description = dto.Description;
            if (dto.Remarks != null) asset.Remarks = dto.Remarks;
            if (!string.IsNullOrEmpty(dto.UsageCategory)) asset.UsageCategory = dto.UsageCategory;
            if (!string.IsNullOrEmpty(dto.Status)) asset.Status = dto.Status;
            if (dto.AutoRenewEnabled.HasValue) asset.AutoRenewEnabled = dto.AutoRenewEnabled.Value;

            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = GetCurrentUserId();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Service asset {AssetId} updated by user {UserId}", asset.AssetId, GetCurrentUserId());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service asset {Id}", id);
            return StatusCode(500, new { message = "Error updating service asset" });
        }
    }

    [HttpPost("{id}/renew")]
    public async Task<IActionResult> RenewService(int id, [FromBody] RenewServiceDto dto)
    {
        try
        {
            var asset = await _context.ServiceAssets.FindAsync(id);
            if (asset == null) return NotFound(new { message = "Service asset not found" });

            if (dto.NewEndDate <= asset.ContractEndDate)
                return BadRequest(new { message = "New end date must be after current end date" });

            var renewal = new ServiceRenewal
            {
                ServiceId = asset.Id,
                PreviousEndDate = asset.ContractEndDate,
                NewStartDate = asset.ContractEndDate,
                NewEndDate = dto.NewEndDate,
                RenewalCost = dto.RenewalCost,
                RenewedBy = GetCurrentUserId() ?? 1,
                RenewalDate = DateTime.UtcNow,
                Remarks = dto.Remarks
            };

            asset.LastRenewalDate = DateTime.UtcNow;
            asset.ContractEndDate = dto.NewEndDate;
            asset.NextRenewalDate = dto.NewEndDate;
            asset.Status = "Active";
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = GetCurrentUserId();

            _context.ServiceRenewals.Add(renewal);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service asset {AssetId} renewed by user {UserId}", asset.AssetId, GetCurrentUserId());
            return Ok(new { message = "Service renewed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing service asset {Id}", id);
            return StatusCode(500, new { message = "Error renewing service: " + ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceAsset(int id)
    {
        try
        {
            var asset = await _context.ServiceAssets.FindAsync(id);
            if (asset == null) return NotFound(new { message = "Service asset not found" });

            _context.ServiceAssets.Remove(asset);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Service asset {AssetId} deleted by user {UserId}", asset.AssetId, GetCurrentUserId());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service asset {Id}", id);
            return StatusCode(500, new { message = "Error deleting service asset" });
        }
    }
}
