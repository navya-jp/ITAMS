# Bulk Upload Troubleshooting Guide

## Issue: Upload Returns 0 Rows

### Root Cause
Your Excel file is **missing the required "Placing" column** or has invalid values in required columns.

## Solution

### Step 1: Check Required Columns
Your Excel file MUST have these 8 columns:

| Column Name | Required | Example Values |
|-------------|----------|----------------|
| Asset_Tag | ✅ Yes | ASSET001, ASSET002 |
| Make | ✅ Yes | Dell, HP, Lenovo |
| Model | ✅ Yes | Latitude 5420, ThinkPad X1 |
| Asset_Type | ✅ Yes | Laptop, Desktop, Server |
| Status | ✅ Yes | In Use, Spare, Repair, Decommissioned |
| **Placing** | ✅ Yes | Lane Area, Booth Area, Plaza Area, Server Room, Control Room, Admin Building |
| Region | ✅ Yes | North, South, East, West |
| Location | ✅ Yes | Maharashtra, Delhi, Karnataka |

### Step 2: Add the Placing Column

1. Open your Excel file
2. Add a new column header called **"Placing"** (exact spelling, capital P)
3. Fill each row with ONE of these values (use Title Case):
   - **Lane Area**
   - **Booth Area**
   - **Plaza Area**
   - **Server Room**
   - **Control Room**
   - **Admin Building**

### Step 3: Verify Status Column

Make sure your Status column has valid values. The system accepts flexible formats:

| What You Can Write | System Converts To |
|-------------------|-------------------|
| decommissioned, Decommissioned, retired, disposed | Decommissioned |
| inuse, In Use, active, working, operational | In Use |
| spare, Spare, available, standby | Spare |
| repair, Repair, maintenance, faulty | Repair |

### Step 4: Example Excel Layout

```
Asset_Tag | Make | Model | Asset_Type | Status | Placing | Region | Location
ASSET001  | Dell | Latitude | Laptop | In Use | Server Room | North | Maharashtra
ASSET002  | HP   | ProBook  | Laptop | Spare  | Lane Area   | South | Karnataka
ASSET003  | Cisco| Switch   | Network| Repair | Control Room| East  | Delhi
```

## Download Template

You can download a pre-formatted template:
1. Go to the Assets page
2. Click "Download Template" button
3. Or visit: http://localhost:5066/api/assets/download-template

A file named `Asset_Upload_Template_Sample.xlsx` has been created in your project folder.

## Common Errors and Fixes

### Error: "Missing required columns: Placing"
**Fix**: Add a "Placing" column to your Excel file

### Error: "Invalid Placing: Invalid placing value"
**Fix**: Use exact Title Case values:
- ✅ "Lane Area" (correct)
- ❌ "lane area" (wrong - lowercase)
- ❌ "Lane area" (wrong - mixed case)
- ❌ "lanearea" (wrong - no space)

### Error: "Invalid Status"
**Fix**: Use one of the accepted status values (case-insensitive):
- In Use / inuse / active / working
- Spare / spare / available
- Repair / repair / maintenance
- Decommissioned / decommissioned / retired

### Error: "Region is required"
**Fix**: Fill the Region column for all rows

### Error: "Location (state or district name) is required"
**Fix**: Fill the Location column for all rows

## Optional Columns

These columns are optional but recommended:

- Serial_Number
- Plaza_Name
- Department
- Sub_Type
- Asset_Classification
- OS_Type, OS_Version
- DB_Type, DB_Version
- IP_Address
- Assigned_User_Name
- User_Role
- Procured_By
- Commissioning_Date
- Criticality (TMS general, TMS critical, IT general, IT critical)
- Patch_Status
- USB_Blocking_Status
- Remarks

## Testing Your File

### Before Upload Checklist:
- [ ] File has "Placing" column
- [ ] All Placing values are in Title Case (Lane Area, not lane area)
- [ ] All Status values are valid
- [ ] All required columns are filled
- [ ] No empty rows in the middle of data
- [ ] File is saved as .xlsx format

### After Upload:
1. Check the upload result message
2. If errors appear, they will show:
   - Row number
   - Asset tag
   - Specific error message
3. Fix the errors in your Excel file
4. Upload again

## Current System Status

- Backend: Running on http://localhost:5066 (Process ID 8)
- Frontend: Running on http://localhost:4200 (Process ID 2)
- Database: Empty (0 assets) - ready for fresh upload
- Display Format: Title Case for Status and Placing

## Need Help?

Run this command to see column requirements:
```powershell
.\check_excel_columns.ps1
```

Check backend logs:
```powershell
Get-Content "logs/itams-$(Get-Date -Format 'yyyyMMdd').txt" -Tail 50
```

## Success Indicators

When upload works correctly, you'll see:
- ✅ "Processed X rows. Success: X, Failed: 0"
- ✅ Assets appear in the Assets table
- ✅ Status displays as "In Use", "Spare", "Repair", or "Decommissioned"
- ✅ Placing displays as "Lane Area", "Booth Area", etc.

## Important Notes

1. **Placing is now REQUIRED** - This was added in the latest update
2. **Title Case matters for Placing** - Must be exact: "Lane Area" not "lane area"
3. **Status is flexible** - Can be lowercase, uppercase, or variations
4. **Partial success works** - Valid rows will be imported even if some rows fail
5. **No duplicates** - Asset_Tag and Serial_Number must be unique
