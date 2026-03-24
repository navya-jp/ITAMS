-- Migration: Create SystemAlerts table
-- Run this on ITAMS_Shared database

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemAlerts')
BEGIN
    CREATE TABLE SystemAlerts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AlertType NVARCHAR(50) NOT NULL,
        Severity NVARCHAR(50) NOT NULL DEFAULT 'Medium',
        Title NVARCHAR(500) NOT NULL,
        Message NVARCHAR(2000) NULL,
        AssetId INT NULL,
        LicensingAssetId INT NULL,
        ServiceAssetId INT NULL,
        EntityType NVARCHAR(100) NULL,
        EntityIdentifier NVARCHAR(100) NULL,
        IsAcknowledged BIT NOT NULL DEFAULT 0,
        AcknowledgedBy INT NULL,
        AcknowledgedAt DATETIME2 NULL,
        EmailSent BIT NOT NULL DEFAULT 0,
        EmailSentAt DATETIME2 NULL,
        EscalationLevel INT NOT NULL DEFAULT 1,
        LastEscalatedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ResolvedAt DATETIME2 NULL,
        IsResolved BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_SystemAlerts_Assets FOREIGN KEY (AssetId) REFERENCES Assets(Id) ON DELETE SET NULL,
        CONSTRAINT FK_SystemAlerts_LicensingAssets FOREIGN KEY (LicensingAssetId) REFERENCES LicensingAssets(Id) ON DELETE SET NULL,
        CONSTRAINT FK_SystemAlerts_ServiceAssets FOREIGN KEY (ServiceAssetId) REFERENCES ServiceAssets(Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_SystemAlerts_AlertType ON SystemAlerts(AlertType);
    CREATE INDEX IX_SystemAlerts_IsResolved ON SystemAlerts(IsResolved);
    CREATE INDEX IX_SystemAlerts_IsAcknowledged ON SystemAlerts(IsAcknowledged);
    CREATE INDEX IX_SystemAlerts_CreatedAt ON SystemAlerts(CreatedAt);

    PRINT 'SystemAlerts table created successfully.';
END
ELSE
BEGIN
    PRINT 'SystemAlerts table already exists.';
END

-- Seed SMTP settings into SystemSettings (if not already present)
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'Email_SmtpHost')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category, DataType, IsEditable, CreatedAt)
    VALUES
        ('Email_SmtpHost',     'smtp.gmail.com',  'SMTP server hostname',          'Email', 'String',  1, GETUTCDATE()),
        ('Email_SmtpPort',     '587',             'SMTP server port',              'Email', 'Integer', 1, GETUTCDATE()),
        ('Email_SmtpUser',     '',                'SMTP username / email address', 'Email', 'String',  1, GETUTCDATE()),
        ('Email_SmtpPassword', '',                'SMTP password or app password', 'Email', 'String',  1, GETUTCDATE()),
        ('Email_FromAddress',  '',                'From email address',            'Email', 'String',  1, GETUTCDATE()),
        ('Email_FromName',     'ITAMS Alerts',    'From display name',             'Email', 'String',  1, GETUTCDATE());

    PRINT 'Email settings seeded into SystemSettings.';
END
