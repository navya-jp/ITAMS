-- Make ProjectId nullable temporarily so user creation works
ALTER TABLE Users ALTER COLUMN ProjectId INT NULL;

-- Make RoleIdRef nullable temporarily
ALTER TABLE Users ALTER COLUMN RoleIdRef NVARCHAR(50) NULL;

-- Verify the changes
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' 
AND COLUMN_NAME IN ('ProjectId', 'RoleIdRef');
