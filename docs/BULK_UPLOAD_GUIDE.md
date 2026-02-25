# Bulk Upload User Guide

## Overview

The Bulk Upload feature allows you to upload multiple assets at once using an Excel file. The system is smart enough to recognize your column headers regardless of their names or order.

## Quick Start

1. **Prepare Your Excel File**
   - Make sure you have the required columns (see below)
   - Your file can have columns in any order
   - Column names can vary (see supported variations)

2. **Upload Your File**
   - Go to "Bulk Upload" in the menu
   - Click "Select File" or drag and drop your Excel file
   - Click "Upload"

3. **Review Results**
   - See how many rows were processed successfully
   - Check any errors and fix them
   - Re-upload if needed

## Required Columns

Your Excel file MUST have these columns (any variation works):

| Column | Variations Accepted |
|--------|-------------------|
| Asset Tag | Asset_Tag, AssetTag, Asset Tag, Tag, Asset ID |
| Asset Type | Asset_Type, AssetType, Asset Type, Type |
| Make | Make, Manufacturer, Brand |
| Model | Model, Model Number |
| Status | Status, Asset Status |

## Optional Columns

You can include any of these columns (all optional):

- Serial Number (Serial_Number, SerialNumber, Serial No, SN)
- Region
- Site Name (Plaza_Name, PlazaName, Plaza Name, Plaza, Site Name, Site)
- Location
- Department
- Sub Type (Sub_Type, SubType, Sub Type, Subtype)
- Asset Classification
- OS Type (OS_Type, OSType, OS Type, Operating System, OS)
- OS Version
- Database Type (DB_Type, DBType, DB Type, Database Type, Database)
- Database Version
- IP Address (IP_Address, IPAddress, IP Address, IP)
- Assigned User Name (Assigned_User_Name, AssignedUserName, Assigned User, Username)
- User Role (User_Role, UserRole, User Role, Role)
- Procured By (Procured_By, ProcuredBy, Procured By, Vendor)
- Commissioning Date (Commissioning_Date, CommissioningDate, Commission Date, Date)
- Patch Status
- USB Blocking Status
- Remarks (Remarks, Notes, Comments, Description)

## Data Validation

### Status Values
Must be one of:
- InUse
- Spare
- Repair
- Decommissioned

### Date Format
Commissioning Date should be in a standard date format:
- 2024-01-15
- 01/15/2024
- 15-Jan-2024

### IP Address Format
Must be a valid IPv4 address:
- 192.168.1.100
- 10.0.0.1

### Unique Fields
These must be unique across all assets:
- Asset Tag (required to be unique)
- Serial Number (if provided, must be unique)

## Example Excel Files

### Example 1: Standard Format
```
Asset_Tag | Serial_Number | Asset_Type | Make | Model | Status
LAPTOP001 | SN123456      | Laptop     | Dell | 5420  | InUse
LAPTOP002 | SN123457      | Laptop     | HP   | 840   | InUse
```

### Example 2: Your Own Column Names
```
Tag | SN       | Type   | Manufacturer | Model Number | Asset Status
001 | SN123456 | Laptop | Dell         | 5420         | InUse
002 | SN123457 | Laptop | HP           | 840          | InUse
```

### Example 3: Different Column Order
```
Make | Model | Tag       | Type   | Status | Serial No
Dell | 5420  | LAPTOP001 | Laptop | InUse  | SN123456
HP   | 840   | LAPTOP002 | Laptop | InUse  | SN123457
```

### Example 4: With Optional Columns
```
Tag | Type | Make | Model | Status | IP Address  | Site Name | Commissioning Date
001 | Laptop | Dell | 5420 | InUse | 192.168.1.10 | HQ | 2024-01-15
002 | Laptop | HP | 840 | InUse | 192.168.1.11 | Branch | 2024-01-20
```

**All these formats work!** The system automatically detects your column structure.

## Common Errors and Solutions

### Error: "Missing required columns"
**Solution**: Make sure your Excel has all 5 required columns (Asset Tag, Asset Type, Make, Model, Status)

### Error: "Asset_Tag already exists"
**Solution**: Each asset tag must be unique. Check for duplicates in your file or existing assets.

### Error: "Serial_Number already exists"
**Solution**: Serial numbers must be unique. Remove duplicates or leave blank if not available.

### Error: "Status must be one of: InUse, Spare, Repair, Decommissioned"
**Solution**: Use only the valid status values listed above.

### Error: "Commissioning_Date is not a valid date"
**Solution**: Use a standard date format like YYYY-MM-DD or MM/DD/YYYY.

### Error: "IP_Address is not a valid IPv4 address"
**Solution**: Use format like 192.168.1.1 (four numbers separated by dots).

## Tips for Success

1. **Start Small**: Test with 5-10 rows first before uploading hundreds
2. **Check Duplicates**: Make sure Asset Tags and Serial Numbers are unique
3. **Use Valid Status**: Only use InUse, Spare, Repair, or Decommissioned
4. **Date Format**: Use consistent date format throughout your file
5. **Review Errors**: If upload fails, check the error details for specific row numbers
6. **Keep Backup**: Always keep a backup of your original Excel file

## File Limits

- **Maximum File Size**: 50MB
- **File Format**: .xlsx only (Excel 2007 and later)
- **Recommended Rows**: Up to 10,000 rows per file

## Need Help?

If you encounter issues:
1. Check the error message for specific row numbers
2. Verify your column names match the supported variations
3. Ensure all required columns are present
4. Contact your system administrator if problems persist

## Download Sample Template

Click the "Download Template" button in the Bulk Upload page to get a sample Excel file with all columns and example data.
