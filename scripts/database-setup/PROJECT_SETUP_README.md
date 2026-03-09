# Project Setup and Asset Assignment Guide

## Overview
Ye scripts Excel se import kiye gaye assets ko proper projects aur locations ke under assign karne ke liye hain.

## Files Created

1. **analyze-and-create-projects.sql** - Analysis aur manual project creation ke liye
2. **assign-assets-to-projects.sql** - Assets ko projects assign karne ke liye
3. **setup-projects-and-assign-assets.sql** - Complete automated solution (RECOMMENDED)
4. **run-project-setup.ps1** - PowerShell script to run the SQL automatically

## Quick Start (Recommended Method)

### Option 1: Using PowerShell Script (Easiest)

```powershell
# Navigate to the scripts folder
cd scripts/database-setup

# Run the PowerShell script
.\run-project-setup.ps1
```

Script automatically:
- Database se connect karega
- Unique plazas find karega
- Projects create karega
- Locations create karega
- Assets assign karega
- Results show karega

### Option 2: Using SQL Server Management Studio (SSMS)

1. Open SSMS
2. Connect to: `192.168.208.26,1433`
3. Database: `ITAMS_Shared`
4. Open file: `setup-projects-and-assign-assets.sql`
5. Execute (F5)

## What the Script Does

### Step 1: Analysis
- Total assets count
- Assigned vs unassigned assets
- Unique plazas in assets

### Step 2: Create Projects
- Har unique plaza ke liye ek project banata hai
- Project ID: PRJ00001, PRJ00002, etc.
- Project Code: PRJ-Plaza-Name
- Example: "Laxmannath Fee Plaza" → Project "PRJ00001"

### Step 3: Create Locations
- Har project ke liye locations banata hai
- Region aur State ke basis par
- Location ID: LOC00001, LOC00002, etc.

### Step 4: Assign Assets to Projects
- Assets ko unke PlazaName ke basis par projects assign karta hai
- Example: Asset with PlazaName="Laxmannath Fee Plaza" → Project "Laxmannath Fee Plaza"

### Step 5: Assign Assets to Locations
- Assets ko unke Region aur State ke basis par locations assign karta hai

### Step 6: Verification
- Final statistics show karta hai
- Unassigned assets check karta hai

## Expected Results

After running the script:

```
Projects Created: ~10-15 (depending on unique plazas)
Locations Created: ~20-30 (depending on regions/states)
Assets Assigned: 799 (all assets)
Unassigned Assets: 0
```

## Verification Steps

### 1. Check Projects
```sql
SELECT * FROM Projects ORDER BY ProjectId;
```

### 2. Check Locations
```sql
SELECT 
    l.LocationId,
    l.Name,
    p.Name as ProjectName
FROM Locations l
INNER JOIN Projects p ON p.Id = l.ProjectId
ORDER BY p.Name, l.Name;
```

### 3. Check Asset Assignments
```sql
SELECT 
    p.Name as ProjectName,
    COUNT(a.Id) as AssetCount
FROM Projects p
LEFT JOIN Assets a ON a.ProjectId = p.Id
GROUP BY p.Name
ORDER BY AssetCount DESC;
```

### 4. Check for Unassigned Assets
```sql
SELECT COUNT(*) as UnassignedCount
FROM Assets
WHERE ProjectId IS NULL OR ProjectId = 0;
```

## Manual Options

If you want more control, use the individual scripts:

### Step 1: Analyze Data
```sql
-- Run: analyze-and-create-projects.sql
-- This will show you what plazas exist
```

### Step 2: Create Projects Manually
```sql
-- Uncomment one of the options in analyze-and-create-projects.sql:
-- OPTION 1: One project per plaza (RECOMMENDED)
-- OPTION 2: One project per region
-- OPTION 3: One default project for all
```

### Step 3: Assign Assets
```sql
-- Run: assign-assets-to-projects.sql
-- Uncomment the option you want:
-- OPTION 1: Assign by PlazaName (RECOMMENDED)
-- OPTION 2: Assign by Region
-- OPTION 3: Assign to default project
-- OPTION 4: Smart assignment (tries all methods)
```

## Troubleshooting

### Issue: "Projects already exist"
**Solution:** Script automatically skips existing projects. Safe to re-run.

### Issue: "Some assets still unassigned"
**Solution:** Check the unassigned assets query output. They might have NULL PlazaName.

```sql
SELECT AssetId, AssetTag, PlazaName, Region, State
FROM Assets
WHERE ProjectId IS NULL OR ProjectId = 0;
```

### Issue: "Location not found for asset"
**Solution:** Create location manually or update asset's Region/State to match existing location.

## Next Steps After Running Script

1. **Verify in Application**
   - Login as SuperAdmin
   - Go to Projects page
   - Check all projects are visible

2. **Assign Users to Projects**
   - Go to Users page
   - Edit each user
   - Assign them to appropriate project

3. **Test Access Control**
   - Login as regular user
   - Check they only see assets from their project
   - Login as Auditor
   - Check they see all assets

4. **Update User Permissions**
   - Assign appropriate permissions to users
   - Test CRUD operations

## Database Backup

**IMPORTANT:** Before running, take a backup:

```sql
BACKUP DATABASE ITAMS_Shared 
TO DISK = 'C:\Backups\ITAMS_Shared_BeforeProjectSetup.bak'
WITH FORMAT, INIT, NAME = 'Before Project Setup';
```

## Rollback (If Needed)

If something goes wrong, the script uses transactions and will automatically rollback. But if you need to manually undo:

```sql
-- Delete created projects (be careful!)
DELETE FROM Projects WHERE ProjectId LIKE 'PRJ%' AND Id > 1;

-- Reset asset assignments
UPDATE Assets SET ProjectId = NULL, LocationId = NULL;
```

## Support

If you face any issues:
1. Check the error message in the output
2. Verify database connection
3. Check if you have proper permissions
4. Review the SQL script for any syntax errors

## Summary

**Recommended Approach:**
1. Run `run-project-setup.ps1` PowerShell script
2. Verify results in application
3. Assign users to projects
4. Test access control

**Time Required:** 2-5 minutes

**Risk Level:** Low (uses transactions, auto-rollback on error)

**Reversible:** Yes (can delete projects and reset assignments)
