-- Migration: Create SystemSettings table for configurable application settings
-- Date: 2026-02-19
-- Description: Allows Super Admin to configure session timeout and other system settings

USE ITAMS_Shared;
GO

PRINT 'Creating SystemSettings table...';

-- Create SystemSettings table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SystemSettings] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [SettingKey] NVARCHAR(100) NOT NULL UNIQUE,
        [SettingValue] NVARCHAR(500) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Category] NVARCHAR(50) NOT NULL DEFAULT 'General',
        [DataType] NVARCHAR(20) NOT NULL DEFAULT 'String', -- String, Integer, Boolean, Decimal
        [IsEditable] BIT NOT NULL DEFAULT 1,
        [UpdatedBy] INT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    PRINT 'SystemSettings table created successfully.';
END
ELSE
BEGIN
    PRINT 'SystemSettings table already exists.';
END
GO

-- Insert default settings
PRINT 'Inserting default system settings...';

-- Session Management Settings
IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'SessionTimeoutMinutes')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('SessionTimeoutMinutes', '30', 'Automatic logout after inactivity (in minutes)', 'Security', 'Integer', 1);
END

IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'SessionWarningMinutes')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('SessionWarningMinutes', '5', 'Show warning before auto-logout (in minutes)', 'Security', 'Integer', 1);
END

IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'MaxLoginAttempts')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('MaxLoginAttempts', '5', 'Maximum failed login attempts before lockout', 'Security', 'Integer', 1);
END

IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'LockoutDurationMinutes')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('LockoutDurationMinutes', '30', 'Account lockout duration (in minutes)', 'Security', 'Integer', 1);
END

IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'PasswordExpiryDays')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('PasswordExpiryDays', '90', 'Password expiration period (in days)', 'Security', 'Integer', 1);
END

IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'RequirePasswordChange')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('RequirePasswordChange', 'true', 'Require password change on first login', 'Security', 'Boolean', 1);
END

IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'AllowMultipleSessions')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('AllowMultipleSessions', 'false', 'Allow users to login from multiple devices', 'Security', 'Boolean', 1);
END

-- Application Settings
IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'ApplicationName')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('ApplicationName', 'ITAMS', 'Application display name', 'General', 'String', 1);
END

IF NOT EXISTS (SELECT * FROM SystemSettings WHERE SettingKey = 'MaintenanceMode')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable)
    VALUES ('MaintenanceMode', 'false', 'Enable maintenance mode', 'General', 'Boolean', 1);
END

PRINT 'Default system settings inserted successfully.';
GO

-- Display current settings
PRINT 'Current System Settings:';
SELECT 
    SettingKey,
    SettingValue,
    Description,
    Category,
    DataType
FROM SystemSettings
ORDER BY Category, SettingKey;
GO

PRINT 'Migration completed successfully.';
