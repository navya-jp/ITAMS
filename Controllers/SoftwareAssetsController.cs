using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using ITAMS.Models;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SoftwareAssetsController : BaseController
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<SoftwareAssetsController> _logger;

    public SoftwareAssetsController(
        ITAMSDbContext context,
        ILogger<SoftwareAssetsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/softwareassets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SoftwareAssetDto>>> GetSoftwareAssets()
    {
        try
        {
            var assets = await _context.SoftwareAssets
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new SoftwareAssetDto
                {
                    Id = a.Id,
                    SoftwareName = a.SoftwareName,
                    Version = a.Version,
                    LicenseKey = a.LicenseKey,
                    LicenseType = a.LicenseType,
                    NumberOfLicenses = a.NumberOfLicenses,
                    PurchaseDate = a.PurchaseDate,
                    ValidityStartDate = a.ValidityStartDate,
                    ValidityEndDate = a.ValidityEndDate,
                    ValidityType = a.ValidityType,
                    Vendor = a.Vendor,
                    Publisher = a.Publisher,
                    AssetTag = a.AssetTag,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving software assets");
            return StatusCode(500, new { message = "Error retrieving software assets" });
        }
    }

    // GET: api/softwareassets/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<SoftwareAssetDto>> GetSoftwareAsset(int id)
    {
        try
        {
            var asset = await _context.SoftwareAssets
                .Where(a => a.Id == id)
                .Select(a => new SoftwareAssetDto
                {
                    Id = a.Id,
                    SoftwareName = a.SoftwareName,
                    Version = a.Version,
                    LicenseKey = a.LicenseKey,
                    LicenseType = a.LicenseType,
                    NumberOfLicenses = a.NumberOfLicenses,
                    PurchaseDate = a.PurchaseDate,
                    ValidityStartDate = a.ValidityStartDate,
                    ValidityEndDate = a.ValidityEndDate,
                    ValidityType = a.ValidityType,
                    Vendor = a.Vendor,
                    Publisher = a.Publisher,
                    AssetTag = a.AssetTag,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return NotFound(new { message = "Software asset not found" });
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving software asset {Id}", id);
            return StatusCode(500, new { message = "Error retrieving software asset" });
        }
    }

    // POST: api/softwareassets
    [HttpPost]
    public async Task<ActionResult<SoftwareAssetDto>> CreateSoftwareAsset([FromBody] CreateSoftwareAssetDto createDto)
    {
        try
        {
            // Validation
            if (createDto.ValidityEndDate <= createDto.ValidityStartDate)
            {
                return BadRequest(new { message = "Validity End Date must be greater than Validity Start Date" });
            }

            if (createDto.NumberOfLicenses <= 0)
            {
                return BadRequest(new { message = "Number of Licenses must be greater than 0" });
            }

            // Check for duplicate AssetTag
            var existingAsset = await _context.SoftwareAssets
                .FirstOrDefaultAsync(a => a.AssetTag == createDto.AssetTag);

            if (existingAsset != null)
            {
                return BadRequest(new { message = "Asset Tag already exists" });
            }

            var asset = new SoftwareAsset
            {
                SoftwareName = createDto.SoftwareName,
                Version = createDto.Version,
                LicenseKey = createDto.LicenseKey,
                LicenseType = createDto.LicenseType,
                NumberOfLicenses = createDto.NumberOfLicenses,
                PurchaseDate = createDto.PurchaseDate,
                ValidityStartDate = createDto.ValidityStartDate,
                ValidityEndDate = createDto.ValidityEndDate,
                ValidityType = createDto.ValidityType,
                Vendor = createDto.Vendor,
                Publisher = createDto.Publisher,
                AssetTag = createDto.AssetTag,
                Status = createDto.Status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId() ?? 1
            };

            _context.SoftwareAssets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Software asset {AssetTag} created by user {UserId}", asset.AssetTag, GetCurrentUserId());

            return CreatedAtAction(nameof(GetSoftwareAsset), new { id = asset.Id }, new SoftwareAssetDto
            {
                Id = asset.Id,
                SoftwareName = asset.SoftwareName,
                Version = asset.Version,
                LicenseKey = asset.LicenseKey,
                LicenseType = asset.LicenseType,
                NumberOfLicenses = asset.NumberOfLicenses,
                PurchaseDate = asset.PurchaseDate,
                ValidityStartDate = asset.ValidityStartDate,
                ValidityEndDate = asset.ValidityEndDate,
                ValidityType = asset.ValidityType,
                Vendor = asset.Vendor,
                Publisher = asset.Publisher,
                AssetTag = asset.AssetTag,
                Status = asset.Status,
                CreatedAt = asset.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating software asset");
            return StatusCode(500, new { message = "Error creating software asset" });
        }
    }

    // PUT: api/softwareassets/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSoftwareAsset(int id, [FromBody] UpdateSoftwareAssetDto updateDto)
    {
        try
        {
            var asset = await _context.SoftwareAssets.FindAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Software asset not found" });
            }

            // Validation
            if (updateDto.ValidityEndDate.HasValue && updateDto.ValidityStartDate.HasValue)
            {
                if (updateDto.ValidityEndDate <= updateDto.ValidityStartDate)
                {
                    return BadRequest(new { message = "Validity End Date must be greater than Validity Start Date" });
                }
            }

            if (updateDto.NumberOfLicenses.HasValue && updateDto.NumberOfLicenses <= 0)
            {
                return BadRequest(new { message = "Number of Licenses must be greater than 0" });
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateDto.SoftwareName))
                asset.SoftwareName = updateDto.SoftwareName;
            if (!string.IsNullOrEmpty(updateDto.Version))
                asset.Version = updateDto.Version;
            if (!string.IsNullOrEmpty(updateDto.LicenseKey))
                asset.LicenseKey = updateDto.LicenseKey;
            if (!string.IsNullOrEmpty(updateDto.LicenseType))
                asset.LicenseType = updateDto.LicenseType;
            if (updateDto.NumberOfLicenses.HasValue)
                asset.NumberOfLicenses = updateDto.NumberOfLicenses.Value;
            if (updateDto.PurchaseDate.HasValue)
                asset.PurchaseDate = updateDto.PurchaseDate.Value;
            if (updateDto.ValidityStartDate.HasValue)
                asset.ValidityStartDate = updateDto.ValidityStartDate.Value;
            if (updateDto.ValidityEndDate.HasValue)
                asset.ValidityEndDate = updateDto.ValidityEndDate.Value;
            if (!string.IsNullOrEmpty(updateDto.ValidityType))
                asset.ValidityType = updateDto.ValidityType;
            if (!string.IsNullOrEmpty(updateDto.Vendor))
                asset.Vendor = updateDto.Vendor;
            if (!string.IsNullOrEmpty(updateDto.Publisher))
                asset.Publisher = updateDto.Publisher;
            if (!string.IsNullOrEmpty(updateDto.AssetTag))
                asset.AssetTag = updateDto.AssetTag;
            if (!string.IsNullOrEmpty(updateDto.Status))
                asset.Status = updateDto.Status;

            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = GetCurrentUserId();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Software asset {AssetTag} updated by user {UserId}", asset.AssetTag, GetCurrentUserId());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating software asset {Id}", id);
            return StatusCode(500, new { message = "Error updating software asset" });
        }
    }

    // DELETE: api/softwareassets/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSoftwareAsset(int id)
    {
        try
        {
            var asset = await _context.SoftwareAssets.FindAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Software asset not found" });
            }

            _context.SoftwareAssets.Remove(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Software asset {AssetTag} deleted by user {UserId}", asset.AssetTag, GetCurrentUserId());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting software asset {Id}", id);
            return StatusCode(500, new { message = "Error deleting software asset" });
        }
    }
}
