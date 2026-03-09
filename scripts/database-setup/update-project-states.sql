-- =====================================================
-- Update Project States from Assets
-- =====================================================

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '========================================';
    PRINT 'Updating Project States';
    PRINT '========================================';
    
    -- Update each project's States field with unique states from its assets
    DECLARE @ProjectId INT;
    DECLARE @StateList NVARCHAR(200);
    
    DECLARE project_cursor CURSOR FOR
    SELECT DISTINCT Id FROM Projects;
    
    OPEN project_cursor;
    FETCH NEXT FROM project_cursor INTO @ProjectId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Get unique states for this project
        SELECT @StateList = STUFF((
            SELECT DISTINCT ', ' + State
            FROM Assets
            WHERE ProjectId = @ProjectId
              AND State IS NOT NULL
              AND State != 'N/A'
              AND State != ''
            FOR XML PATH('')
        ), 1, 2, '');
        
        -- Update project
        IF @StateList IS NOT NULL
        BEGIN
            UPDATE Projects
            SET States = @StateList
            WHERE Id = @ProjectId;
            
            PRINT 'Updated project ID ' + CAST(@ProjectId AS VARCHAR(10)) + ' with states: ' + @StateList;
        END
        
        FETCH NEXT FROM project_cursor INTO @ProjectId;
    END
    
    CLOSE project_cursor;
    DEALLOCATE project_cursor;
    
    PRINT '';
    PRINT 'Project states updated successfully!';
    PRINT '';
    
    -- Show results
    PRINT 'Updated Projects:';
    SELECT 
        ProjectId,
        Name,
        States,
        (SELECT COUNT(*) FROM Assets WHERE ProjectId = p.Id) as AssetCount
    FROM Projects p
    WHERE States IS NOT NULL
    ORDER BY ProjectId;
    
    COMMIT TRANSACTION;
    PRINT '';
    PRINT 'Transaction committed successfully!';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
END CATCH;
