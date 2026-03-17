using ITAMS.Data;
using ITAMS.Domain.Entities;
using ITAMS.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Net;

namespace ITAMS.Services;

public class BulkUploadService : IBulkUploadService
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<BulkUploadService> _logger;

    public BulkUploadService(ITAMSDbContext context, ILogger<BulkUploadService> logger)
    {
        _context = context;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<BulkUploadResult> ProcessAssetExcelAsync(Stream fileStream, int userId, string usageCategory = "ITNonTMS")
    {
        var result = new BulkUploadResult();
        var assetsToInsert = new List<Asset>();
        var errors = new List<BulkUploadError>();

        try
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            
            if (worksheet == null || worksheet.Dimension == null)
            {
                _logger.LogWarning("Excel file is empty or has no worksheet");
                result.Message = "Excel file is empty or invalid";
                return result;
            }

            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;

            _logger.LogInformation("Excel file has {RowCount} rows and {ColCount} columns", rowCount, colCount);

            if (rowCount < 2)
            {
                _logger.LogWarning("Excel file has less than 2 rows (header + data)");
                result.Message = "Excel file must contain at least a header row and one data row";
                return result;
            }

            // Auto-detect header row (first row where we find known column names)
            int headerRow = 1;
            for (int r = 1; r <= Math.Min(5, rowCount); r++)
            {
                var testMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int c = 1; c <= colCount; c++)
                {
                    var h = GetCellValue(worksheet, r, c).Trim();
                    if (!string.IsNullOrEmpty(h))
                        testMap[h] = c;
                }
                // If this row contains at least 2 known field names, it's the header
                var knownHeaders = new[] { "Make", "Model", "Status", "Asset type", "Asset Type", "Asset_Type", "Region", "Location", "SubType", "Sub Type" };
                if (knownHeaders.Count(k => testMap.ContainsKey(k)) >= 2)
                {
                    headerRow = r;
                    break;
                }
            }
            _logger.LogInformation("Detected header row at row {HeaderRow}", headerRow);

            // Build column mapping from detected header row
            var columnMapping = BuildColumnMapping(worksheet, colCount, headerRow);
            
            _logger.LogInformation("Column mapping built. Found {Count} columns", columnMapping.Count);
            foreach (var col in columnMapping)
            {
                _logger.LogInformation("  Column: {Name} at position {Position}", col.Key, col.Value);
            }
            
            // Validate required columns exist
            var missingColumns = ValidateRequiredColumns(columnMapping);
            if (missingColumns.Any())
            {
                _logger.LogWarning("Missing required columns: {Columns}", string.Join(", ", missingColumns));
                result.Message = $"Missing required columns: {string.Join(", ", missingColumns)}";
                return result;
            }

            result.TotalRows = rowCount - headerRow; // Exclude header row(s)

            // Debug: log what columns were mapped
            _logger.LogInformation("Column mapping result: {Mapping}", 
                string.Join(", ", columnMapping.Select(x => $"{x.Key}=col{x.Value}")));
            
            // Debug: log first data row raw values
            if (rowCount > headerRow)
            {
                var debugVals = string.Join(" | ", Enumerable.Range(1, Math.Min(colCount, 18))
                    .Select(c => $"C{c}={GetCellValue(worksheet, headerRow + 1, c)}"));
                _logger.LogInformation("First data row raw values: {Vals}", debugVals);
            }

            // Get existing asset tags and serial numbers for duplicate check
            var existingAssetTags = await _context.Assets
                .Select(a => a.AssetTag.ToLower())
                .ToListAsync();
            
            var existingSerialNumbers = await _context.Assets
                .Where(a => a.SerialNumber != null)
                .Select(a => a.SerialNumber!.ToLower())
                .ToListAsync();

            // Get the last AssetId to generate new ones (ASTH prefix)
            var lastAsset = await _context.Assets
                .Where(a => a.AssetId.StartsWith("ASTH"))
                .OrderByDescending(a => a.AssetId)
                .FirstOrDefaultAsync();
            var nextAssetIdNumber = 1;
            if (lastAsset != null && !string.IsNullOrEmpty(lastAsset.AssetId) && lastAsset.AssetId.Length > 4)
            {
                // Extract numeric part from AssetId (e.g., "ASTH00365" -> 365)
                var numericPart = lastAsset.AssetId.Substring(4); // Skip "ASTH" prefix
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextAssetIdNumber = lastNumber + 1;
                }
            }

            // Process each row (skip header)
            for (int row = headerRow + 1; row <= rowCount; row++)
            {
                try
                {
                    var excelRow = ReadExcelRow(worksheet, row, columnMapping);
                    
                    _logger.LogInformation("Row {Row}: AssetTag={AssetTag}, Make={Make}, Model={Model}, Type={Type}, Status={Status}", 
                        row, excelRow.Asset_Tag, excelRow.Make, excelRow.Model, excelRow.Asset_Type, excelRow.Status);
                    
                    // Skip completely empty rows (common in Excel files with formatting)
                    if (IsRowEmpty(excelRow))
                    {
                        _logger.LogWarning("Skipping empty row {Row}", row);
                        result.TotalRows--; // Adjust total count
                        continue;
                    }
                    
                    var validationError = await ValidateRow(excelRow, existingAssetTags, existingSerialNumbers);

                    if (!string.IsNullOrEmpty(validationError))
                    {
                        errors.Add(new BulkUploadError
                        {
                            RowNumber = row,
                            AssetTag = excelRow.Asset_Tag,
                            ErrorMessage = validationError
                        });
                        continue;
                    }

                    // Map to Asset entity
                    var (asset, mapError) = await MapToAssetAsync(excelRow, nextAssetIdNumber, userId, usageCategory);
                    
                    if (asset != null)
                    {
                        assetsToInsert.Add(asset);
                        // Only track non-NA tags for duplicate detection
                        if (!asset.AssetTag.Equals("NA", StringComparison.OrdinalIgnoreCase))
                            existingAssetTags.Add(asset.AssetTag.ToLower());
                        if (!string.IsNullOrEmpty(asset.SerialNumber))
                            existingSerialNumbers.Add(asset.SerialNumber.ToLower());
                        nextAssetIdNumber++;
                    }
                    else
                    {
                        errors.Add(new BulkUploadError
                        {
                            RowNumber = row,
                            AssetTag = excelRow.Asset_Tag,
                            ErrorMessage = mapError ?? "Failed to map row to asset"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing row {Row}", row);
                    var assetTagCol = columnMapping.GetValueOrDefault("Asset_Tag", 1);
                    errors.Add(new BulkUploadError
                    {
                        RowNumber = row,
                        AssetTag = GetCellValue(worksheet, row, assetTagCol),
                        ErrorMessage = $"Unexpected error: {ex.Message}"
                    });
                }
            }

            // Bulk insert
            if (assetsToInsert.Any())
            {
                try
                {
                    await _context.Assets.AddRangeAsync(assetsToInsert);
                    await _context.SaveChangesAsync();
                    result.SuccessCount = assetsToInsert.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bulk insert failed");
                    result.Message = $"Database error during insert: {ex.InnerException?.Message ?? ex.Message}";
                    result.FailedCount = assetsToInsert.Count;
                    return result;
                }
            }

            result.FailedCount = errors.Count;
            result.Errors = errors;
            result.Message = $"Processed {result.TotalRows} rows. Success: {result.SuccessCount}, Failed: {result.FailedCount}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Excel file");
            result.Message = $"Error processing file: {ex.Message}";
        }

        return result;
    }

    private Dictionary<string, int> BuildColumnMapping(ExcelWorksheet worksheet, int colCount, int headerRow = 1)
    {
        var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        // Define all possible column name variations
        var columnVariations = new Dictionary<string, string[]>
        {
            { "Asset_Tag", new[] { "Asset_Tag", "AssetTag", "Asset Tag", "Tag", "Asset ID", "Asset No", "Asset No." } },
            { "Serial_Number", new[] { "Serial_Number", "SerialNumber", "Serial Number", "Serial No", "Serial No.", "SN", "Device Serial No.", "Device Serial No", "Device Serial Number" } },
            { "Region", new[] { "Region" } },
            { "Plaza_Name", new[] { "Plaza_Name", "PlazaName", "Plaza Name", "Plaza", "Site Name", "Site" } },
            { "Location", new[] { "Location", "Site Location", "State", "District" } },
            { "Department", new[] { "Department", "Dept" } },
            { "Asset_Type", new[] { "Asset_Type", "AssetType", "Asset Type", "Asset type", "Type" } },
            { "Sub_Type", new[] { "Sub_Type", "SubType", "Sub Type", "Subtype", "Sub type" } },
            { "Make", new[] { "Make", "Manufacturer", "Brand" } },
            { "Model", new[] { "Model", "Model Number" } },
            { "Asset_Classification", new[] { "Asset_Classification", "AssetClassification", "Asset Classification", "Classification", "Criticality", "Asset Classification" } },
            { "OS_Type", new[] { "OS_Type", "OSType", "OS Type", "Operating System", "OS", "OS type", "OS Type" } },
            { "OS_Version", new[] { "OS_Version", "OSVersion", "OS Version", "OS Ver", "OS Ver.", "OS version" } },
            { "DB_Type", new[] { "DB_Type", "DBType", "DB Type", "Database Type", "Database" } },
            { "DB_Version", new[] { "DB_Version", "DBVersion", "DB Version", "Database Version" } },
            { "IP_Address", new[] { "IP_Address", "IPAddress", "IP Address", "IP" } },
            { "Assigned_User_Name", new[] { "Assigned_User_Name", "AssignedUserName", "Assigned User Name", "Assigned User", "User Name", "Username", "Assigned user" } },
            { "User_Role", new[] { "User_Role", "UserRole", "User Role", "Role" } },
            { "Procured_By", new[] { "Procured_By", "ProcuredBy", "Procured By", "Procured by", "Vendor" } },
            { "Commissioning_Date", new[] { "Commissioning_Date", "CommissioningDate", "Commissioning Date", "Commission Date", "Date", "Issue Date", "Issue Date\\ commissioning", "Issue Date/ commissioning", "Issue Date\\commissioning", "Issue Date/commissioning", "Issue Date\\ Commissioning", "Issue Date/ Commissioning" } },
            { "Status", new[] { "Status", "Asset Status" } },
            { "Criticality", new[] { "Criticality", "Critical Level", "Priority" } },
            { "Placing", new[] { "Placing", "Placement", "Area", "Location Area" } },
            { "Patch_Status", new[] { "Patch_Status", "PatchStatus", "Patch Status" } },
            { "USB_Blocking_Status", new[] { "USB_Blocking_Status", "USBBlockingStatus", "USB Blocking Status", "USB Status", "Status of USB Blocking", "Status of USB blocking", "USB Blocking" } },
            { "Remarks", new[] { "Remarks", "Notes", "Comments", "Description" } }
        };

        // Read header row and build mapping
        for (int col = 1; col <= colCount; col++)
        {
            var headerValue = GetCellValue(worksheet, headerRow, col);
            if (string.IsNullOrWhiteSpace(headerValue))
                continue;

            var h = headerValue.Trim();

            // 1. Try exact match first
            bool matched = false;
            foreach (var kvp in columnVariations)
            {
                if (kvp.Value.Any(v => v.Equals(h, StringComparison.OrdinalIgnoreCase)))
                {
                    mapping[kvp.Key] = col;
                    matched = true;
                    break;
                }
            }

            if (matched) continue;

            // 2. Fallback: partial/contains match (e.g. "Asset Type" matches "Asset_Type")
            var hNorm = h.Replace(" ", "").Replace("_", "").Replace("-", "").ToLower();
            foreach (var kvp in columnVariations)
            {
                if (mapping.ContainsKey(kvp.Key)) continue; // already mapped
                var keyNorm = kvp.Key.Replace("_", "").ToLower();
                if (hNorm == keyNorm || kvp.Value.Any(v =>
                    v.Replace(" ", "").Replace("_", "").ToLower() == hNorm))
                {
                    mapping[kvp.Key] = col;
                    break;
                }
            }
        }

        return mapping;
    }

    private List<string> ValidateRequiredColumns(Dictionary<string, int> columnMapping)
    {
        var requiredColumns = new[] { "Asset_Type", "Make", "Model", "Status" };
        var missingColumns = new List<string>();

        foreach (var required in requiredColumns)
        {
            if (!columnMapping.ContainsKey(required))
            {
                missingColumns.Add(required);
            }
        }

        return missingColumns;
    }

    private AssetExcelRow ReadExcelRow(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMapping)
    {
        return new AssetExcelRow
        {
            RowNumber = row,
            Asset_Tag = GetMappedCellValue(worksheet, row, columnMapping, "Asset_Tag"),
            Serial_Number = GetMappedCellValue(worksheet, row, columnMapping, "Serial_Number"),
            Region = GetMappedCellValue(worksheet, row, columnMapping, "Region"),
            Plaza_Name = GetMappedCellValue(worksheet, row, columnMapping, "Plaza_Name"),
            Location = GetMappedCellValue(worksheet, row, columnMapping, "Location"),
            Department = GetMappedCellValue(worksheet, row, columnMapping, "Department"),
            Asset_Type = GetMappedCellValue(worksheet, row, columnMapping, "Asset_Type"),
            Sub_Type = GetMappedCellValue(worksheet, row, columnMapping, "Sub_Type"),
            Make = GetMappedCellValue(worksheet, row, columnMapping, "Make"),
            Model = GetMappedCellValue(worksheet, row, columnMapping, "Model"),
            Asset_Classification = GetMappedCellValue(worksheet, row, columnMapping, "Asset_Classification"),
            OS_Type = GetMappedCellValue(worksheet, row, columnMapping, "OS_Type"),
            OS_Version = GetMappedCellValue(worksheet, row, columnMapping, "OS_Version"),
            DB_Type = GetMappedCellValue(worksheet, row, columnMapping, "DB_Type"),
            DB_Version = GetMappedCellValue(worksheet, row, columnMapping, "DB_Version"),
            IP_Address = GetMappedCellValue(worksheet, row, columnMapping, "IP_Address"),
            Assigned_User_Name = GetMappedCellValue(worksheet, row, columnMapping, "Assigned_User_Name"),
            User_Role = GetMappedCellValue(worksheet, row, columnMapping, "User_Role"),
            Procured_By = GetMappedCellValue(worksheet, row, columnMapping, "Procured_By"),
            Commissioning_Date = GetMappedCellValue(worksheet, row, columnMapping, "Commissioning_Date"),
            Status = GetMappedCellValue(worksheet, row, columnMapping, "Status"),
            Criticality = GetMappedCellValue(worksheet, row, columnMapping, "Criticality"),
            Placing = GetMappedCellValue(worksheet, row, columnMapping, "Placing"),
            Patch_Status = GetMappedCellValue(worksheet, row, columnMapping, "Patch_Status"),
            USB_Blocking_Status = GetMappedCellValue(worksheet, row, columnMapping, "USB_Blocking_Status"),
            Remarks = GetMappedCellValue(worksheet, row, columnMapping, "Remarks")
        };
    }

    private string GetMappedCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMapping, string columnName)
    {
        if (columnMapping.TryGetValue(columnName, out int colIndex))
        {
            return GetCellValue(worksheet, row, colIndex);
        }
        return string.Empty;
    }

    private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        var cell = worksheet.Cells[row, col];
        return cell.Value?.ToString()?.Trim() ?? string.Empty;
    }

    private bool IsRowEmpty(AssetExcelRow row)
    {
        // Only check fields that are actually required — Asset_Tag may never be in the file
        return string.IsNullOrWhiteSpace(row.Make) &&
               string.IsNullOrWhiteSpace(row.Model) &&
               string.IsNullOrWhiteSpace(row.Asset_Type);
    }

    private async Task<string> ValidateRow(AssetExcelRow row, List<string> existingAssetTags, List<string> existingSerialNumbers)
    {
        // Required fields (Asset_Tag is optional — defaults to "NA" if missing)
        if (string.IsNullOrWhiteSpace(row.Asset_Type))
            return "Asset_Type is required";

        if (string.IsNullOrWhiteSpace(row.Make))
            return "Make is required";

        if (string.IsNullOrWhiteSpace(row.Model))
            return "Model is required";

        if (string.IsNullOrWhiteSpace(row.Status))
            return "Status is required";

        // Duplicate check (skip if tag is empty/NA — will be defaulted)
        if (!string.IsNullOrWhiteSpace(row.Asset_Tag) &&
            !row.Asset_Tag.Equals("NA", StringComparison.OrdinalIgnoreCase) &&
            existingAssetTags.Contains(row.Asset_Tag.ToLower()))
            return "Asset_Tag already exists";

        if (!string.IsNullOrWhiteSpace(row.Serial_Number) && 
            existingSerialNumbers.Contains(row.Serial_Number.ToLower()))
            return "Serial_Number already exists";

        // Status validation - case-insensitive
        var statusExists = await _context.AssetStatuses.AnyAsync(s => s.StatusName.ToLower() == row.Status.ToLower());
        if (!statusExists)
        {
            // Try common mappings
            var statusMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "inuse", "In Use" }, { "in-use", "In Use" }, { "active", "In Use" },
                { "spare", "Spare" }, { "repair", "Repair" }, { "decommissioned", "Decommissioned" },
                { "scrap", "Decommissioned" }
            };
            if (statusMap.TryGetValue(row.Status.Replace(" ", ""), out var mapped))
                row.Status = mapped;
            else
            {
                // Last resort: use first available status
                var firstStatus = await _context.AssetStatuses.FirstOrDefaultAsync();
                if (firstStatus != null) row.Status = firstStatus.StatusName;
                else return $"Invalid Status: '{row.Status}'. Valid values: In Use, Spare, Repair, Decommissioned";
            }
        }

        // Placing validation - only if provided
        if (!string.IsNullOrWhiteSpace(row.Placing))
        {
            var placingExists = await _context.AssetPlacings.AnyAsync(p => p.Name == row.Placing);
            if (!placingExists)
                return $"Invalid Placing: '{row.Placing}' not found in database";
        }

        // Date validation
        if (!string.IsNullOrWhiteSpace(row.Commissioning_Date) && !IsValidDate(row.Commissioning_Date))
            return "Commissioning_Date is not a valid date";

        // IP Address validation
        if (!string.IsNullOrWhiteSpace(row.IP_Address) && !IsValidIPv4(row.IP_Address))
            return "IP_Address is not a valid IPv4 address";

        return string.Empty;
    }

    private async Task<(Asset? asset, string? error)> MapToAssetAsync(AssetExcelRow row, int nextAssetIdNumber, int userId, string usageCategory = "ITNonTMS")
    {
        var defaultProject = await _context.Projects.FirstOrDefaultAsync();
        var defaultLocation = await _context.Locations.FirstOrDefaultAsync();

        if (defaultProject == null || defaultLocation == null)
            return (null, "No project or location found in database");

        // Asset type — try exact match first, then partial
        var assetTypeName = row.Asset_Type?.Trim() ?? "";
        var assetType = await _context.AssetTypes.FirstOrDefaultAsync(x => x.TypeName.ToLower() == assetTypeName.ToLower());
        if (assetType == null && !string.IsNullOrEmpty(assetTypeName))
            assetType = await _context.AssetTypes.FirstOrDefaultAsync(x => x.TypeName.ToLower().Contains(assetTypeName.ToLower()) || assetTypeName.ToLower().Contains(x.TypeName.ToLower()));

        var subType = string.IsNullOrEmpty(row.Sub_Type) ? null :
            await _context.AssetSubTypes.FirstOrDefaultAsync(x => x.SubTypeName.ToLower() == row.Sub_Type.ToLower());

        // Status — try exact, then fallback to first
        var statusName = row.Status?.Trim() ?? "";
        var status = await _context.AssetStatuses.FirstOrDefaultAsync(x => x.StatusName.ToLower() == statusName.ToLower());
        if (status == null)
            status = await _context.AssetStatuses.FirstOrDefaultAsync(); // fallback

        var placing = string.IsNullOrWhiteSpace(row.Placing)
            ? await _context.AssetPlacings.FirstOrDefaultAsync()
            : await _context.AssetPlacings.FirstOrDefaultAsync(x => x.Name.ToLower() == row.Placing.ToLower())
              ?? await _context.AssetPlacings.FirstOrDefaultAsync();

        var classification = string.IsNullOrEmpty(row.Asset_Classification) ? null :
            await _context.AssetClassifications.FirstOrDefaultAsync(x => x.Name.ToLower() == row.Asset_Classification.ToLower());
        var osType = string.IsNullOrEmpty(row.OS_Type) ? null :
            await _context.OperatingSystems.FirstOrDefaultAsync(x => x.Name.ToLower() == row.OS_Type.ToLower());
        var dbType = string.IsNullOrEmpty(row.DB_Type) ? null :
            await _context.DatabaseTypes.FirstOrDefaultAsync(x => x.Name.ToLower() == row.DB_Type.ToLower());
        var patchStatus = string.IsNullOrEmpty(row.Patch_Status) ? null :
            await _context.PatchStatuses.FirstOrDefaultAsync(x => x.Name.ToLower() == row.Patch_Status.ToLower());
        var usbStatus = string.IsNullOrEmpty(row.USB_Blocking_Status) ? null :
            await _context.USBBlockingStatuses.FirstOrDefaultAsync(x => x.Name.ToLower() == row.USB_Blocking_Status.ToLower());

        if (assetType == null)
            return (null, $"Asset type '{row.Asset_Type}' not found in database");
        if (status == null)
            return (null, $"No statuses found in database");

        return (new Asset
        {
            AssetId = $"ASTH{nextAssetIdNumber:D5}",
            AssetTag = string.IsNullOrWhiteSpace(row.Asset_Tag) ? "NA" : row.Asset_Tag,
            SerialNumber = row.Serial_Number,
            ProjectId = defaultProject.Id,
            ProjectIdRef = defaultProject.ProjectId,
            LocationId = defaultLocation.Id,
            LocationIdRef = defaultLocation.LocationId,
            Region = string.IsNullOrWhiteSpace(row.Region) ? null : row.Region,
            State = null,
            Site = null,
            PlazaName = string.IsNullOrWhiteSpace(row.Plaza_Name) ? null : row.Plaza_Name,
            LocationText = string.IsNullOrWhiteSpace(row.Location) ? null : row.Location,
            Department = string.IsNullOrWhiteSpace(row.Department) ? null : row.Department,
            OSVersion = string.IsNullOrWhiteSpace(row.OS_Version) ? null : row.OS_Version,
            DBVersion = string.IsNullOrWhiteSpace(row.DB_Version) ? null : row.DB_Version,
            IPAddress = string.IsNullOrWhiteSpace(row.IP_Address) ? null : row.IP_Address,
            ProcuredBy = string.IsNullOrWhiteSpace(row.Procured_By) ? null : row.Procured_By,
            Remarks = string.IsNullOrWhiteSpace(row.Remarks) ? null : row.Remarks,
            AssetTypeId = assetType.Id,
            AssetSubTypeId = subType?.Id,
            AssetStatusId = status.Id,
            AssetPlacingId = placing?.Id,
            AssetClassificationId = classification?.Id,
            OperatingSystemId = osType?.Id,
            DatabaseTypeId = dbType?.Id,
            PatchStatusId = patchStatus?.Id,
            USBBlockingStatusId = usbStatus?.Id,
            Make = row.Make,
            Model = row.Model,
            UsageCategory = Enum.TryParse<AssetUsageCategory>(usageCategory, out var parsedCategory) ? parsedCategory : AssetUsageCategory.ITNonTMS,
            CommissioningDate = ParseDate(row.Commissioning_Date),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        }, null);
    }

    private bool IsValidDate(string dateStr)
    {
        return DateTime.TryParse(dateStr, out _);
    }

    private bool IsValidIPv4(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private DateTime? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return null;

        return DateTime.TryParse(dateStr, out var date) ? date : null;
    }

    // ── Licensing Bulk Upload ─────────────────────────────────────────────────

    public async Task<BulkUploadResult> ProcessLicensingExcelAsync(Stream fileStream, int userId, string usageCategory = "TMS")
    {
        var result = new BulkUploadResult();
        var toInsert = new List<ITAMS.Domain.Entities.LicensingAsset>();
        var errors = new List<BulkUploadError>();

        try
        {
            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets[0];
            if (ws?.Dimension == null) { result.Message = "Excel file is empty or invalid"; return result; }

            int rowCount = ws.Dimension.Rows;
            if (rowCount < 2) { result.Message = "File must have a header row and at least one data row"; return result; }

            result.TotalRows = rowCount - 1;

            // Get last licensing asset id number
            var lastAsset = await _context.LicensingAssets.OrderByDescending(a => a.AssetId).FirstOrDefaultAsync();
            int nextNum = 1;
            if (lastAsset != null && lastAsset.AssetId.Length > 4 && int.TryParse(lastAsset.AssetId.Substring(4), out int n))
                nextNum = n + 1;

            // Build header-based column map (case-insensitive, handles variations)
            var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int totalCols = ws.Dimension.Columns;
            for (int c = 1; c <= totalCols; c++)
            {
                var h = ws.Cells[1, c].Value?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(h)) colMap[h] = c;
            }

            string LicGet(int row, string[] names)
            {
                foreach (var n in names)
                    if (colMap.TryGetValue(n, out int ci))
                        return ws.Cells[row, ci].Value?.ToString()?.Trim() ?? "";
                return "";
            }

            for (int row = 2; row <= rowCount; row++)
            {
                string licenseName   = LicGet(row, new[] { "LicenseName", "License Name", "License_Name", "LicenseName*" });
                string version       = LicGet(row, new[] { "Version" });
                string licenseKey    = LicGet(row, new[] { "LicenseKey", "License Key", "License_Key", "LicenseKey*" });
                string licenseType   = LicGet(row, new[] { "LicenseType", "License Type", "License_Type" });
                string vendor        = LicGet(row, new[] { "Vendor" });
                string publisher     = LicGet(row, new[] { "Publisher" });
                string validityType  = LicGet(row, new[] { "ValidityType", "Validity Type", "Validity_Type" });
                string numStr        = LicGet(row, new[] { "NumberOfLicenses", "Number Of Licenses", "Number_Of_Licenses", "NumberOfLicenses*", "Qty", "Quantity" });
                string purchaseDateStr = LicGet(row, new[] { "PurchaseDate", "Purchase Date", "Purchase_Date" });
                string startDateStr  = LicGet(row, new[] { "ValidityStartDate", "Validity Start Date", "Start Date", "ValidityStartDate*" });
                string endDateStr    = LicGet(row, new[] { "ValidityEndDate", "Validity End Date", "End Date", "ValidityEndDate*" });
                string status        = LicGet(row, new[] { "Status" });

                if (string.IsNullOrWhiteSpace(licenseName) && string.IsNullOrWhiteSpace(licenseKey)) { result.TotalRows--; continue; }

                if (string.IsNullOrWhiteSpace(licenseName)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseKey, ErrorMessage = "LicenseName is required" }); continue; }
                if (string.IsNullOrWhiteSpace(licenseKey)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseName, ErrorMessage = "LicenseKey is required" }); continue; }

                // NumberOfLicenses is optional — default to 1 if missing/invalid
                if (!int.TryParse(numStr, out int numLicenses) || numLicenses <= 0) numLicenses = 1;

                if (!DateTime.TryParse(startDateStr, out var startDate)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseName, ErrorMessage = "ValidityStartDate is invalid or missing" }); continue; }
                if (!DateTime.TryParse(endDateStr, out var endDate)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseName, ErrorMessage = "ValidityEndDate is invalid or missing" }); continue; }
                if (endDate <= startDate) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseName, ErrorMessage = "ValidityEndDate must be after ValidityStartDate" }); continue; }

                DateTime.TryParse(purchaseDateStr, out var purchaseDate);
                var assetId = $"ASTL{nextNum:D5}";

                toInsert.Add(new ITAMS.Domain.Entities.LicensingAsset
                {
                    AssetId = assetId,
                    LicenseName = licenseName,
                    Version = string.IsNullOrWhiteSpace(version) ? "N/A" : version,
                    LicenseKey = licenseKey,
                    LicenseType = string.IsNullOrWhiteSpace(licenseType) ? "Subscription" : licenseType,
                    Vendor = string.IsNullOrWhiteSpace(vendor) ? "N/A" : vendor,
                    Publisher = string.IsNullOrWhiteSpace(publisher) ? "N/A" : publisher,
                    ValidityType = string.IsNullOrWhiteSpace(validityType) ? "Renewable" : validityType,
                    NumberOfLicenses = numLicenses,
                    PurchaseDate = purchaseDate == default ? DateTime.UtcNow : purchaseDate,
                    ValidityStartDate = startDate,
                    ValidityEndDate = endDate,
                    Status = string.IsNullOrWhiteSpace(status) ? "Active" : status,
                    AssetTag = assetId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
                nextNum++;
            }

            if (toInsert.Any())
            {
                await _context.LicensingAssets.AddRangeAsync(toInsert);
                await _context.SaveChangesAsync();
                result.SuccessCount = toInsert.Count;
            }
            result.FailedCount = errors.Count;
            result.Errors = errors;
            result.Message = $"Processed {result.TotalRows} rows. Success: {result.SuccessCount}, Failed: {result.FailedCount}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing licensing Excel");
            result.Message = $"Error processing file: {ex.Message}";
        }
        return result;
    }

    // ── Service Bulk Upload ───────────────────────────────────────────────────

    public async Task<BulkUploadResult> ProcessServiceExcelAsync(Stream fileStream, int userId, string usageCategory = "TMS")
    {
        var result = new BulkUploadResult();
        var toInsert = new List<ITAMS.Domain.Entities.ServiceAsset>();
        var errors = new List<BulkUploadError>();

        try
        {
            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets[0];
            if (ws?.Dimension == null) { result.Message = "Excel file is empty or invalid"; return result; }

            int rowCount = ws.Dimension.Rows;
            if (rowCount < 2) { result.Message = "File must have a header row and at least one data row"; return result; }

            result.TotalRows = rowCount - 1;

            var lastAsset = await _context.ServiceAssets.OrderByDescending(a => a.AssetId).FirstOrDefaultAsync();
            int nextNum = 1;
            if (lastAsset != null && lastAsset.AssetId.Length > 4 && int.TryParse(lastAsset.AssetId.Substring(4), out int n))
                nextNum = n + 1;

            // Pre-load service types for lookup
            var serviceTypes = await _context.ServiceTypes.ToListAsync();

            for (int row = 2; row <= rowCount; row++)
            {
                string Get(int col) => ws.Cells[row, col].Value?.ToString()?.Trim() ?? "";
                string serviceName = Get(1), serviceTypeName = Get(2), vendorName = Get(3),
                       contractNumber = Get(4), startDateStr = Get(5), endDateStr = Get(6),
                       cycleStr = Get(7), reminderStr = Get(8), costStr = Get(9),
                       billingCycle = Get(10), currency = Get(11),
                       contactPerson = Get(12), slaType = Get(13), description = Get(14);

                if (string.IsNullOrWhiteSpace(serviceName) && string.IsNullOrWhiteSpace(vendorName)) { result.TotalRows--; continue; }

                if (string.IsNullOrWhiteSpace(serviceName)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = vendorName, ErrorMessage = "ServiceName is required" }); continue; }
                if (string.IsNullOrWhiteSpace(vendorName)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = serviceName, ErrorMessage = "VendorName is required" }); continue; }
                if (!DateTime.TryParse(startDateStr, out var startDate)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = serviceName, ErrorMessage = "ContractStartDate is invalid" }); continue; }
                if (!DateTime.TryParse(endDateStr, out var endDate)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = serviceName, ErrorMessage = "ContractEndDate is invalid" }); continue; }
                if (endDate <= startDate) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = serviceName, ErrorMessage = "ContractEndDate must be after ContractStartDate" }); continue; }

                var serviceType = serviceTypes.FirstOrDefault(t => t.TypeName.Equals(serviceTypeName, StringComparison.OrdinalIgnoreCase))
                                  ?? serviceTypes.FirstOrDefault();
                if (serviceType == null) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = serviceName, ErrorMessage = "No ServiceTypes found in database" }); continue; }

                int.TryParse(cycleStr, out int cycle); if (cycle <= 0) cycle = 12;
                int.TryParse(reminderStr, out int reminder); if (reminder <= 0) reminder = 30;
                decimal.TryParse(costStr, out decimal cost);

                var assetId = $"ASTV{nextNum:D5}";
                toInsert.Add(new ITAMS.Domain.Entities.ServiceAsset
                {
                    AssetId = assetId,
                    ServiceName = serviceName,
                    ServiceTypeId = serviceType.Id,
                    VendorName = vendorName,
                    ContractNumber = string.IsNullOrWhiteSpace(contractNumber) ? null : contractNumber,
                    ContractStartDate = startDate,
                    ContractEndDate = endDate,
                    NextRenewalDate = endDate,
                    RenewalCycleMonths = cycle,
                    RenewalReminderDays = reminder,
                    ContractCost = cost > 0 ? cost : null,
                    BillingCycle = string.IsNullOrWhiteSpace(billingCycle) ? null : billingCycle,
                    Currency = string.IsNullOrWhiteSpace(currency) ? "INR" : currency,
                    UsageCategory = usageCategory,
                    ContactPerson = string.IsNullOrWhiteSpace(contactPerson) ? null : contactPerson,
                    SLAType = string.IsNullOrWhiteSpace(slaType) ? null : slaType,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
                nextNum++;
            }

            if (toInsert.Any())
            {
                await _context.ServiceAssets.AddRangeAsync(toInsert);
                await _context.SaveChangesAsync();
                result.SuccessCount = toInsert.Count;
            }
            result.FailedCount = errors.Count;
            result.Errors = errors;
            result.Message = $"Processed {result.TotalRows} rows. Success: {result.SuccessCount}, Failed: {result.FailedCount}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing service Excel");
            result.Message = $"Error processing file: {ex.Message}";
        }
        return result;
    }

    // ── Templates ─────────────────────────────────────────────────────────────

    public byte[] GenerateLicensingTemplate()
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Licensing Assets");
        var headers = new[] { "LicenseName*", "Version", "LicenseKey*", "LicenseType", "Vendor", "Publisher", "ValidityType", "NumberOfLicenses*", "PurchaseDate", "ValidityStartDate*", "ValidityEndDate*", "Status" };
        for (int i = 0; i < headers.Length; i++) { ws.Cells[1, i + 1].Value = headers[i]; ws.Cells[1, i + 1].Style.Font.Bold = true; }
        // Sample row
        ws.Cells[2, 1].Value = "Microsoft Office 365";
        ws.Cells[2, 2].Value = "2024";
        ws.Cells[2, 3].Value = "XXXX-XXXX-XXXX-XXXX";
        ws.Cells[2, 4].Value = "Subscription";
        ws.Cells[2, 5].Value = "Microsoft";
        ws.Cells[2, 6].Value = "Microsoft";
        ws.Cells[2, 7].Value = "Renewable";
        ws.Cells[2, 8].Value = 10;
        ws.Cells[2, 9].Value = "2025-01-01";
        ws.Cells[2, 10].Value = "2025-01-01";
        ws.Cells[2, 11].Value = "2026-01-01";
        ws.Cells[2, 12].Value = "Active";
        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] GenerateServiceTemplate()
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Services");
        var headers = new[] { "ServiceName*", "ServiceType", "VendorName*", "ContractNumber", "ContractStartDate*", "ContractEndDate*", "RenewalCycleMonths", "RenewalReminderDays", "ContractCost", "BillingCycle", "Currency", "ContactPerson", "SLAType", "Description" };
        for (int i = 0; i < headers.Length; i++) { ws.Cells[1, i + 1].Value = headers[i]; ws.Cells[1, i + 1].Style.Font.Bold = true; }
        // Sample row
        ws.Cells[2, 1].Value = "Annual Server Maintenance";
        ws.Cells[2, 2].Value = "AMC Comprehensive";
        ws.Cells[2, 3].Value = "TechCorp Pvt Ltd";
        ws.Cells[2, 4].Value = "AMC-2025-001";
        ws.Cells[2, 5].Value = "2025-01-01";
        ws.Cells[2, 6].Value = "2026-01-01";
        ws.Cells[2, 7].Value = 12;
        ws.Cells[2, 8].Value = 30;
        ws.Cells[2, 9].Value = 150000;
        ws.Cells[2, 10].Value = "Annually";
        ws.Cells[2, 11].Value = "INR";
        ws.Cells[2, 12].Value = "John Doe";
        ws.Cells[2, 13].Value = "8x5";
        ws.Cells[2, 14].Value = "Comprehensive AMC for all servers";
        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] GenerateSampleTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Assets");

        var headers = new[]
        {
            "Region", "Location", "Asset_Type*", "Asset_Tag", "Sub_Type",
            "Make*", "Model*", "Serial_Number", "Assigned_User_Name", "Status*",
            "Procured_By", "Commissioning_Date", "OS_Type", "OS_Version",
            "USB_Blocking_Status", "Asset_Classification", "Placing", "Remarks"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Sample data row
        worksheet.Cells[2, 1].Value = "North";
        worksheet.Cells[2, 2].Value = "Head Office";
        worksheet.Cells[2, 3].Value = "Laptop";
        worksheet.Cells[2, 4].Value = ""; // Leave blank if tag not yet assigned
        worksheet.Cells[2, 5].Value = "Business";
        worksheet.Cells[2, 6].Value = "Dell";
        worksheet.Cells[2, 7].Value = "Latitude 5420";
        worksheet.Cells[2, 8].Value = "SN123456";
        worksheet.Cells[2, 9].Value = "John Doe";
        worksheet.Cells[2, 10].Value = "In Use";
        worksheet.Cells[2, 11].Value = "IT Department";
        worksheet.Cells[2, 12].Value = "2024-01-15";
        worksheet.Cells[2, 13].Value = "Windows 11";
        worksheet.Cells[2, 14].Value = "22H2";
        worksheet.Cells[2, 15].Value = "Enabled";
        worksheet.Cells[2, 16].Value = "TMS Critical";
        worksheet.Cells[2, 17].Value = ""; // Leave blank for head office
        worksheet.Cells[2, 18].Value = "";

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }
}
