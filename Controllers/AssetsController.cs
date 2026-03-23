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

    private AssetDto MapAssetToDto(Asset a)
    {
        return new AssetDto
        {
            Id = a.Id,
            AssetId = a.AssetId,
            AssetTag = a.AssetTag,
            ProjectId = a.ProjectId,
            ProjectName = a.Project?.Name,
            LocationId = a.LocationId,
            LocationName = a.Location?.Name,
            Region = a.Region,
            State = a.State,
            Site = a.Site,
            PlazaName = a.PlazaName,
            LocationText = a.LocationText,
            Department = a.Department,
            Classification = a.Classification?.Name,
            OSType = a.OperatingSystem?.Name,
            OSVersion = a.OSVersion,
            DBType = a.DatabaseType?.Name,
            DBVersion = a.DBVersion,
            IPAddress = a.IPAddress,
            AssignedUserText = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : a.AssignedUserText,
            UserRole = a.AssignedUser?.Role?.Name,
            ProcuredBy = a.ProcuredBy,
            PatchStatus = a.PatchStatus?.Name,
            USBBlockingStatus = a.USBBlockingStatus?.Name,
            Remarks = a.Remarks,
            UsageCategory = a.UsageCategory.ToString(),
            AssetType = a.AssetType?.TypeName,
            SubType = a.SubType?.SubTypeName,
            Make = a.Make,
            Model = a.Model,
            SerialNumber = a.SerialNumber,
            ProcurementDate = a.ProcurementDate,
            ProcurementCost = a.ProcurementCost,
            Vendor = a.Vendor?.VendorName,
            WarrantyStartDate = a.WarrantyStartDate,
            WarrantyEndDate = a.WarrantyEndDate,
            CommissioningDate = a.CommissioningDate,
            CommissioningDateText = a.CommissioningDateText,
            ExtraFields = string.IsNullOrEmpty(a.ExtraFields) ? null :
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(a.ExtraFields),
            Status = a.AssetStatus?.StatusName,
            Placing = a.Placing?.Name,
            AssignedUserId = a.AssignedUserId,
            AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : null,
            AssignedUserRole = a.AssignedUser?.Role?.Name,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };
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
                .ThenInclude(u => u.Role)
                .Include(a => a.AssetType)
                .Include(a => a.SubType)
                .Include(a => a.Vendor)
                .Include(a => a.AssetStatus)
                .Include(a => a.Classification)
                .Include(a => a.OperatingSystem)
                .Include(a => a.DatabaseType)
                .Include(a => a.PatchStatus)
                .Include(a => a.USBBlockingStatus)
                .Include(a => a.Placing)
                .OrderBy(a => a.AssetId)
                .ToListAsync();

            var dtos = assets.Select(MapAssetToDto).ToList();
            return Ok(dtos);
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

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            IQueryable<Asset> query = _context.Assets
                .Include(a => a.Project)
                .Include(a => a.Location)
                .Include(a => a.AssignedUser)
                .ThenInclude(u => u.Role)
                .Include(a => a.AssetType)
                .Include(a => a.SubType)
                .Include(a => a.Vendor)
                .Include(a => a.AssetStatus)
                .Include(a => a.Classification)
                .Include(a => a.OperatingSystem)
                .Include(a => a.DatabaseType)
                .Include(a => a.PatchStatus)
                .Include(a => a.USBBlockingStatus)
                .Include(a => a.Placing);

            // SuperAdmins see all assets
            if (user.RoleId != 1 && user.ProjectId.HasValue)
            {
                query = query.Where(a => a.ProjectId == user.ProjectId.Value);
            }

            var assets = await query.OrderBy(a => a.AssetId).ToListAsync();
            var dtos = assets.Select(MapAssetToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user assets");
            return StatusCode(500, new { message = "Error retrieving user assets" });
        }
    }

    // GET: api/assets/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssetDto>> GetAsset(int id)
    {
        try
        {
            var asset = await _context.Assets
                .Include(a => a.Project)
                .Include(a => a.Location)
                .Include(a => a.AssignedUser)
                .ThenInclude(u => u.Role)
                .Include(a => a.AssetType)
                .Include(a => a.SubType)
                .Include(a => a.Vendor)
                .Include(a => a.AssetStatus)
                .Include(a => a.Classification)
                .Include(a => a.OperatingSystem)
                .Include(a => a.DatabaseType)
                .Include(a => a.PatchStatus)
                .Include(a => a.USBBlockingStatus)
                .Include(a => a.Placing)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null)
            {
                return NotFound(new { message = "Asset not found" });
            }

            return Ok(MapAssetToDto(asset));
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
            // Lookup FK values using correct property names
            var assetType = await _context.AssetTypes.FirstOrDefaultAsync(x => x.TypeName == createDto.AssetType);
            var subType = string.IsNullOrEmpty(createDto.SubType) ? null : 
                await _context.AssetSubTypes.FirstOrDefaultAsync(x => x.SubTypeName == createDto.SubType);
            var vendor = string.IsNullOrEmpty(createDto.Vendor) ? null :
                await _context.Vendors.FirstOrDefaultAsync(x => x.VendorName == createDto.Vendor);
            var status = await _context.AssetStatuses.FirstOrDefaultAsync(x => x.StatusName == createDto.Status);
            var placing = string.IsNullOrWhiteSpace(createDto.Placing) ? null :
                await _context.AssetPlacings.FirstOrDefaultAsync(x => x.Name == createDto.Placing);

            if (assetType == null)
                return BadRequest(new { message = $"Asset type '{createDto.AssetType}' not found" });
            if (status == null)
                return BadRequest(new { message = $"Status '{createDto.Status}' not found" });

            // Lookup optional FK values
            var classification = string.IsNullOrEmpty(createDto.Classification) ? null :
                await _context.AssetClassifications.FirstOrDefaultAsync(x => x.Name.ToLower() == createDto.Classification.ToLower());
            var osType = string.IsNullOrEmpty(createDto.OsType) ? null :
                await _context.OperatingSystems.FirstOrDefaultAsync(x => x.Name.ToLower() == createDto.OsType.ToLower());
            var usbStatus = string.IsNullOrEmpty(createDto.UsbBlockingStatus) ? null :
                await _context.USBBlockingStatuses.FirstOrDefaultAsync(x => x.Name.ToLower() == createDto.UsbBlockingStatus.ToLower());

            var project = createDto.ProjectId > 0
                ? await _context.Projects.FindAsync(createDto.ProjectId)
                : await _context.Projects.FirstOrDefaultAsync();
            var location = createDto.LocationId > 0
                ? await _context.Locations.FindAsync(createDto.LocationId)
                : await _context.Locations.FirstOrDefaultAsync();

            if (project == null)
                return BadRequest(new { message = "Project not found" });
            if (location == null)
                return BadRequest(new { message = "Location not found" });

            var assetId = await GenerateAssetId();

            var asset = new Asset
            {
                AssetId = assetId,
                AssetTag = string.IsNullOrWhiteSpace(createDto.AssetTag) ? "NA" : createDto.AssetTag,
                ProjectId = project.Id,
                ProjectIdRef = project.ProjectId,
                LocationId = location.Id,
                LocationIdRef = location.LocationId,
                UsageCategory = Enum.Parse<AssetUsageCategory>(createDto.UsageCategory),
                AssetTypeId = assetType.Id,
                AssetSubTypeId = subType?.Id,
                Make = createDto.Make,
                Model = createDto.Model,
                SerialNumber = createDto.SerialNumber,
                ProcurementDate = createDto.ProcurementDate,
                ProcurementCost = createDto.ProcurementCost,
                VendorId = vendor?.Id,
                WarrantyStartDate = createDto.WarrantyStartDate,
                WarrantyEndDate = createDto.WarrantyEndDate,
                CommissioningDate = createDto.CommissioningDate,
                AssetStatusId = status.Id,
                AssetPlacingId = placing?.Id,
                AssetClassificationId = classification?.Id,
                OperatingSystemId = osType?.Id,
                USBBlockingStatusId = usbStatus?.Id,
                Region = createDto.Region,
                OSVersion = createDto.OsVersion,
                ProcuredBy = createDto.ProcuredBy,
                Remarks = createDto.Remarks,
                AssignedUserId = createDto.AssignedUserId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId() ?? 1
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Asset {AssetId} created by user {UserId}", asset.AssetId, GetCurrentUserId());

            // Reload with navigation properties
            await _context.Entry(asset).ReloadAsync();
            await _context.Entry(asset).Reference(a => a.Project).LoadAsync();
            await _context.Entry(asset).Reference(a => a.Location).LoadAsync();
            await _context.Entry(asset).Reference(a => a.AssetType).LoadAsync();
            await _context.Entry(asset).Reference(a => a.SubType).LoadAsync();
            await _context.Entry(asset).Reference(a => a.Vendor).LoadAsync();
            await _context.Entry(asset).Reference(a => a.AssetStatus).LoadAsync();
            await _context.Entry(asset).Reference(a => a.Placing).LoadAsync();

            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, MapAssetToDto(asset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset");
            return StatusCode(500, new { message = "Error creating asset" });
        }
    }

    // PUT: api/assets/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsset(int id, [FromBody] UpdateAssetDto updateDto)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Asset not found" });
            }

            if (!string.IsNullOrEmpty(updateDto.AssetTag))
                asset.AssetTag = updateDto.AssetTag;
            if (updateDto.LocationId.HasValue)
                asset.LocationId = updateDto.LocationId.Value;
            if (!string.IsNullOrEmpty(updateDto.AssetType))
            {
                var assetType = await _context.AssetTypes.FirstOrDefaultAsync(x => x.TypeName == updateDto.AssetType);
                if (assetType != null)
                    asset.AssetTypeId = assetType.Id;
            }
            if (!string.IsNullOrEmpty(updateDto.SubType))
            {
                var subType = await _context.AssetSubTypes.FirstOrDefaultAsync(x => x.SubTypeName == updateDto.SubType);
                if (subType != null)
                    asset.AssetSubTypeId = subType.Id;
            }
            if (!string.IsNullOrEmpty(updateDto.Make))
                asset.Make = updateDto.Make;
            if (!string.IsNullOrEmpty(updateDto.Model))
                asset.Model = updateDto.Model;
            if (!string.IsNullOrEmpty(updateDto.SerialNumber))
                asset.SerialNumber = updateDto.SerialNumber;
            if (updateDto.ProcurementDate.HasValue)
                asset.ProcurementDate = updateDto.ProcurementDate;
            if (updateDto.ProcurementCost.HasValue)
                asset.ProcurementCost = updateDto.ProcurementCost;
            if (!string.IsNullOrEmpty(updateDto.Vendor))
            {
                var vendor = await _context.Vendors.FirstOrDefaultAsync(x => x.VendorName == updateDto.Vendor);
                if (vendor != null)
                    asset.VendorId = vendor.Id;
            }
            if (updateDto.WarrantyStartDate.HasValue)
                asset.WarrantyStartDate = updateDto.WarrantyStartDate;
            if (updateDto.WarrantyEndDate.HasValue)
                asset.WarrantyEndDate = updateDto.WarrantyEndDate;
            if (updateDto.CommissioningDate.HasValue)
                asset.CommissioningDate = updateDto.CommissioningDate;
            if (!string.IsNullOrEmpty(updateDto.Status))
            {
                var status = await _context.AssetStatuses.FirstOrDefaultAsync(x => x.StatusName == updateDto.Status);
                if (status != null)
                    asset.AssetStatusId = status.Id;
            }
            if (!string.IsNullOrEmpty(updateDto.Placing))
            {
                var placing = await _context.AssetPlacings.FirstOrDefaultAsync(x => x.Name == updateDto.Placing);
                if (placing != null)
                    asset.AssetPlacingId = placing.Id;
            }
            // Track assignment/location changes in history
            bool userChanged = updateDto.AssignedUserId.HasValue && updateDto.AssignedUserId != asset.AssignedUserId;
            bool locationChanged = updateDto.LocationId.HasValue && updateDto.LocationId != asset.LocationId;

            if (userChanged || locationChanged)
            {
                var userId = GetCurrentUserId() ?? 1;
                var currentUser = await _context.Users.FindAsync(userId);
                var prevLocation = locationChanged ? await _context.Locations.FindAsync(asset.LocationId) : null;
                var newLocation = locationChanged && updateDto.LocationId.HasValue ? await _context.Locations.FindAsync(updateDto.LocationId.Value) : null;
                var prevUser = userChanged ? await _context.Users.FindAsync(asset.AssignedUserId) : null;
                var newUser = userChanged && updateDto.AssignedUserId.HasValue ? await _context.Users.FindAsync(updateDto.AssignedUserId.Value) : null;

                _context.AssetAssignmentHistories.Add(new Domain.Entities.AssetAssignmentHistory
                {
                    AssetId = asset.Id,
                    PreviousUserId = asset.AssignedUserId,
                    PreviousUserName = prevUser != null ? $"{prevUser.FirstName} {prevUser.LastName}" : null,
                    NewUserId = userChanged ? updateDto.AssignedUserId : asset.AssignedUserId,
                    NewUserName = newUser != null ? $"{newUser.FirstName} {newUser.LastName}" : null,
                    PreviousLocationId = locationChanged ? asset.LocationId : null,
                    PreviousLocationName = prevLocation?.Name,
                    NewLocationId = locationChanged ? updateDto.LocationId : null,
                    NewLocationName = newLocation?.Name,
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = userId,
                    ChangedByName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "System"
                });
            }

            if (updateDto.AssignedUserId.HasValue)
                asset.AssignedUserId = updateDto.AssignedUserId;

            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = GetCurrentUserId();

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
    [HttpDelete("{id:int}")]
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

    // POST: api/assets/bulk-upload
    [HttpPost("bulk-upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> BulkUpload([FromForm] IFormFile file, [FromForm] int projectId, [FromForm] string usageCategory = "ITNonTMS")
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { message = $"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB" });
            }

            var userId = GetCurrentUserId() ?? 1;
            using (var stream = file.OpenReadStream())
            {
                var result = await _bulkUploadService.ProcessAssetExcelAsync(stream, userId, usageCategory);
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk upload");
            return StatusCode(500, new { message = "Error processing bulk upload", error = ex.Message });
        }
    }

    // GET: api/assets/download-template
    [HttpGet("download-template")]
    public IActionResult DownloadTemplate()
    {
        var bytes = _bulkUploadService.GenerateSampleTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "hardware-assets-template.xlsx");
    }

    private async Task<string> GenerateAssetId()
    {
        var lastAsset = await _context.Assets
            .Where(a => a.AssetId.StartsWith("ASTH"))
            .OrderByDescending(a => a.AssetId)
            .FirstOrDefaultAsync();

        if (lastAsset == null)
            return "ASTH00001";

        if (int.TryParse(lastAsset.AssetId.Substring(4), out int lastNumber))
        {
            return $"ASTH{(lastNumber + 1):D5}";
        }

        return "ASTH00001";
    }
}
