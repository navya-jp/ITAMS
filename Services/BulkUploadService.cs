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

    public async Task<BulkUploadResult> ProcessAssetExcelAsync(Stream fileStream, int userId)
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

            // Build column mapping from header row
            var columnMapping = BuildColumnMapping(worksheet, colCount);
            
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

            result.TotalRows = rowCount - 1; // Exclude header row

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
            for (int row = 2; row <= rowCount; row++)
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
                    
                    var validationError = ValidateRow(excelRow, existingAssetTags, existingSerialNumbers);

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
                    var asset = await MapToAssetAsync(excelRow, nextAssetIdNumber, userId);
                    
                    if (asset != null)
                    {
                        assetsToInsert.Add(asset);
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

    private Dictionary<string, int> BuildColumnMapping(ExcelWorksheet worksheet, int colCount)
    {
        var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        // Define all possible column name variations
        var columnVariations = new Dictionary<string, string[]>
        {
            { "Asset_Tag", new[] { "Asset_Tag", "AssetTag", "Asset Tag", "Tag", "Asset ID" } },
            { "Serial_Number", new[] { "Serial_Number", "SerialNumber", "Serial Number", "Serial No", "SN" } },
            { "Region", new[] { "Region" } },
            { "Plaza_Name", new[] { "Plaza_Name", "PlazaName", "Plaza Name", "Plaza", "Site Name", "Site" } },
            { "Location", new[] { "Location", "Site Location", "State", "District" } },
            { "Department", new[] { "Department", "Dept" } },
            { "Asset_Type", new[] { "Asset_Type", "AssetType", "Asset Type", "Type" } },
            { "Sub_Type", new[] { "Sub_Type", "SubType", "Sub Type", "Subtype" } },
            { "Make", new[] { "Make", "Manufacturer", "Brand" } },
            { "Model", new[] { "Model", "Model Number" } },
            { "Asset_Classification", new[] { "Asset_Classification", "AssetClassification", "Asset Classification", "Classification", "Criticality" } },
            { "OS_Type", new[] { "OS_Type", "OSType", "OS Type", "Operating System", "OS" } },
            { "OS_Version", new[] { "OS_Version", "OSVersion", "OS Version" } },
            { "DB_Type", new[] { "DB_Type", "DBType", "DB Type", "Database Type", "Database" } },
            { "DB_Version", new[] { "DB_Version", "DBVersion", "DB Version", "Database Version" } },
            { "IP_Address", new[] { "IP_Address", "IPAddress", "IP Address", "IP" } },
            { "Assigned_User_Name", new[] { "Assigned_User_Name", "AssignedUserName", "Assigned User Name", "Assigned User", "User Name", "Username" } },
            { "User_Role", new[] { "User_Role", "UserRole", "User Role", "Role" } },
            { "Procured_By", new[] { "Procured_By", "ProcuredBy", "Procured By", "Vendor" } },
            { "Commissioning_Date", new[] { "Commissioning_Date", "CommissioningDate", "Commissioning Date", "Commission Date", "Date" } },
            { "Status", new[] { "Status", "Asset Status" } },
            { "Criticality", new[] { "Criticality", "Critical Level", "Priority" } },
            { "Placing", new[] { "Placing", "Placement", "Area", "Location Area" } },
            { "Patch_Status", new[] { "Patch_Status", "PatchStatus", "Patch Status" } },
            { "USB_Blocking_Status", new[] { "USB_Blocking_Status", "USBBlockingStatus", "USB Blocking Status", "USB Status" } },
            { "Remarks", new[] { "Remarks", "Notes", "Comments", "Description" } }
        };

        // Read header row and build mapping
        for (int col = 1; col <= colCount; col++)
        {
            var headerValue = GetCellValue(worksheet, 1, col);
            if (string.IsNullOrWhiteSpace(headerValue))
                continue;

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
        var requiredColumns = new[] { "Asset_Tag", "Asset_Type", "Make", "Model", "Status", "Placing" };
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
        // A row is considered empty if all critical fields are empty
        return string.IsNullOrWhiteSpace(row.Asset_Tag) &&
               string.IsNullOrWhiteSpace(row.Make) &&
               string.IsNullOrWhiteSpace(row.Model) &&
               string.IsNullOrWhiteSpace(row.Asset_Type) &&
               string.IsNullOrWhiteSpace(row.Status);
    }

    private string ValidateRow(AssetExcelRow row, List<string> existingAssetTags, List<string> existingSerialNumbers)
    {
        // Required fields
        if (string.IsNullOrWhiteSpace(row.Asset_Tag))
            return "Asset_Tag is required";

        if (string.IsNullOrWhiteSpace(row.Asset_Type))
            return "Asset_Type is required";

        if (string.IsNullOrWhiteSpace(row.Make))
            return "Make is required";

        if (string.IsNullOrWhiteSpace(row.Model))
            return "Model is required";

        if (string.IsNullOrWhiteSpace(row.Status))
            return "Status is required";

        if (string.IsNullOrWhiteSpace(row.Placing))
            return "Placing is required";

        // Duplicate check
        if (existingAssetTags.Contains(row.Asset_Tag.ToLower()))
            return "Asset_Tag already exists";

        if (!string.IsNullOrWhiteSpace(row.Serial_Number) && 
            existingSerialNumbers.Contains(row.Serial_Number.ToLower()))
            return "Serial_Number already exists";

        // Status validation - strict validation using AssetEnumHelpers
        try
        {
            AssetEnumHelpers.ParseStatus(row.Status);
        }
        catch (ArgumentException ex)
        {
            return $"Invalid Status: {ex.Message}";
        }

                // Placing validation - strict validation
        try
        {
            AssetEnumHelpers.ValidatePlacing(row.Placing);
        }
        catch (ArgumentException ex)
        {
            return $"Invalid Placing: {ex.Message}";
        }

        // Date validation
        if (!string.IsNullOrWhiteSpace(row.Commissioning_Date) && !IsValidDate(row.Commissioning_Date))
            return "Commissioning_Date is not a valid date";

        // IP Address validation
        if (!string.IsNullOrWhiteSpace(row.IP_Address) && !IsValidIPv4(row.IP_Address))
            return "IP_Address is not a valid IPv4 address";

        return string.Empty;
    }

    private async Task<Asset?> MapToAssetAsync(AssetExcelRow row, int nextAssetIdNumber, int userId)
    {
        // Get default project and location for foreign key requirements
        var defaultProject = await _context.Projects.FirstOrDefaultAsync();
        var defaultLocation = await _context.Locations.FirstOrDefaultAsync();

        if (defaultProject == null || defaultLocation == null)
        {
            _logger.LogWarning("No default project or location found");
            return null;
        }

        // Parse and validate status using AssetEnumHelpers
        AssetStatus status;
        try
        {
            status = AssetEnumHelpers.ParseStatus(row.Status);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Failed to parse status '{Status}': {Error}", row.Status, ex.Message);
            throw; // Re-throw to be caught by caller
        }

                // Validate placing using AssetEnumHelpers
        string placing;
        try
        {
            placing = AssetEnumHelpers.ValidatePlacing(row.Placing);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Failed to validate placing '{Placing}': {Error}", row.Placing, ex.Message);
            throw; // Re-throw to be caught by caller
        }

        return new Asset
        {
            AssetId = $"AST{nextAssetIdNumber:D5}",
            AssetTag = row.Asset_Tag,
            SerialNumber = row.Serial_Number,
            ProjectId = defaultProject.Id,
            ProjectIdRef = defaultProject.ProjectId,
            LocationId = defaultLocation.Id,
            LocationIdRef = defaultLocation.LocationId,
            
            // Store location data as text from Excel (display as-is)
            Region = string.IsNullOrWhiteSpace(row.Region) ? "N/A" : row.Region,
            State = "N/A", // Not in current Excel mapping
            Site = "N/A", // Not in current Excel mapping  
            PlazaName = string.IsNullOrWhiteSpace(row.Plaza_Name) ? "N/A" : row.Plaza_Name,
            LocationText = string.IsNullOrWhiteSpace(row.Location) ? "N/A" : row.Location,
            Department = string.IsNullOrWhiteSpace(row.Department) ? "N/A" : row.Department,
            
            // Extended asset fields
            Classification = string.IsNullOrWhiteSpace(row.Asset_Classification) ? "N/A" : row.Asset_Classification,
            OSType = string.IsNullOrWhiteSpace(row.OS_Type) ? "N/A" : row.OS_Type,
            OSVersion = string.IsNullOrWhiteSpace(row.OS_Version) ? "N/A" : row.OS_Version,
            DBType = string.IsNullOrWhiteSpace(row.DB_Type) ? "N/A" : row.DB_Type,
            DBVersion = string.IsNullOrWhiteSpace(row.DB_Version) ? "N/A" : row.DB_Version,
            IPAddress = string.IsNullOrWhiteSpace(row.IP_Address) ? "N/A" : row.IP_Address,
            AssignedUserText = string.IsNullOrWhiteSpace(row.Assigned_User_Name) ? "N/A" : row.Assigned_User_Name,
            UserRole = string.IsNullOrWhiteSpace(row.User_Role) ? "N/A" : row.User_Role,
            ProcuredBy = string.IsNullOrWhiteSpace(row.Procured_By) ? "N/A" : row.Procured_By,
            PatchStatus = string.IsNullOrWhiteSpace(row.Patch_Status) ? "N/A" : row.Patch_Status,
            USBBlockingStatus = string.IsNullOrWhiteSpace(row.USB_Blocking_Status) ? "N/A" : row.USB_Blocking_Status,
            Remarks = string.IsNullOrWhiteSpace(row.Remarks) ? "N/A" : row.Remarks,
            
            AssetType = row.Asset_Type,
            SubType = row.Sub_Type,
            Make = row.Make,
            Model = row.Model,
            UsageCategory = AssetUsageCategory.ITNonTMS, // Default
                        Status = status,
            Placing = placing,
            CommissioningDate = ParseDate(row.Commissioning_Date),
            AssignedUserRole = row.User_Role,
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

    public byte[] GenerateSampleTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Assets");

        // Headers
        var headers = new[]
        {
            "Asset_Tag", "Serial_Number", "Region", "Plaza_Name", "Location",
            "Department", "Asset_Type", "Sub_Type", "Make", "Model",
            "Asset_Classification", "OS_Type", "OS_Version", "DB_Type", "DB_Version",
            "IP_Address", "Assigned_User_Name", "User_Role", "Procured_By",
            "Commissioning_Date", "Status", "Criticality", "Placing", 
            "Patch_Status", "USB_Blocking_Status", "Remarks"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Sample data
        worksheet.Cells[2, 1].Value = "ASSET001";
        worksheet.Cells[2, 2].Value = "SN123456";
        worksheet.Cells[2, 3].Value = "North";
        worksheet.Cells[2, 4].Value = "Plaza A";
        worksheet.Cells[2, 5].Value = "Maharashtra";
        worksheet.Cells[2, 6].Value = "IT";
        worksheet.Cells[2, 7].Value = "Laptop";
        worksheet.Cells[2, 8].Value = "Business";
        worksheet.Cells[2, 9].Value = "Dell";
        worksheet.Cells[2, 10].Value = "Latitude 5420";
        worksheet.Cells[2, 20].Value = "2024-01-15";
        worksheet.Cells[2, 21].Value = "inuse";
        worksheet.Cells[2, 22].Value = "IT general";
        worksheet.Cells[2, 23].Value = "server room";

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }
}
