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

    // GET: api/serviceassets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceAssetDto>>> GetServiceAssets()
    {
        try
        {
            var assets = await _context.ServiceAssets
                .Include(a => a.ServiceType)
                .Include(a => a.ContractType)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ServiceAssetDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    ServiceName = a.ServiceName,
                    Description = a.Description,
                    ServiceTypeId = a.ServiceTypeId,
                    ServiceTypeName = a.ServiceType!.TypeName,
                    ContractTypeId = a.ContractTypeId,
                    ContractTypeName = a.ContractType!.TypeName,
                    Vendor = a.Vendor,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    RenewalCycle = a.RenewalCycle,
                    RenewalReminderDays = a.RenewalReminderDays,
                    LastRenewalDate = a.LastRenewalDate,
                    NextRenewalDate = a.NextRenewalDate,
                    Status = a.Status,
                    ContractValue = a.ContractValue,
                    ContractNumber = a.ContractNumber,
                    Remarks = a.Remarks,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service assets");
            return StatusCode(500, new { message = "Error retrieving service assets" });
        }
    }

    // GET: api/serviceassets/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceAssetDto>> GetServiceAsset(int id)
    {
        try
        {
            var asset = await _context.ServiceAssets
                .Include(a => a.ServiceType)
                .Include(a => a.ContractType)
                .Where(a => a.Id == id)
                .Select(a => new ServiceAssetDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    ServiceName = a.ServiceName,
                    Description = a.Description,
                    ServiceTypeId = a.ServiceTypeId,
                    ServiceTypeName = a.ServiceType!.TypeName,
                    ContractTypeId = a.ContractTypeId,
                    ContractTypeName = a.ContractType!.TypeName,
                    Vendor = a.Vendor,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    RenewalCycle = a.RenewalCycle,
                    RenewalReminderDays = a.RenewalReminderDays,
                    LastRenewalDate = a.LastRenewalDate,
                    NextRenewalDate = a.NextRenewalDate,
                    Status = a.Status,
                    ContractValue = a.ContractValue,
                    ContractNumber = a.ContractNumber,
                    Remarks = a.Remarks,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return NotFound(new { message = "Service asset not found" });
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service asset {Id}", id);
            return StatusCode(500, new { message = "Error retrieving service asset" });
        }
    }

    // POST: api/serviceassets
    [HttpPost]
    public async Task<ActionResult<ServiceAssetDto>> CreateServiceAsset([FromBody] CreateServiceAssetDto createDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(createDto.ServiceName))
                return BadRequest(new { message = "Service name is required" });
            if (string.IsNullOrWhiteSpace(createDto.Description))
                return BadRequest(new { message = "Description is required" });
            if (createDto.ServiceTypeId <= 0)
                return BadRequest(new { message = "Service type is required" });
            if (createDto.ContractTypeId <= 0)
                return BadRequest(new { message = "Contract type is required" });
            if (string.IsNullOrWhiteSpace(createDto.Vendor))
                return BadRequest(new { message = "Vendor is required" });
            if (string.IsNullOrWhiteSpace(createDto.RenewalCycle))
                return BadRequest(new { message = "Renewal cycle is required" });

            // Validation
            if (createDto.EndDate <= createDto.StartDate)
            {
                return BadRequest(new { message = "End Date must be greater than Start Date" });
            }

            if (createDto.RenewalReminderDays < 0)
            {
                return BadRequest(new { message = "Renewal reminder days cannot be negative" });
            }

            // Generate AssetId automatically
            var assetId = await _assetIdGeneratorService.GenerateServiceAssetIdAsync();

            var asset = new ServiceAsset
            {
                AssetId = assetId,
                ServiceName = createDto.ServiceName,
                Description = createDto.Description,
                ServiceTypeId = createDto.ServiceTypeId,
                ContractTypeId = createDto.ContractTypeId,
                Vendor = createDto.Vendor,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                RenewalCycle = createDto.RenewalCycle,
                RenewalReminderDays = createDto.RenewalReminderDays,
                Status = createDto.Status,
                ContractValue = createDto.ContractValue,
                ContractNumber = createDto.ContractNumber,
                Remarks = createDto.Remarks,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId() ?? 1
            };

            _context.ServiceAssets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service asset {AssetId} created by user {UserId}", asset.AssetId, GetCurrentUserId());

            return CreatedAtAction(nameof(GetServiceAsset), new { id = asset.Id }, new ServiceAssetDto
            {
                Id = asset.Id,
                AssetId = asset.AssetId,
                ServiceName = asset.ServiceName,
                Description = asset.Description,
                ServiceTypeId = asset.ServiceTypeId,
                ContractTypeId = asset.ContractTypeId,
                Vendor = asset.Vendor,
                StartDate = asset.StartDate,
                EndDate = asset.EndDate,
                RenewalCycle = asset.RenewalCycle,
                RenewalReminderDays = asset.RenewalReminderDays,
                Status = asset.Status,
                ContractValue = asset.ContractValue,
                ContractNumber = asset.ContractNumber,
                Remarks = asset.Remarks,
                CreatedAt = asset.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service asset");
            return StatusCode(500, new { message = "Error creating service asset: " + ex.Message });
        }
    }

    // PUT: api/serviceassets/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServiceAsset(int id, [FromBody] UpdateServiceAssetDto updateDto)
    {
        try
        {
            var asset = await _context.ServiceAssets.FindAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Service asset not found" });
            }

            if (!string.IsNullOrEmpty(updateDto.ServiceName))
                asset.ServiceName = updateDto.ServiceName;
            if (!string.IsNullOrEmpty(updateDto.Description))
                asset.Description = updateDto.Description;
            if (updateDto.ServiceTypeId.HasValue && updateDto.ServiceTypeId > 0)
                asset.ServiceTypeId = updateDto.ServiceTypeId.Value;
            if (updateDto.ContractTypeId.HasValue && updateDto.ContractTypeId > 0)
                asset.ContractTypeId = updateDto.ContractTypeId.Value;
            if (!string.IsNullOrEmpty(updateDto.Vendor))
                asset.Vendor = updateDto.Vendor;
            if (updateDto.StartDate.HasValue)
                asset.StartDate = updateDto.StartDate.Value;
            if (updateDto.EndDate.HasValue)
                asset.EndDate = updateDto.EndDate.Value;
            if (!string.IsNullOrEmpty(updateDto.RenewalCycle))
                asset.RenewalCycle = updateDto.RenewalCycle;
            if (updateDto.RenewalReminderDays.HasValue)
                asset.RenewalReminderDays = updateDto.RenewalReminderDays.Value;
            if (updateDto.LastRenewalDate.HasValue)
                asset.LastRenewalDate = updateDto.LastRenewalDate.Value;
            if (updateDto.NextRenewalDate.HasValue)
                asset.NextRenewalDate = updateDto.NextRenewalDate.Value;
            if (!string.IsNullOrEmpty(updateDto.Status))
                asset.Status = updateDto.Status;
            if (updateDto.ContractValue.HasValue)
                asset.ContractValue = updateDto.ContractValue.Value;
            if (!string.IsNullOrEmpty(updateDto.ContractNumber))
                asset.ContractNumber = updateDto.ContractNumber;
            if (!string.IsNullOrEmpty(updateDto.Remarks))
                asset.Remarks = updateDto.Remarks;

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

    // DELETE: api/serviceassets/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceAsset(int id)
    {
        try
        {
            var asset = await _context.ServiceAssets.FindAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Service asset not found" });
            }

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
