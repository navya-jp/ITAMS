-- =====================================================
-- Script to Analyze Assets and Create Projects
-- =====================================================

-- Step 1: Check current assets and their project assignments
SELECT 
    ProjectId,
    COUNT(*) as AssetCount,
    STRING_AGG(DISTINCT Region, ', ') as Regions,
    STRING_AGG(DISTINCT State, ', ') as States,
    STRING_AGG(DISTINCT PlazaName, ', ') as Plazas
FROM Assets
GROUP BY ProjectId
ORDER BY ProjectId;

-- Step 2: Check if we have a default project
SELECT * FROM Projects;

-- Step 3: Analyze unique regions/states/plazas from assets
SELECT DISTINCT 
    Region,
    State,
    PlazaName,
    COUNT(*) as AssetCount
FROM Assets
WHERE Region IS NOT NULL
GROUP BY Region, State, PlazaName
ORDER BY Region, State, PlazaName;

-- Step 4: Check unique plaza names (these could be projects)
SELECT DISTINCT 
    PlazaName,
    Region,
    State,
    COUNT(*) as AssetCount
FROM Assets
WHERE PlazaName IS NOT NULL
GROUP BY PlazaName, Region, State
ORDER BY PlazaName;

-- =====================================================
-- CREATE PROJECTS BASED ON PLAZA NAMES
-- =====================================================

-- First, let's see what we have
PRINT '=== Current Projects ==='
SELECT * FROM Projects;

PRINT '=== Unique Plazas in Assets ==='
SELECT DISTINCT PlazaName, COUNT(*) as AssetCount
FROM Assets
WHERE PlazaName IS NOT NULL
GROUP BY PlazaName
ORDER BY PlazaName;

-- =====================================================
-- OPTION 1: Create one project per Plaza
-- =====================================================
-- Uncomment below to create projects for each plaza

/*
DECLARE @Counter INT = 1;
DECLARE @PlazaName NVARCHAR(200);
DECLARE @ProjectCode NVARCHAR(50);
DECLARE @ProjectId NVARCHAR(50);

DECLARE plaza_cursor CURSOR FOR
SELECT DISTINCT PlazaName
FROM Assets
WHERE PlazaName IS NOT NULL
ORDER BY PlazaName;

OPEN plaza_cursor;
FETCH NEXT FROM plaza_cursor INTO @PlazaName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Generate Project ID and Code
    SET @ProjectId = 'PRJ' + RIGHT('00000' + CAST(@Counter AS VARCHAR(5)), 5);
    SET @ProjectCode = 'PRJ-' + REPLACE(@PlazaName, ' ', '-');
    
    -- Insert project if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM Projects WHERE Name = @PlazaName)
    BEGIN
        INSERT INTO Projects (ProjectId, Name, Code, Description, IsActive, CreatedAt)
        VALUES (
            @ProjectId,
            @PlazaName,
            @ProjectCode,
            'Project for ' + @PlazaName + ' plaza',
            1,
            GETUTCDATE()
        );
        
        PRINT 'Created project: ' + @PlazaName + ' (' + @ProjectId + ')';
        SET @Counter = @Counter + 1;
    END
    
    FETCH NEXT FROM plaza_cursor INTO @PlazaName;
END

CLOSE plaza_cursor;
DEALLOCATE plaza_cursor;
*/

-- =====================================================
-- OPTION 2: Create one project per Region
-- =====================================================
-- Uncomment below to create projects for each region

/*
DECLARE @Counter INT = 1;
DECLARE @Region NVARCHAR(100);
DECLARE @ProjectCode NVARCHAR(50);
DECLARE @ProjectId NVARCHAR(50);

DECLARE region_cursor CURSOR FOR
SELECT DISTINCT Region
FROM Assets
WHERE Region IS NOT NULL
ORDER BY Region;

OPEN region_cursor;
FETCH NEXT FROM region_cursor INTO @Region;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Generate Project ID and Code
    SET @ProjectId = 'PRJ' + RIGHT('00000' + CAST(@Counter AS VARCHAR(5)), 5);
    SET @ProjectCode = 'PRJ-' + REPLACE(@Region, ' ', '-');
    
    -- Insert project if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM Projects WHERE Name = @Region)
    BEGIN
        INSERT INTO Projects (ProjectId, Name, Code, Description, IsActive, CreatedAt)
        VALUES (
            @ProjectId,
            @Region,
            @ProjectCode,
            'Project for ' + @Region + ' region',
            1,
            GETUTCDATE()
        );
        
        PRINT 'Created project: ' + @Region + ' (' + @ProjectId + ')';
        SET @Counter = @Counter + 1;
    END
    
    FETCH NEXT FROM region_cursor INTO @Region;
END

CLOSE region_cursor;
DEALLOCATE region_cursor;
*/

-- =====================================================
-- OPTION 3: Create a single default project for all assets
-- =====================================================
-- Uncomment below to create one default project

/*
IF NOT EXISTS (SELECT 1 FROM Projects WHERE Code = 'DEFAULT')
BEGIN
    INSERT INTO Projects (ProjectId, Name, Code, Description, IsActive, CreatedAt)
    VALUES (
        'PRJ00001',
        'Default Project',
        'DEFAULT',
        'Default project for all imported assets',
        1,
        GETUTCDATE()
    );
    
    PRINT 'Created default project';
END
*/

-- =====================================================
-- After creating projects, run this to see the results
-- =====================================================
SELECT * FROM Projects ORDER BY ProjectId;
