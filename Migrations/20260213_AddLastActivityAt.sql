-- Add LastActivityAt column to Users table for real-time online status tracking
-- This will be updated on every API request to track active users across multiple systems

-- Add the column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'LastActivityAt')
BEGIN
    ALTER TABLE Users
    ADD LastActivityAt DATETIME2 NULL;
    
    PRINT 'Added LastActivityAt column to Users table';
    
    -- Initialize with LastLoginAt values for existing users
    UPDATE Users
    SET LastActivityAt = LastLoginAt
    WHERE LastLoginAt IS NOT NULL;
    
    PRINT 'Initialized LastActivityAt with LastLoginAt values';
END
ELSE
BEGIN
    PRINT 'LastActivityAt column already exists';
END

PRINT 'Migration completed successfully';

