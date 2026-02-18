-- Migration: Rename Plaza column to Site in Locations table
-- Date: 2026-02-18
-- Description: Changes Plaza field to Site field to better reflect location types

USE ITAMS_Shared;
GO

-- Check if Plaza column exists and Site column doesn't exist
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Locations') AND name = 'Plaza')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Locations') AND name = 'Site')
BEGIN
    PRINT 'Renaming Plaza column to Site...';
    
    -- Rename the column
    EXEC sp_rename 'Locations.Plaza', 'Site', 'COLUMN';
    
    PRINT 'Column renamed successfully.';
END
ELSE
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Locations') AND name = 'Site')
    BEGIN
        PRINT 'Site column already exists. No changes needed.';
    END
    ELSE
    BEGIN
        PRINT 'Plaza column not found. Creating Site column...';
        ALTER TABLE Locations ADD Site NVARCHAR(100) NULL;
        PRINT 'Site column created.';
    END
END
GO

PRINT 'Migration completed successfully.';
GO
