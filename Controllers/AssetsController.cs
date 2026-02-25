using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using ITAMS.Models;
using ITAMS.Services;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : BaseController
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<AssetsController> _logger;
    private readonly IBulkUploadService _bulkUploadService;
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

    public AssetsController(
        ITAMSDbContext context, 
        ILogger<AssetsController> logger,
        IBulkUploadService bulkUploadService)
    {
        _context = context;
        _logger = logger;
        _bulkUploadService = bulkUploadService;
    }

    // GET: api/assets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssets()
    {
        try
        {
            var assets = await _context.Assets
                .Include(a => a.Project)
                .Include(a => a.Location)
                .Include(a => a.AssignedUser)
                .Select(a => new AssetDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    AssetTag = a.AssetTag,
                    ProjectId = a.ProjectId,
                    ProjectName = a.Project.Name,
                    LocationId = a.LocationId,
                    LocationName = a.Location.Name,
                    UsageCategory = a.UsageCategory.ToString(),
                    Criticality = a.Criticality.ToString(),
                    AssetType = a.AssetType,
                    SubType = a.SubType,
                    Make = a.Make,
                    Model = a.Model,
                    SerialNumber = a.SerialNumber,
                    ProcurementDate = a.ProcurementDate,
                    ProcurementCost = a.ProcurementCost,
                    Vendor = a.Vendor,
                    WarrantyStartDate = a.WarrantyStartDate,
                    WarrantyEndDate = a.WarrantyEndDate,
                    CommissioningDate = a.CommissioningDate,
                    Status = a.Status.ToString(),
                    AssignedUserId = a.AssignedUserId,
                    AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null,
                    AssignedUserRole = a.AssignedUserRole,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets");
            return StatusCode(500, new { message = "Error retrieving assets" });
        }
    }

    // GET: api/assets/my-assets
    [HttpGet("my-assets")]
    public async Task<ActionResult<IEnumerable<AssetDto>>> GetMyAssets()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Get user's project
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // SuperAdmins see all assets
            if (user.RoleId == 1) // SuperAdmin role
            {
                var allAssets = await _context.Assets
                    .Include(a => a.Project)
                    .Include(a => a.Location)
                    .Include(a => a.AssignedUser)
                    .Select(a => new AssetDto
                    {
                        Id = a.Id,
                        AssetId = a.AssetId,
                        AssetTag = a.AssetTag,
                        ProjectId = a.ProjectId,
                        ProjectName = a.Project.Name,
                        LocationId = a.LocationId,
                        LocationName = a.Location.Name,
                        UsageCategory = a.UsageCategory.ToString(),
                        Criticality = a.Criticality.ToString(),
                        AssetType = a.AssetType,
                        SubType = a.SubType,
                        Make = a.Make,
                        Model = a.Model,
                        SerialNumber = a.SerialNumber,
                        ProcurementDate = a.ProcurementDate,
                        ProcurementCost = a.ProcurementCost,
                        Vendor = a.Vendor,
                        WarrantyStartDate = a.WarrantyStartDate,
                        WarrantyEndDate = a.WarrantyEndDate,
                        CommissioningDate = a.CommissioningDate,
                        Status = a.Status.ToString(),
                        AssignedUserId = a.AssignedUserId,
                        AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null,
                        AssignedUserRole = a.AssignedUserRole,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(allAssets);
            }

            // Regular users see assets from their project
            if (!user.ProjectId.HasValue)
            {
                return Ok(new List<AssetDto>()); // No project assigned
            }

            var assets = await _context.Assets
                .Include(a => a.Project)
                .Include(a => a.Location)
                .Include(a => a.AssignedUser)
                .Where(a => a.ProjectId == user.ProjectId.Value)
                .Select(a => new AssetDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    AssetTag = a.AssetTag,
                    ProjectId = a.ProjectId,
                    ProjectName = a.Project.Name,
                    LocationId = a.LocationId,
                    LocationName = a.Location.Name,
                    UsageCategory = a.UsageCategory.ToString(),
                    Criticality = a.Criticality.ToString(),
                    AssetType = a.AssetType,
                    SubType = a.SubType,
                    Make = a.Make,
                    Model = a.Model,
                    SerialNumber = a.SerialNumber,
                    ProcurementDate = a.ProcurementDate,
                    ProcurementCost = a.ProcurementCost,
                    Vendor = a.Vendor,
                    WarrantyStartDate = a.WarrantyStartDate,
                    WarrantyEndDate = a.WarrantyEndDate,
                    CommissioningDate = a.CommissioningDate,
                    Status = a.Status.ToString(),
                    AssignedUserId = a.AssignedUserId,
                    AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null,
                    AssignedUserRole = a.AssignedUserRole,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user assets");
            return StatusCode(500, new { message = "Error retrieving user assets" });
        }
    }

    // GET: api/assets/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AssetDto>> GetAsset(int id)
    {
        try
        {
            var asset = await _context.Assets
                .Include(a => a.Project)
                .Include(a => a.Location)
                .Include(a => a.AssignedUser)
                .Where(a => a.Id == id)
                .Select(a => new AssetDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    AssetTag = a.AssetTag,
                    ProjectId = a.ProjectId,
                    ProjectName = a.Project.Name,
                    LocationId = a.LocationId,
                    LocationName = a.Location.Name,
                    UsageCategory = a.UsageCategory.ToString(),
                    Criticality = a.Criticality.ToString(),
                    AssetType = a.AssetType,
                    SubType = a.SubType,
                    Make = a.Make,
                    Model = a.Model,
                    SerialNumber = a.SerialNumber,
                    ProcurementDate = a.ProcurementDate,
                    ProcurementCost = a.ProcurementCost,
                    Vendor = a.Vendor,
                    WarrantyStartDate = a.WarrantyStartDate,
                    WarrantyEndDate = a.WarrantyEndDate,
                    CommissioningDate = a.CommissioningDate,
                    Status = a.Status.ToString(),
                    AssignedUserId = a.AssignedUserId,
                    AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null,
                    AssignedUserRole = a.AssignedUserRole,
                    CreatedAt = a.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return NotFound(new { message = "Asset not found" });
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset {AssetId}", id);
            return StatusCode(500, new { message = "Error retrieving asset" });
        }
    }

    // POST: api/assets
    [HttpPost]
    public async Task<ActionResult<AssetDto>> CreateAsset([FromBody] CreateAssetDto createDto)
    {
        try
        {
            // Generate Asset ID
            var assetId = await GenerateAssetId();

            // Get Project and Location references
            var project = await _context.Projects.FindAsync(createDto.ProjectId);
            var location = await _context.Locations.FindAsync(createDto.LocationId);

            if (project == null)
            {
                return BadRequest(new { message = "Project not found" });
            }

            if (location == null)
            {
                return BadRequest(new { message = "Location not found" });
            }

            var asset = new Asset
            {
                AssetId = assetId,
                AssetTag = createDto.AssetTag,
                ProjectId = createDto.ProjectId,
                ProjectIdRef = project.ProjectId, // Use project ProjectId (alternate key)
                LocationId = createDto.LocationId,
                LocationIdRef = location.LocationId, // Use location alternate key (LocationId is required string)
                UsageCategory = Enum.Parse<AssetUsageCategory>(createDto.UsageCategory),
                Criticality = Enum.Parse<AssetCriticality>(createDto.Criticality),
                AssetType = createDto.AssetType,
                SubType = createDto.SubType,
                Make = createDto.Make,
                Model = createDto.Model,
                SerialNumber = createDto.SerialNumber,
                ProcurementDate = createDto.ProcurementDate,
                ProcurementCost = createDto.ProcurementCost,
                Vendor = createDto.Vendor,
                WarrantyStartDate = createDto.WarrantyStartDate,
                WarrantyEndDate = createDto.WarrantyEndDate,
                CommissioningDate = createDto.CommissioningDate,
                Status = Enum.Parse<AssetStatus>(createDto.Status),
                AssignedUserId = createDto.AssignedUserId,
                AssignedUserRole = createDto.AssignedUserRole,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId() ?? 1 // Default to 1 if no user context
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            // Log asset creation
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Asset {AssetId} created by user {UserId}", asset.AssetId, currentUserId);

            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, new AssetDto
            {
                Id = asset.Id,
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                ProjectId = asset.ProjectId,
                LocationId = asset.LocationId,
                UsageCategory = asset.UsageCategory.ToString(),
                Criticality = asset.Criticality.ToString(),
                AssetType = asset.AssetType,
                SubType = asset.SubType,
                Make = asset.Make,
                Model = asset.Model,
                SerialNumber = asset.SerialNumber,
                ProcurementDate = asset.ProcurementDate,
                ProcurementCost = asset.ProcurementCost,
                Vendor = asset.Vendor,
                WarrantyStartDate = asset.WarrantyStartDate,
                WarrantyEndDate = asset.WarrantyEndDate,
                CommissioningDate = asset.CommissioningDate,
                Status = asset.Status.ToString(),
                AssignedUserId = asset.AssignedUserId,
                AssignedUserRole = asset.AssignedUserRole,
                CreatedAt = asset.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset");
            return StatusCode(500, new { message = "Error creating asset" });
        }
    }

    // PUT: api/assets/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, [FromBody] UpdateAssetDto updateDto)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Asset not found" });
            }

            // Update fields
            asset.AssetTag = updateDto.AssetTag ?? asset.AssetTag;
            asset.LocationId = updateDto.LocationId ?? asset.LocationId;
            asset.AssetType = updateDto.AssetType ?? asset.AssetType;
            asset.SubType = updateDto.SubType ?? asset.SubType;
            asset.Make = updateDto.Make ?? asset.Make;
            asset.Model = updateDto.Model ?? asset.Model;
            asset.SerialNumber = updateDto.SerialNumber ?? asset.SerialNumber;
            asset.ProcurementDate = updateDto.ProcurementDate ?? asset.ProcurementDate;
            asset.ProcurementCost = updateDto.ProcurementCost ?? asset.ProcurementCost;
            asset.Vendor = updateDto.Vendor ?? asset.Vendor;
            asset.WarrantyStartDate = updateDto.WarrantyStartDate ?? asset.WarrantyStartDate;
            asset.WarrantyEndDate = updateDto.WarrantyEndDate ?? asset.WarrantyEndDate;
            asset.CommissioningDate = updateDto.CommissioningDate ?? asset.CommissioningDate;
            
            if (updateDto.Status != null)
            {
                asset.Status = Enum.Parse<AssetStatus>(updateDto.Status);
            }
            
            asset.AssignedUserId = updateDto.AssignedUserId ?? asset.AssignedUserId;
            asset.AssignedUserRole = updateDto.AssignedUserRole ?? asset.AssignedUserRole;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asset {AssetId} updated by user {UserId}", asset.AssetId, GetCurrentUserId());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset {AssetId}", id);
            return StatusCode(500, new { message = "Error updating asset" });
        }
    }

    // DELETE: api/assets/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Asset not found" });
            }

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Asset {AssetId} deleted by user {UserId}", asset.AssetId, GetCurrentUserId());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset {AssetId}", id);
            return StatusCode(500, new { message = "Error deleting asset" });
        }
    }

    // Helper method to generate Asset ID
    private async Task<string> GenerateAssetId()
    {
        var lastAsset = await _context.Assets
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();

        if (lastAsset == null)
        {
            return "AST00001";
        }

        // Extract number from last asset ID (AST00001 -> 1)
        var lastNumber = int.Parse(lastAsset.AssetId.Substring(3));
        var newNumber = lastNumber + 1;

        return $"AST{newNumber:D5}";
    }

    // POST: api/assets/bulk-upload
    [HttpPost("bulk-upload")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)] // 50MB
    public async Task<IActionResult> BulkUpload([FromForm] IFormFile file)
    {
        try
        {
            _logger.LogWarning("=== BULK UPLOAD ENDPOINT HIT ===");
            _logger.LogInformation("Bulk upload request received. File: {FileName}, Size: {Size}", 
                file?.FileName ?? "null", file?.Length ?? 0);
            
            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
            {
                return BadRequest(new { message = "Only .xlsx files are allowed" });
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { message = $"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)}MB" });
            }

            // Get user ID
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            // Process file
            using var stream = file.OpenReadStream();
            var result = await _bulkUploadService.ProcessAssetExcelAsync(stream, userId.Value);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk upload");
            return StatusCode(500, new { message = "An error occurred during upload", error = ex.Message });
        }
    }

    // GET: api/assets/download-template
    [HttpGet("download-template")]
    public IActionResult DownloadTemplate()
    {
        try
        {
            _logger.LogInformation("Template download requested");
            var fileBytes = _bulkUploadService.GenerateSampleTemplate();
            var fileName = $"Asset_Upload_Template_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating template");
            return StatusCode(500, new { message = "Error generating template" });
        }
    }
}
