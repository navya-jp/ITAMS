# Bulk Asset Upload - Setup Instructions

## Installation Steps

### 1. Install EPPlus Package
```powershell
# Run from project root
.\install-epplus.ps1

# Or manually:
dotnet add package EPPlus --version 7.0.0
```

### 2. Verify Files Created
Backend files:
- ✅ `Models/BulkUploadDtos.cs`
- ✅ `Services/BulkUploadService.cs`
- ✅ `Controllers/BulkUploadController.cs`
- ✅ `Program.cs` (updated)

Frontend files:
- ✅ `itams-frontend/src/app/bulk-upload/bulk-upload.ts`
- ✅ `itams-frontend/src/app/bulk-upload/bulk-upload.html`
- ✅ `itams-frontend/src/app/bulk-upload/bulk-upload.scss`
- ✅ `itams-frontend/src/app/app.routes.ts` (updated)
- ✅ `itams-frontend/src/app/navigation/navigation.ts` (updated)

## Features

### ✨ Dynamic Column Mapping
The system now supports **flexible Excel file uploads** with intelligent column recognition:

- **Any column order** - columns can be in any position
- **Multiple naming variations** - recognizes different column name formats
- **Case-insensitive** - works with any capitalization
- **No template required** - upload your existing Excel files directly

### Supported Column Name Variations

| Standard Name | Recognized Variations |
|--------------|----------------------|
| Asset_Tag | Asset_Tag, AssetTag, Asset Tag, Tag, Asset ID |
| Serial_Number | Serial_Number, SerialNumber, Serial Number, Serial No, SN |
| Asset_Type | Asset_Type, AssetType, Asset Type, Type |
| Make | Make, Manufacturer, Brand |
| Model | Model, Model Number |
| Status | Status, Asset Status |
| Plaza_Name | Plaza_Name, PlazaName, Plaza Name, Plaza, Site Name, Site |
| IP_Address | IP_Address, IPAddress, IP Address, IP |
| Commissioning_Date | Commissioning_Date, CommissioningDate, Commissioning Date, Commission Date, Date |
| Remarks | Remarks, Notes, Comments, Description |

*And many more variations for all 24 columns...*

## API Endpoints

### 1. Upload Assets
- **Endpoint**: `POST /api/assets/bulk-upload`
- **Content-Type**: `multipart/form-data`
- **Max File Size**: 50MB
- **Allowed Format**: .xlsx only
- **Authentication**: Required (Admin/SuperAdmin)

### 2. Download Template
- **Endpoint**: `GET /api/assets/download-template`
- **Returns**: Excel file with sample data and headers

## Required Columns

Your Excel file must contain these columns (any variation):
- **Asset_Tag** (or Tag, AssetTag, Asset ID, etc.)
- **Asset_Type** (or Type, AssetType, etc.)
- **Make** (or Manufacturer, Brand)
- **Model** (or Model Number)
- **Status** (or Asset Status)

## Optional Columns

- Serial_Number
- Region
- Plaza_Name / Site Name
- Location
- Department
- Sub_Type
- Asset_Classification
- OS_Type
- OS_Version
- DB_Type
- DB_Version
- IP_Address
- Assigned_User_Name
- User_Role
- Procured_By
- Commissioning_Date
- Patch_Status
- USB_Blocking_Status
- Remarks

## Validation Rules

1. **Asset_Tag**: Must be unique across all assets
2. **Serial_Number**: Must be unique (if provided)
3. **Status**: Must be one of: InUse, Spare, Repair, Decommissioned
4. **Commissioning_Date**: Must be a valid date format
5. **IP_Address**: Must be a valid IPv4 address (e.g., 192.168.1.1)

## Usage Examples

### Example 1: Standard Format
```
Asset_Tag | Serial_Number | Asset_Type | Make | Model | Status
ASSET001  | SN123456      | Laptop     | Dell | 5420  | InUse
```

### Example 2: Your Own Format
```
Tag | SN       | Type   | Manufacturer | Model Number | Asset Status
001 | SN123456 | Laptop | Dell         | 5420         | InUse
```

### Example 3: Different Order
```
Make | Model | Tag      | Type   | Status | Serial No
Dell | 5420  | ASSET001 | Laptop | InUse  | SN123456
```

**All three formats work perfectly!** 🎉

## How to Use

1. Navigate to **Bulk Upload** in the application menu
2. Select your Excel file or drag and drop
3. Click **Upload**
4. Review results:
   - Total rows processed
   - Success count
   - Failed count with error details
5. Fix any errors and re-upload failed rows if needed

## Error Handling

The system provides detailed error messages:
- Row number where error occurred
- Asset Tag (if available)
- Specific error message

Common errors:
- Missing required columns
- Duplicate Asset Tag or Serial Number
- Invalid date format
- Invalid IP address format
- Invalid status value

## Performance

- Optimized for large files (up to 50MB)
- Bulk insert for better performance
- Processes 1000+ rows efficiently
- Async processing to avoid timeouts

## Security

- Only Admin and SuperAdmin roles can upload
- File type validation (.xlsx only)
- File size validation (50MB max)
- SQL injection protection via EF Core
- Duplicate detection to prevent data corruption
