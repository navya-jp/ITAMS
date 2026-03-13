using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using ITAMS.Models;
using ITAMS.Services;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicensingAssetsController : BaseController
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<LicensingAssetsController> _logger;
    private readonly IAssetIdGeneratorService _assetIdGeneratorService;

    public LicensingAssetsController(
        ITAMSDbContext context,
        ILogger<LicensingAssetsController> logger,
        IAssetIdGeneratorService assetIdGeneratorService)
    {
        _context = context;
        _logger = logger;
        _assetIdGeneratorService = assetIdGeneratorService;
    }

    // GET: api/licensingassets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LicensingAssetDto>>> GetLicensingAssets()
    {
        try
        {
            var assets = await _context.LicensingAssets
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new LicensingAssetDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    LicenseName = a.LicenseName,
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

    // GET: api/LicensingAssets/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<LicensingAssetDto>> GetLicensingAsset(int id)
    {
        try
        {
            var asset = await _context.LicensingAssets
                .Where(a => a.Id == id)
                .Select(a => new LicensingAssetDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    LicenseName = a.LicenseName,
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

    // POST: api/LicensingAssets
    [HttpPost]
    public async Task<ActionResult<LicensingAssetDto>> CreateLicensingAsset([FromBody] CreateLicensingAssetDto createDto)
    {
        try
        {
            _logger.LogInformation("Received software asset creation request: {@CreateDto}", createDto);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(createDto.LicenseName))
                return BadRequest(new { message = "Software name is required" });
            if (string.IsNullOrWhiteSpace(createDto.Version))
                return BadRequest(new { message = "Version is required" });
            if (string.IsNullOrWhiteSpace(createDto.LicenseKey))
                return BadRequest(new { message = "License key is required" });
            if (string.IsNullOrWhiteSpace(createDto.LicenseType))
                return BadRequest(new { message = "License type is required" });
            if (string.IsNullOrWhiteSpace(createDto.Status))
                return BadRequest(new { message = "Status is required" });
            if (string.IsNullOrWhiteSpace(createDto.Vendor))
                return BadRequest(new { message = "Vendor is required" });
            if (string.IsNullOrWhiteSpace(createDto.Publisher))
                return BadRequest(new { message = "Publisher is required" });
            if (string.IsNullOrWhiteSpace(createDto.ValidityType))
                return BadRequest(new { message = "Validity type is required" });

            // Validation
            if (createDto.ValidityEndDate <= createDto.ValidityStartDate)
            {
                return BadRequest(new { message = "Validity End Date must be greater than Validity Start Date" });
            }

            if (createDto.NumberOfLicenses <= 0)
            {
                return BadRequest(new { message = "Number of Licenses must be greater than 0" });
            }

            // Generate AssetId automatically
            var assetId = await _assetIdGeneratorService.GenerateLicensingAssetIdAsync();

            var asset = new LicensingAsset
            {
                AssetId = assetId,
                LicenseName = createDto.LicenseName,
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
                AssetTag = createDto.AssetTag ?? assetId, // Use generated AssetId if not provided
                Status = createDto.Status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId() ?? 1
            };

            _context.LicensingAssets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Software asset {AssetId} created by user {UserId}", asset.AssetId, GetCurrentUserId());

            return CreatedAtAction(nameof(GetLicensingAsset), new { id = asset.Id }, new LicensingAssetDto
            {
                Id = asset.Id,
                AssetId = asset.AssetId,
                LicenseName = asset.LicenseName,
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
            return StatusCode(500, new { message = "Error creating software asset: " + ex.Message });
        }
    }

    // PUT: api/LicensingAssets/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLicensingAsset(int id, [FromBody] UpdateLicensingAssetDto updateDto)
    {
        try
        {
            var asset = await _context.LicensingAssets.FindAsync(id);
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
            if (!string.IsNullOrEmpty(updateDto.LicenseName))
                asset.LicenseName = updateDto.LicenseName;
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

    // DELETE: api/LicensingAssets/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLicensingAsset(int id)
    {
        try
        {
            var asset = await _context.LicensingAssets.FindAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Software asset not found" });
            }

            _context.LicensingAssets.Remove(asset);
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

