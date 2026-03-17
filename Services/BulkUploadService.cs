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

            // Log raw header row for debugging
            _logger.LogInformation("=== RAW HEADER ROW ===");
            for (int c = 1; c <= colCount; c++)
            {
                var raw = GetCellValue(worksheet, 1, c);
                _logger.LogInformation("  Col {C}: '{Header}'", c, raw);
            }

            // Build column mapping from header row
            var columnMapping = BuildColumnMapping(worksheet, colCount);
            int headerRowNum = columnMapping.TryGetValue("__headerRow__", out int hrn) ? hrn : 1;
            int dataStartRow = headerRowNum + 1;
            
            _logger.LogInformation("Column mapping built. Found {Count} columns, data starts at row {DataStart}", columnMapping.Count, dataStartRow);
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

            result.TotalRows = 0; // Will be counted during processing (excludes empty rows)

            // Get existing asset tags and serial numbers for duplicate check
            var existingAssetTags = await _context.Assets
                .Select(a => a.AssetTag.ToLower())
                .ToListAsync();
            
            var existingSerialNumbers = await _context.Assets
                .Where(a => a.SerialNumber != null)
                .Select(a => a.SerialNumber!.ToLower())
                .ToListAsync();

            // Get the last AssetId to generate new ones (parse the numeric part from AssetId like "AST00365")
            var lastAsset = await _context.Assets.OrderByDescending(a => a.AssetId).FirstOrDefaultAsync();
            var nextAssetIdNumber = 1;
            if (lastAsset != null && !string.IsNullOrEmpty(lastAsset.AssetId))
            {
                // Extract numeric part from AssetId (e.g., "AST00365" -> 365)
                var numericPart = lastAsset.AssetId.Substring(3); // Skip "AST" prefix
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextAssetIdNumber = lastNumber + 1;
                }
            }

            // Process each row (skip header)
            for (int row = dataStartRow; row <= rowCount; row++)
            {
                try
                {
                    var excelRow = ReadExcelRow(worksheet, row, columnMapping);
                    
                    _logger.LogInformation("Row {Row}: AssetTag={AssetTag}, Make={Make}, Model={Model}, Type={Type}, Status={Status}", 
                        row, excelRow.Asset_Tag, excelRow.Make, excelRow.Model, excelRow.Asset_Type, excelRow.Status);
                    
                    // Log raw cell values for first data row
                    if (row == dataStartRow)
                    {
                        _logger.LogInformation("=== RAW DATA ROW {Row} ===", row);
                        for (int c = 1; c <= colCount; c++)
                        {
                            _logger.LogInformation("  Col {C}: '{Value}'", c, GetCellValue(worksheet, row, c));
                        }
                    }
                    
                    // Skip completely empty rows (common in Excel files with formatting)
                    if (IsRowEmpty(excelRow))
                    {
                        _logger.LogWarning("Skipping empty row {Row} — Tag='{Tag}' Make='{Make}' Model='{Model}' Type='{Type}' Status='{Status}' Region='{Region}' Location='{Location}'",
                            row, excelRow.Asset_Tag, excelRow.Make, excelRow.Model, excelRow.Asset_Type, excelRow.Status, excelRow.Region, excelRow.Location);
                        continue;
                    }

                    result.TotalRows++; // Count only non-empty rows
                    
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
                    var asset = await MapToAssetAsync(excelRow, nextAssetIdNumber, userId, usageCategory);
                    
                    if (asset != null)
                    {
                        assetsToInsert.Add(asset);
                        // Only track non-empty tags for duplicate detection (auto-generated tags are always unique)
                        if (!string.IsNullOrWhiteSpace(excelRow.Asset_Tag))
                            existingAssetTags.Add(asset.AssetTag.ToLower());
                        if (!string.IsNullOrEmpty(asset.SerialNumber))
                        {
                            existingSerialNumbers.Add(asset.SerialNumber.ToLower());
                        }
                        nextAssetIdNumber++;
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
                await _context.Assets.AddRangeAsync(assetsToInsert);
                await _context.SaveChangesAsync();
                result.SuccessCount = assetsToInsert.Count;
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

    // Find the actual header row (scan first 5 rows for the row with most recognized column names)
    private int FindHeaderRow(ExcelWorksheet worksheet, int colCount)
    {
        var knownHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Asset ID", "Asset_Tag", "AssetTag", "Asset Tag", "Tag", "Asset tag",
            "Make", "Manufacturer", "Brand",
            "Model", "Model Number",
            "Status", "Asset Status",
            "Asset Type", "Asset_Type", "AssetType", "Type", "Asset type",
            "Serial Number", "Serial_Number", "SerialNumber", "Serial No", "SN",
            "Device Serial Number", "Device Serial No", "Device Serial No.",
            "Region", "Location", "Department",
            "Sub Type", "Sub_Type", "SubType", "Subtype",
            "OS Type", "OS_Type", "OSType", "Operating System",
            "OS Version", "OS_Version", "OSVersion",
            "USB Blocking Status", "USB_Blocking_Status", "Status of USB Blocking",
            "Asset Classification", "Asset_Classification",
            "Assigned User", "Assigned_User_Name",
            "Procured By", "Procured_By",
            "Issue Date / Commissioning Date", "Commissioning Date", "Commissioning_Date",
            "Issue Date\\ commissioning", "Issue Date/ commissioning"
        };

        int bestRow = 1;
        int bestCount = 0;

        for (int row = 1; row <= Math.Min(5, worksheet.Dimension.Rows); row++)
        {
            int count = 0;
            for (int col = 1; col <= colCount; col++)
            {
                var val = GetCellValue(worksheet, row, col);
                if (knownHeaders.Contains(val)) count++;
            }
            _logger.LogInformation("Header scan row {Row}: {Count} recognized columns", row, count);
            if (count > bestCount)
            {
                bestCount = count;
                bestRow = row;
            }
        }

        _logger.LogInformation("Using row {Row} as header row ({Count} matches)", bestRow, bestCount);
        return bestRow;
    }

    private Dictionary<string, int> BuildColumnMapping(ExcelWorksheet worksheet, int colCount)
    {
        var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        // Define all possible column name variations
        var columnVariations = new Dictionary<string, string[]>
        {
            { "Asset_Tag", new[] { "Asset_Tag", "AssetTag", "Asset Tag", "Tag", "Asset ID", "Asset tag", "asset tag" } },
            { "Serial_Number", new[] { "Serial_Number", "SerialNumber", "Serial Number", "Serial No", "SN", "Device Serial Number", "Device Serial No", "Device Serial No." } },
            { "Region", new[] { "Region" } },
            { "Plaza_Name", new[] { "Plaza_Name", "PlazaName", "Plaza Name", "Plaza", "Site Name", "Site" } },
            { "Location", new[] { "Location", "Site Location", "State", "District" } },
            { "Department", new[] { "Department", "Dept" } },
            { "Asset_Type", new[] { "Asset_Type", "AssetType", "Asset Type", "Type", "Asset type" } },
            { "Sub_Type", new[] { "Sub_Type", "SubType", "Sub Type", "Subtype" } },
            { "Make", new[] { "Make", "Manufacturer", "Brand" } },
            { "Model", new[] { "Model", "Model Number" } },
            { "Asset_Classification", new[] { "Asset_Classification", "AssetClassification", "Asset Classification", "Classification", "Criticality" } },
            { "OS_Type", new[] { "OS_Type", "OSType", "OS Type", "Operating System", "OS" } },
            { "OS_Version", new[] { "OS_Version", "OSVersion", "OS Version" } },
            { "DB_Type", new[] { "DB_Type", "DBType", "DB Type", "Database Type", "Database" } },
            { "DB_Version", new[] { "DB_Version", "DBVersion", "DB Version", "Database Version" } },
            { "IP_Address", new[] { "IP_Address", "IPAddress", "IP Address", "IP" } },
            { "Assigned_User_Name", new[] { "Assigned_User_Name", "AssignedUserName", "Assigned User Name", "Assigned User", "User Name", "Username", "User By", "Used By" } },
            { "User_Role", new[] { "User_Role", "UserRole", "User Role", "Role" } },
            { "Procured_By", new[] { "Procured_By", "ProcuredBy", "Procured By", "Vendor" } },
            { "Commissioning_Date", new[] { "Commissioning_Date", "CommissioningDate", "Commissioning Date", "Commission Date", "Date", "Issue Date / Commissioning Date", "Issue Date", "Issue Date/ Commissioning Date", "Issue Date\\ commissioning", "Issue Date\\ Commissioning", "Issue Date/ commissioning" } },
            { "Status", new[] { "Status", "Asset Status" } },
            { "Criticality", new[] { "Criticality", "Critical Level", "Priority" } },
            { "Placing", new[] { "Placing", "Placement", "Area", "Location Area" } },
            { "Patch_Status", new[] { "Patch_Status", "PatchStatus", "Patch Status" } },
            { "USB_Blocking_Status", new[] { "USB_Blocking_Status", "USBBlockingStatus", "USB Blocking Status", "USB Status", "Status of USB Blocking", "Status of USB Blo" } },
            { "Remarks", new[] { "Remarks", "Notes", "Comments", "Description" } }
        };

        // Find actual header row and read it
        int headerRow = FindHeaderRow(worksheet, colCount);
        mapping["__headerRow__"] = headerRow; // store for caller

        for (int col = 1; col <= colCount; col++)
        {
            var headerValue = GetCellValue(worksheet, headerRow, col);
            if (string.IsNullOrWhiteSpace(headerValue))
                continue;

            _logger.LogInformation("  Raw header col {Col}: '{Header}'", col, headerValue);

            // Try to match with known column variations
            foreach (var kvp in columnVariations)
            {
                if (kvp.Value.Any(v => v.Equals(headerValue, StringComparison.OrdinalIgnoreCase)))
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
        // Only skip if every single mapped field is empty
        return string.IsNullOrWhiteSpace(row.Asset_Tag) &&
               string.IsNullOrWhiteSpace(row.Make) &&
               string.IsNullOrWhiteSpace(row.Model) &&
               string.IsNullOrWhiteSpace(row.Asset_Type) &&
               string.IsNullOrWhiteSpace(row.Status) &&
               string.IsNullOrWhiteSpace(row.Serial_Number) &&
               string.IsNullOrWhiteSpace(row.Region) &&
               string.IsNullOrWhiteSpace(row.Location) &&
               string.IsNullOrWhiteSpace(row.Assigned_User_Name);
    }

    private async Task<string> ValidateRow(AssetExcelRow row, List<string> existingAssetTags, List<string> existingSerialNumbers)
    {
        // Required fields (Asset_Tag is optional — defaults to "NA" if missing)
        if (string.IsNullOrWhiteSpace(row.Asset_Type))
            return "Asset_Type is required";

        if (string.IsNullOrWhiteSpace(row.Model))
            return "Model is required";

        if (string.IsNullOrWhiteSpace(row.Status))
            return "Status is required";

        // Duplicate check (skip if tag is empty — will be auto-generated as unique AssetId)
        if (!string.IsNullOrWhiteSpace(row.Asset_Tag) &&
            existingAssetTags.Contains(row.Asset_Tag.ToLower()))
            return "Asset_Tag already exists";

        if (!string.IsNullOrWhiteSpace(row.Serial_Number) && 
            existingSerialNumbers.Contains(row.Serial_Number.ToLower()))
            return "Serial_Number already exists";

        // Status validation - check if status exists in database
        var statusExists = await _context.AssetStatuses.AnyAsync(s => s.StatusName == row.Status);
        if (!statusExists)
            return $"Invalid Status: '{row.Status}' not found in database";

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

    private async Task<Asset?> MapToAssetAsync(AssetExcelRow row, int nextAssetIdNumber, int userId, string usageCategory = "ITNonTMS")
    {
        // Get default project and location for foreign key requirements
        var defaultProject = await _context.Projects.FirstOrDefaultAsync();
        var defaultLocation = await _context.Locations.FirstOrDefaultAsync();

        if (defaultProject == null || defaultLocation == null)
        {
            _logger.LogWarning("No default project or location found");
            return null;
        }

        // Lookup FK values from master tables using correct property names
        var assetType = await _context.AssetTypes.FirstOrDefaultAsync(x => x.TypeName == row.Asset_Type);
        
        // Auto-create asset type if it doesn't exist
        if (assetType == null && !string.IsNullOrWhiteSpace(row.Asset_Type))
        {
            assetType = new Domain.Entities.MasterData.AssetType
            {
                TypeName = row.Asset_Type,
                TypeCode = row.Asset_Type.ToUpper().Replace(" ", "_").Substring(0, Math.Min(row.Asset_Type.Length, 50)),
                CategoryId = 1, // Hardware
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            _context.AssetTypes.Add(assetType);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Auto-created asset type '{TypeName}'", row.Asset_Type);
        }
        var subType = string.IsNullOrEmpty(row.Sub_Type) ? null : 
            await _context.AssetSubTypes.FirstOrDefaultAsync(x => x.SubTypeName == row.Sub_Type);
        var status = await _context.AssetStatuses.FirstOrDefaultAsync(x => x.StatusName == row.Status);
        var placing = string.IsNullOrWhiteSpace(row.Placing)
            ? null
            : await _context.AssetPlacings.FirstOrDefaultAsync(x => x.Name == row.Placing);
        var classification = string.IsNullOrEmpty(row.Asset_Classification) ? null :
            await _context.AssetClassifications.FirstOrDefaultAsync(x => x.Name == row.Asset_Classification);
        var osType = string.IsNullOrEmpty(row.OS_Type) ? null :
            await _context.OperatingSystems.FirstOrDefaultAsync(x => x.Name == row.OS_Type);
        var dbType = string.IsNullOrEmpty(row.DB_Type) ? null :
            await _context.DatabaseTypes.FirstOrDefaultAsync(x => x.Name == row.DB_Type);
        var patchStatus = string.IsNullOrEmpty(row.Patch_Status) ? null :
            await _context.PatchStatuses.FirstOrDefaultAsync(x => x.Name == row.Patch_Status);
        var usbStatus = string.IsNullOrEmpty(row.USB_Blocking_Status) ? null :
            await _context.USBBlockingStatuses.FirstOrDefaultAsync(x => x.Name == row.USB_Blocking_Status);

        if (status == null)
        {
            _logger.LogError("Status '{Status}' not found", row.Status);
            throw new ArgumentException($"Status '{row.Status}' not found");
        }
        if (placing == null && !string.IsNullOrWhiteSpace(row.Placing))
        {
            _logger.LogError("Placing '{Placing}' not found", row.Placing);
            throw new ArgumentException($"Placing '{row.Placing}' not found");
        }

        return new Asset
        {
            AssetId = $"AST{nextAssetIdNumber:D5}",
            AssetTag = string.IsNullOrWhiteSpace(row.Asset_Tag) ? $"AST{nextAssetIdNumber:D5}" : row.Asset_Tag,
            SerialNumber = row.Serial_Number,
            ProjectId = defaultProject.Id,
            ProjectIdRef = defaultProject.ProjectId,
            LocationId = defaultLocation.Id,
            LocationIdRef = defaultLocation.LocationId,
            
            // Store location data as text from Excel (display as-is)
            Region = string.IsNullOrWhiteSpace(row.Region) ? "N/A" : row.Region,
            State = "N/A",
            Site = "N/A",
            PlazaName = string.IsNullOrWhiteSpace(row.Plaza_Name) ? "N/A" : row.Plaza_Name,
            LocationText = string.IsNullOrWhiteSpace(row.Location) ? "N/A" : row.Location,
            Department = string.IsNullOrWhiteSpace(row.Department) ? "N/A" : row.Department,
            
            OSVersion = string.IsNullOrWhiteSpace(row.OS_Version) ? "N/A" : row.OS_Version,
            DBVersion = string.IsNullOrWhiteSpace(row.DB_Version) ? "N/A" : row.DB_Version,
            IPAddress = string.IsNullOrWhiteSpace(row.IP_Address) ? "N/A" : row.IP_Address,
            ProcuredBy = string.IsNullOrWhiteSpace(row.Procured_By) ? "N/A" : row.Procured_By,
            Remarks = string.IsNullOrWhiteSpace(row.Remarks) ? "N/A" : row.Remarks,
            
            // FK assignments
            AssetTypeId = assetType.Id,
            AssetTypeName = assetType.TypeName,
            AssetSubTypeId = subType?.Id,
            AssetStatusId = status.Id,
            AssetPlacingId = placing?.Id,
            AssetClassificationId = classification?.Id,
            OperatingSystemId = osType?.Id,
            DatabaseTypeId = dbType?.Id,
            PatchStatusId = patchStatus?.Id,
            USBBlockingStatusId = usbStatus?.Id,
            
            Make = string.IsNullOrWhiteSpace(row.Make) ? "NA" : row.Make,
            Model = row.Model,
            UsageCategory = Enum.TryParse<AssetUsageCategory>(usageCategory, out var parsedCategory) ? parsedCategory : AssetUsageCategory.ITNonTMS,
            CommissioningDate = ParseDate(row.Commissioning_Date),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
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

            for (int row = 2; row <= rowCount; row++)
            {
                string Get(int col) => ws.Cells[row, col].Value?.ToString()?.Trim() ?? "";
                string licenseName = Get(1), version = Get(2), licenseKey = Get(3), licenseType = Get(4),
                       vendor = Get(5), publisher = Get(6), validityType = Get(7),
                       numStr = Get(8), purchaseDateStr = Get(9), startDateStr = Get(10), endDateStr = Get(11),
                       status = Get(12);

                if (string.IsNullOrWhiteSpace(licenseName) && string.IsNullOrWhiteSpace(licenseKey)) { result.TotalRows--; continue; }

                if (string.IsNullOrWhiteSpace(licenseName)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseKey, ErrorMessage = "LicenseName is required" }); continue; }
                if (string.IsNullOrWhiteSpace(licenseKey)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseName, ErrorMessage = "LicenseKey is required" }); continue; }
                if (!int.TryParse(numStr, out int numLicenses) || numLicenses <= 0) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseName, ErrorMessage = "NumberOfLicenses must be a positive integer" }); continue; }
                if (!DateTime.TryParse(startDateStr, out var startDate)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseName, ErrorMessage = "ValidityStartDate is invalid" }); continue; }
                if (!DateTime.TryParse(endDateStr, out var endDate)) { errors.Add(new BulkUploadError { RowNumber = row, AssetTag = licenseName, ErrorMessage = "ValidityEndDate is invalid" }); continue; }
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
