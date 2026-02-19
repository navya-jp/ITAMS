-- Migration: Add SessionId column to LoginAudit table
-- Date: 2026-02-19
-- Description: Add SessionId to track which session each login/logout belonged to

USE ITAMS_Shared;
GO

PRINT 'Adding SessionId column to LoginAudit table...';

-- Add SessionId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'LoginAudit' AND COLUMN_NAME = 'SessionId')
BEGIN
    ALTER TABLE LoginAudit
    ADD SessionId NVARCHAR(500) NULL;
    
    PRINT 'SessionId column added successfully.';
END
ELSE
BEGIN
    PRINT 'SessionId column already exists.';
END
GO

-- Display current structure
PRINT 'Current LoginAudit table structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoginAudit'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Migration completed successfully.';
