-- =====================================================
-- Script to Assign Assets to Projects
-- =====================================================

-- First, check current state
PRINT '=== Current Asset-Project Assignments ==='
SELECT 
    p.Name as ProjectName,
    COUNT(a.Id) as AssetCount
FROM Projects p
LEFT JOIN Assets a ON a.ProjectId = p.Id
GROUP BY p.Name
ORDER BY p.Name;

-- =====================================================
-- OPTION 1: Assign assets to projects based on PlazaName
-- =====================================================
-- This will match assets to projects where project name = plaza name

/*
UPDATE a
SET a.ProjectId = p.Id
FROM Assets a
INNER JOIN Projects p ON p.Name = a.PlazaName
WHERE a.PlazaName IS NOT NULL;

PRINT 'Assets assigned to projects based on PlazaName';
*/

-- =====================================================
-- OPTION 2: Assign assets to projects based on Region
-- =====================================================
-- This will match assets to projects where project name = region

/*
UPDATE a
SET a.ProjectId = p.Id
FROM Assets a
INNER JOIN Projects p ON p.Name = a.Region
WHERE a.Region IS NOT NULL;

PRINT 'Assets assigned to projects based on Region';
*/

-- =====================================================
-- OPTION 3: Assign all assets to default project
-- =====================================================
-- This will assign all assets to a single default project

/*
DECLARE @DefaultProjectId INT;
SELECT @DefaultProjectId = Id FROM Projects WHERE Code = 'DEFAULT';

UPDATE Assets
SET ProjectId = @DefaultProjectId
WHERE ProjectId IS NULL OR ProjectId = 0;

PRINT 'All assets assigned to default project';
*/

-- =====================================================
-- OPTION 4: Smart assignment based on available data
-- =====================================================
-- This will try to match by plaza first, then region, then default

/*
-- Step 1: Assign by PlazaName
UPDATE a
SET a.ProjectId = p.Id
FROM Assets a
INNER JOIN Projects p ON p.Name = a.PlazaName
WHERE a.PlazaName IS NOT NULL
  AND (a.ProjectId IS NULL OR a.ProjectId = 0);

PRINT 'Step 1: Assigned assets by PlazaName';

-- Step 2: Assign by Region for remaining assets
UPDATE a
SET a.ProjectId = p.Id
FROM Assets a
INNER JOIN Projects p ON p.Name = a.Region
WHERE a.Region IS NOT NULL
  AND (a.ProjectId IS NULL OR a.ProjectId = 0);

PRINT 'Step 2: Assigned assets by Region';

-- Step 3: Assign remaining to default project
DECLARE @DefaultProjectId INT;
SELECT @DefaultProjectId = Id FROM Projects WHERE Code = 'DEFAULT';

IF @DefaultProjectId IS NOT NULL
BEGIN
    UPDATE Assets
    SET ProjectId = @DefaultProjectId
    WHERE ProjectId IS NULL OR ProjectId = 0;
    
    PRINT 'Step 3: Assigned remaining assets to default project';
END
*/

-- =====================================================
-- Verify the assignments
-- =====================================================
PRINT '=== Final Asset-Project Assignments ==='
SELECT 
    p.ProjectId,
    p.Name as ProjectName,
    p.Code as ProjectCode,
    COUNT(a.Id) as AssetCount,
    STRING_AGG(DISTINCT a.Region, ', ') as Regions,
    STRING_AGG(DISTINCT a.State, ', ') as States
FROM Projects p
LEFT JOIN Assets a ON a.ProjectId = p.Id
GROUP BY p.ProjectId, p.Name, p.Code
ORDER BY p.ProjectId;

-- Check for any unassigned assets
PRINT '=== Unassigned Assets ==='
SELECT COUNT(*) as UnassignedCount
FROM Assets
WHERE ProjectId IS NULL OR ProjectId = 0;

-- Show sample of unassigned assets if any
SELECT TOP 10
    AssetId,
    AssetTag,
    AssetType,
    Region,
    State,
    PlazaName
FROM Assets
WHERE ProjectId IS NULL OR ProjectId = 0;
