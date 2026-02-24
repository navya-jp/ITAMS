-- Make LocationId nullable temporarily so location creation works
ALTER TABLE Locations ALTER COLUMN LocationId NVARCHAR(50) NULL;

-- Verify the change
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Locations' 
AND COLUMN_NAME = 'LocationId';
