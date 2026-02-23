-- Migration: Create Master Data Tables for Vendors, Asset Status, and Criticality Levels
-- Date: 2026-02-23
-- Description: Creates configurable master data tables to replace hardcoded enums

-- =============================================
-- 1. Create Vendors Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vendors]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Vendors] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [VendorName] NVARCHAR(100) NOT NULL,
        [VendorCode] NVARCHAR(50) NOT NULL,
        [ContactPerson] NVARCHAR(100) NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [Address] NVARCHAR(500) NULL,
        [Website] NVARCHAR(200) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_Vendors] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_Vendors_VendorCode] UNIQUE ([VendorCode]),
        CONSTRAINT [UK_Vendors_VendorName] UNIQUE ([VendorName])
    );
    
    CREATE INDEX [IX_Vendors_IsActive] ON [dbo].[Vendors] ([IsActive]);
    CREATE INDEX [IX_Vendors_IsDeleted] ON [dbo].[Vendors] ([IsDeleted]);
    
    PRINT 'Created Vendors table';
END
GO

-- =============================================
-- 2. Create AssetStatuses Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetStatuses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AssetStatuses] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [StatusName] NVARCHAR(50) NOT NULL,
        [StatusCode] NVARCHAR(20) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [ColorCode] NVARCHAR(7) NOT NULL DEFAULT '#808080',
        [Icon] NVARCHAR(50) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsPredefined] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_AssetStatuses] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_AssetStatuses_StatusCode] UNIQUE ([StatusCode]),
        CONSTRAINT [UK_AssetStatuses_StatusName] UNIQUE ([StatusName])
    );
    
    CREATE INDEX [IX_AssetStatuses_IsActive] ON [dbo].[AssetStatuses] ([IsActive]);
    
    PRINT 'Created AssetStatuses table';
END
GO

-- =============================================
-- 3. Create CriticalityLevels Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CriticalityLevels]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CriticalityLevels] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [LevelName] NVARCHAR(50) NOT NULL,
        [LevelCode] NVARCHAR(20) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [PriorityOrder] INT NOT NULL,
        [SlaHours] INT NOT NULL,
        [PriorityLevel] NVARCHAR(20) NOT NULL DEFAULT 'Medium',
        [NotificationThresholdDays] INT NOT NULL DEFAULT 30,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsPredefined] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CriticalityLevels] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_CriticalityLevels_LevelCode] UNIQUE ([LevelCode]),
        CONSTRAINT [UK_CriticalityLevels_LevelName] UNIQUE ([LevelName]),
        CONSTRAINT [UK_CriticalityLevels_PriorityOrder] UNIQUE ([PriorityOrder])
    );
    
    CREATE INDEX [IX_CriticalityLevels_IsActive] ON [dbo].[CriticalityLevels] ([IsActive]);
    CREATE INDEX [IX_CriticalityLevels_PriorityOrder] ON [dbo].[CriticalityLevels] ([PriorityOrder]);
    
    PRINT 'Created CriticalityLevels table';
END
GO

-- =============================================
-- 4. Insert Predefined Asset Statuses
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[AssetStatuses] WHERE [StatusCode] = 'IN_USE')
BEGIN
    INSERT INTO [dbo].[AssetStatuses] ([StatusName], [StatusCode], [Description], [ColorCode], [IsPredefined], [CreatedBy])
    VALUES 
        ('In Use', 'IN_USE', 'Asset is currently in use', '#28a745', 1, 1),
        ('Spare', 'SPARE', 'Asset is available as spare', '#17a2b8', 1, 1),
        ('Repair', 'REPAIR', 'Asset is under repair', '#ffc107', 1, 1),
        ('Decommissioned', 'DECOMMISSIONED', 'Asset has been decommissioned', '#dc3545', 1, 1);
    
    PRINT 'Inserted predefined asset statuses';
END
GO

-- =============================================
-- 5. Insert Predefined Criticality Levels
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[CriticalityLevels] WHERE [LevelCode] = 'TMS_CRITICAL')
BEGIN
    INSERT INTO [dbo].[CriticalityLevels] 
        ([LevelName], [LevelCode], [Description], [PriorityOrder], [SlaHours], [PriorityLevel], [NotificationThresholdDays], [IsPredefined], [CreatedBy])
    VALUES 
        ('TMS-Critical', 'TMS_CRITICAL', 'Critical TMS infrastructure assets', 1, 4, 'High', 90, 1, 1),
        ('TMS-General', 'TMS_GENERAL', 'General TMS assets', 2, 24, 'Medium', 60, 1, 1),
        ('IT-Critical', 'IT_CRITICAL', 'Critical IT infrastructure assets', 3, 8, 'High', 90, 1, 1),
        ('IT-General', 'IT_GENERAL', 'General IT assets', 4, 48, 'Low', 30, 1, 1);
    
    PRINT 'Inserted predefined criticality levels';
END
GO

PRINT 'Master Data Tables migration completed successfully';
