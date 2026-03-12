-- ============================================================================
-- ITAMS Database Migration - Migrate Asset Data to FK References
-- Date: March 11, 2026
-- Purpose: Populate FK fields in Assets table from existing data
-- ============================================================================

-- Step 1: Migrate AssetType data
-- ============================================================================
UPDATE a
SET a.AssetTypeId = at.Id
FROM Assets a
INNER JOIN AssetTypes at ON a.AssetType = at.TypeName
WHERE a.AssetTypeId IS NULL AND a.AssetType IS NOT NULL;

-- Step 2: Migrate AssetStatus data (Status field contains numeric IDs)
-- ============================================================================
UPDATE a
SET a.AssetStatusId = CAST(a.Status AS INT)
FROM Assets a
WHERE a.AssetStatusId IS NULL AND a.Status IS NOT NULL AND ISNUMERIC(a.Status) = 1;

-- Step 3: Migrate AssetPlacing data
-- ============================================================================
UPDATE a
SET a.AssetPlacingId = p.Id
FROM Assets a
INNER JOIN AssetPlacings p ON a.Placing = p.Name
WHERE a.AssetPlacingId IS NULL AND a.Placing IS NOT NULL;

-- Step 4: Migrate Vendor data
-- ============================================================================
UPDATE a
SET a.VendorId = v.Id
FROM Assets a
INNER JOIN Vendors v ON a.Vendor = v.VendorName
WHERE a.VendorId IS NULL AND a.Vendor IS NOT NULL;

-- Step 5: Migrate AssetClassification data
-- ============================================================================
UPDATE a
SET a.AssetClassificationId = c.Id
FROM Assets a
INNER JOIN AssetClassifications c ON a.Classification = c.Name
WHERE a.AssetClassificationId IS NULL AND a.Classification IS NOT NULL;

-- Step 6: Migrate OperatingSystem data
-- ============================================================================
UPDATE a
SET a.OperatingSystemId = os.Id
FROM Assets a
INNER JOIN OperatingSystems os ON a.OSType = os.Name
WHERE a.OperatingSystemId IS NULL AND a.OSType IS NOT NULL;

-- Step 7: Migrate DatabaseType data
-- ============================================================================
UPDATE a
SET a.DatabaseTypeId = db.Id
FROM Assets a
INNER JOIN DatabaseTypes db ON a.DBType = db.Name
WHERE a.DatabaseTypeId IS NULL AND a.DBType IS NOT NULL;

-- Step 8: Migrate PatchStatus data
-- ============================================================================
UPDATE a
SET a.PatchStatusId = ps.Id
FROM Assets a
INNER JOIN PatchStatuses ps ON a.PatchStatus = ps.Name
WHERE a.PatchStatusId IS NULL AND a.PatchStatus IS NOT NULL;

-- Step 9: Migrate USBBlockingStatus data
-- ============================================================================
UPDATE a
SET a.USBBlockingStatusId = ubs.Id
FROM Assets a
INNER JOIN USBBlockingStatuses ubs ON a.USBBlockingStatus = ubs.Name
WHERE a.USBBlockingStatusId IS NULL AND a.USBBlockingStatus IS NOT NULL;

-- Step 10: Verify migration
-- ============================================================================
SELECT 
    COUNT(*) as TotalAssets,
    SUM(CASE WHEN AssetTypeId IS NOT NULL THEN 1 ELSE 0 END) as AssetsWithType,
    SUM(CASE WHEN AssetStatusId IS NOT NULL THEN 1 ELSE 0 END) as AssetsWithStatus,
    SUM(CASE WHEN AssetPlacingId IS NOT NULL THEN 1 ELSE 0 END) as AssetsWithPlacing
FROM Assets;
