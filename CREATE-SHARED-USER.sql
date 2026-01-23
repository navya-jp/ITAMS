-- Create SQL Server login and user for shared database access
USE master;
GO

-- Create login for network access
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'itams_user')
BEGIN
    CREATE LOGIN itams_user WITH PASSWORD = 'ITAMS@2024!';
    PRINT 'Login itams_user created successfully';
END
ELSE
BEGIN
    PRINT 'Login itams_user already exists';
END
GO

-- Switch to ITAMS database
USE ITAMS;
GO

-- Create user in database
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'itams_user')
BEGIN
    CREATE USER itams_user FOR LOGIN itams_user;
    PRINT 'User itams_user created successfully';
END
ELSE
BEGIN
    PRINT 'User itams_user already exists';
END
GO

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER itams_user;
PRINT 'Permissions granted to itams_user';
GO

PRINT 'Shared database user setup complete!';