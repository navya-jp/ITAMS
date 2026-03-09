-- =====================================================
-- Complete Script: Create Projects and Assign Assets
-- =====================================================
-- This script will:
-- 1. Analyze existing assets
-- 2. Create projects based on unique plazas
-- 3. Create locations for each project
-- 4. Assign assets to appropriate projects and locations
-- =====================================================

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '========================================';
    PRINT 'STEP 1: Analyzing Existing Data';
    PRINT '========================================';
    
    -- Check current state
    DECLARE @TotalAssets INT;
    DECLARE @AssignedAssets INT;
    DECLARE @UnassignedAssets INT;
    
    SELECT @TotalAssets = COUNT(*) FROM Assets;
    SELECT @AssignedAssets = COUNT(*) FROM Assets WHERE ProjectId IS NOT NULL AND ProjectId > 0;
    SELECT @UnassignedAssets = COUNT(*) FROM Assets WHERE ProjectId IS NULL OR ProjectId = 0;
    
    PRINT 'Total Assets: ' + CAST(@TotalAssets AS VARCHAR(10));
    PRINT 'Assigned Assets: ' + CAST(@AssignedAssets AS VARCHAR(10));
    PRINT 'Unassigned Assets: ' + CAST(@UnassignedAssets AS VARCHAR(10));
    PRINT '';
    
    -- Show unique plazas
    PRINT 'Unique Plazas found in assets:';
    SELECT DISTINCT PlazaName, COUNT(*) as AssetCount
    FROM Assets
    WHERE PlazaName IS NOT NULL
    GROUP BY PlazaName
    ORDER BY PlazaName;
    PRINT '';
    
    PRINT '========================================';
    PRINT 'STEP 2: Creating Projects';
    PRINT '========================================';
    
    -- Get the next available project ID number
    DECLARE @NextProjectNum INT;
    SELECT @NextProjectNum = ISNULL(MAX(CAST(SUBSTRING(ProjectId, 4, 5) AS INT)), 0) + 1
    FROM Projects
    WHERE ProjectId LIKE 'PRJ%';
    
    -- Create projects for each unique plaza
    DECLARE @PlazaName NVARCHAR(200);
    DECLARE @ProjectId NVARCHAR(50);
    DECLARE @ProjectCode NVARCHAR(50);
    DECLARE @NewProjectId INT;
    
    DECLARE plaza_cursor CURSOR FOR
    SELECT DISTINCT PlazaName
    FROM Assets
    WHERE PlazaName IS NOT NULL
      AND PlazaName NOT IN (SELECT Name FROM Projects)
    ORDER BY PlazaName;
    
    OPEN plaza_cursor;
    FETCH NEXT FROM plaza_cursor INTO @PlazaName;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Generate Project ID and Code
        SET @ProjectId = 'PRJ' + RIGHT('00000' + CAST(@NextProjectNum AS VARCHAR(5)), 5);
        SET @ProjectCode = 'PRJ-' + REPLACE(REPLACE(@PlazaName, ' ', '-'), '.', '');
        
        -- Insert project
        INSERT INTO Projects (ProjectId, Name, Code, Description, IsActive, CreatedAt, CreatedBy)
        VALUES (
            @ProjectId,
            @PlazaName,
            @ProjectCode,
            'Project for ' + @PlazaName + ' toll plaza',
            1,
            GETUTCDATE(),
            1  -- SuperAdmin user ID
        );
        
        SET @NewProjectId = SCOPE_IDENTITY();
        
        PRINT 'Created project: ' + @PlazaName + ' (ID: ' + CAST(@NewProjectId AS VARCHAR(10)) + ', Code: ' + @ProjectId + ')';
        
        SET @NextProjectNum = @NextProjectNum + 1;
        
        FETCH NEXT FROM plaza_cursor INTO @PlazaName;
    END
    
    CLOSE plaza_cursor;
    DEALLOCATE plaza_cursor;
    PRINT '';
    
    PRINT '========================================';
    PRINT 'STEP 3: Creating Locations';
    PRINT '========================================';
    
    -- Get the next available location ID number
    DECLARE @NextLocationNum INT;
    SELECT @NextLocationNum = ISNULL(MAX(CAST(SUBSTRING(LocationId, 4, 5) AS INT)), 0) + 1
    FROM Locations
    WHERE LocationId LIKE 'LOC%';
    
    -- Create locations for each project based on unique region/state/plaza combinations
    DECLARE @Region NVARCHAR(100);
    DECLARE @State NVARCHAR(100);
    DECLARE @ProjectIdForLocation INT;
    DECLARE @LocationId NVARCHAR(50);
    DECLARE @LocationName NVARCHAR(200);
    
    DECLARE location_cursor CURSOR FOR
    SELECT DISTINCT 
        p.Id as ProjectId,
        a.Region,
        a.State,
        a.PlazaName
    FROM Assets a
    INNER JOIN Projects p ON p.Name = a.PlazaName
    WHERE a.Region IS NOT NULL
      AND a.State IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM Locations l 
          WHERE l.ProjectId = p.Id 
            AND l.Region = a.Region 
            AND l.State = a.State
      )
    ORDER BY p.Id, a.Region, a.State;
    
    OPEN location_cursor;
    FETCH NEXT FROM location_cursor INTO @ProjectIdForLocation, @Region, @State, @PlazaName;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Generate Location ID
        SET @LocationId = 'LOC' + RIGHT('00000' + CAST(@NextLocationNum AS VARCHAR(5)), 5);
        SET @LocationName = @PlazaName + ' - ' + @Region + ', ' + @State;
        
        -- Get the ProjectIdRef for this project
        DECLARE @ProjectIdRef NVARCHAR(50);
        SELECT @ProjectIdRef = ProjectId FROM Projects WHERE Id = @ProjectIdForLocation;
        
        -- Insert location
        INSERT INTO Locations (LocationId, Name, Region, State, ProjectId, ProjectIdRef, IsActive, CreatedAt)
        VALUES (
            @LocationId,
            @LocationName,
            @Region,
            @State,
            @ProjectIdForLocation,
            @ProjectIdRef,
            1,
            GETUTCDATE()
        );
        
        PRINT 'Created location: ' + @LocationName + ' (Code: ' + @LocationId + ')';
        
        SET @NextLocationNum = @NextLocationNum + 1;
        
        FETCH NEXT FROM location_cursor INTO @ProjectIdForLocation, @Region, @State, @PlazaName;
    END
    
    CLOSE location_cursor;
    DEALLOCATE location_cursor;
    PRINT '';
    
    PRINT '========================================';
    PRINT 'STEP 4: Assigning Assets to Projects';
    PRINT '========================================';
    
    -- Assign assets to projects based on PlazaName
    UPDATE a
    SET a.ProjectId = p.Id
    FROM Assets a
    INNER JOIN Projects p ON p.Name = a.PlazaName
    WHERE a.PlazaName IS NOT NULL;
    
    DECLARE @AssignedCount INT = @@ROWCOUNT;
    PRINT 'Assigned ' + CAST(@AssignedCount AS VARCHAR(10)) + ' assets to projects based on PlazaName';
    PRINT '';
    
    PRINT '========================================';
    PRINT 'STEP 5: Assigning Assets to Locations';
    PRINT '========================================';
    
    -- Assign assets to locations based on Project, Region, and State
    UPDATE a
    SET a.LocationId = l.Id
    FROM Assets a
    INNER JOIN Locations l ON l.ProjectId = a.ProjectId 
                          AND l.Region = a.Region 
                          AND l.State = a.State
    WHERE a.Region IS NOT NULL 
      AND a.State IS NOT NULL;
    
    SET @AssignedCount = @@ROWCOUNT;
    PRINT 'Assigned ' + CAST(@AssignedCount AS VARCHAR(10)) + ' assets to locations';
    PRINT '';
    
    PRINT '========================================';
    PRINT 'STEP 6: Verification';
    PRINT '========================================';
    
    -- Show final statistics
    PRINT 'Projects created:';
    SELECT 
        p.ProjectId,
        p.Name,
        p.Code,
        COUNT(a.Id) as AssetCount
    FROM Projects p
    LEFT JOIN Assets a ON a.ProjectId = p.Id
    GROUP BY p.ProjectId, p.Name, p.Code
    ORDER BY p.ProjectId;
    PRINT '';
    
    PRINT 'Locations created:';
    SELECT 
        l.LocationId,
        l.Name,
        p.Name as ProjectName,
        COUNT(a.Id) as AssetCount
    FROM Locations l
    INNER JOIN Projects p ON p.Id = l.ProjectId
    LEFT JOIN Assets a ON a.LocationId = l.Id
    GROUP BY l.LocationId, l.Name, p.Name
    ORDER BY p.Name, l.Name;
    PRINT '';
    
    -- Check for any remaining unassigned assets
    SELECT @UnassignedAssets = COUNT(*) 
    FROM Assets 
    WHERE ProjectId IS NULL OR ProjectId = 0;
    
    IF @UnassignedAssets > 0
    BEGIN
        PRINT 'WARNING: ' + CAST(@UnassignedAssets AS VARCHAR(10)) + ' assets still unassigned!';
        PRINT 'Sample unassigned assets:';
        SELECT TOP 5
            AssetId,
            AssetTag,
            AssetType,
            Region,
            State,
            PlazaName
        FROM Assets
        WHERE ProjectId IS NULL OR ProjectId = 0;
    END
    ELSE
    BEGIN
        PRINT 'SUCCESS: All assets have been assigned to projects!';
    END
    PRINT '';
    
    PRINT '========================================';
    PRINT 'TRANSACTION COMPLETED SUCCESSFULLY';
    PRINT '========================================';
    
    COMMIT TRANSACTION;
    PRINT 'Changes have been committed to the database.';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT '';
    PRINT '========================================';
    PRINT 'ERROR OCCURRED - TRANSACTION ROLLED BACK';
    PRINT '========================================';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    
END CATCH;

-- Final summary
PRINT '';
PRINT '========================================';
PRINT 'FINAL SUMMARY';
PRINT '========================================';

SELECT 
    'Total Projects' as Metric,
    COUNT(*) as Count
FROM Projects
UNION ALL
SELECT 
    'Total Locations' as Metric,
    COUNT(*) as Count
FROM Locations
UNION ALL
SELECT 
    'Total Assets' as Metric,
    COUNT(*) as Count
FROM Assets
UNION ALL
SELECT 
    'Assigned Assets' as Metric,
    COUNT(*) as Count
FROM Assets
WHERE ProjectId IS NOT NULL AND ProjectId > 0
UNION ALL
SELECT 
    'Unassigned Assets' as Metric,
    COUNT(*) as Count
FROM Assets
WHERE ProjectId IS NULL OR ProjectId = 0;
