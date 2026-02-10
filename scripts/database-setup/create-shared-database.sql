-- Create Shared Database and User for ITAMS
-- Run this script in SQL Server Management Studio as Administrator

USE master;
GO

-- Step 1: Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ITAMS_Shared')
BEGIN
    CREATE DATABASE ITAMS_Shared;
    PRINT 'Database ITAMS_Shared created successfully';
END
ELSE
BEGIN
    PRINT 'Database ITAMS_Shared already exists';
END
GO

-- Step 2: Enable SQL Server Authentication (Mixed Mode)
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', 
    N'Software\Microsoft\MSSQLServer\MSSQLServer', 
    N'LoginMode', REG_DWORD, 2;
PRINT 'SQL Server Authentication enabled (Mixed Mode)';
GO

-- Step 3: Create login for remote access
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'itams_user')
BEGIN
    CREATE LOGIN itams_user WITH PASSWORD = 'ITAMS@2024!';
    PRINT 'Login itams_user created successfully';
END
ELSE
BEGIN
    PRINT 'Login itams_user already exists';
END
GO

-- Step 4: Switch to ITAMS_Shared database and create user
USE ITAMS_Shared;
GO

IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'itams_user')
BEGIN
    CREATE USER itams_user FOR LOGIN itams_user;
    PRINT 'User itams_user created in ITAMS_Shared database';
END
ELSE
BEGIN
    PRINT 'User itams_user already exists in database';
END
GO

-- Step 5: Grant permissions to the user
ALTER ROLE db_owner ADD MEMBER itams_user;
PRINT 'User itams_user granted db_owner permissions';
GO

-- Step 6: Verify the setup
SELECT 
    'Database: ' + DB_NAME() as Info
UNION ALL
SELECT 
    'User: ' + name as Info
FROM sys.database_principals 
WHERE name = 'itams_user'
UNION ALL
SELECT 
    'Login exists: ' + CASE WHEN EXISTS(SELECT 1 FROM sys.server_principals WHERE name = 'itams_user') THEN 'YES' ELSE 'NO' END as Info;

PRINT 'Setup verification completed';
GO