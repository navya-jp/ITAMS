-- Migration: Create Asset Master Fields and Category/Type Tables
-- Date: 2026-02-23
-- Description: Creates dynamic field system and asset classification hierarchy

-- =============================================
-- 1. Create AssetMasterFields Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetMasterFields]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AssetMasterFields] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [FieldName] NVARCHAR(100) NOT NULL,
        [FieldCode] NVARCHAR(50) NOT NULL,
        [DataType] NVARCHAR(50) NOT NULL DEFAULT 'Text',
        [Description] NVARCHAR(200) NULL,
        [IsRequired] BIT NOT NULL DEFAULT 0,
        [DefaultValue] NVARCHAR(500) NULL,
        [ValidationRules] NVARCHAR(1000) NULL,
        [MaxLength] INT NULL,
        [MinValue] DECIMAL(18,2) NULL,
        [MaxValue] DECIMAL(18,2) NULL,
        [RegexPattern] NVARCHAR(500) NULL,
        [ValidationMessage] NVARCHAR(200) NULL,
        [DropdownOptions] NVARCHAR(1000) NULL,
        [FieldGroup] NVARCHAR(100) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsSystemField] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_AssetMasterFields] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_AssetMasterFields_FieldCode] UNIQUE ([FieldCode]),
        CONSTRAINT [UK_AssetMasterFields_FieldName] UNIQUE ([FieldName])
    );
    
    CREATE INDEX [IX_AssetMasterFields_IsActive] ON [dbo].[AssetMasterFields] ([IsActive]);
    CREATE INDEX [IX_AssetMasterFields_FieldGroup] ON [dbo].[AssetMasterFields] ([FieldGroup]);
    CREATE INDEX [IX_AssetMasterFields_DisplayOrder] ON [dbo].[AssetMasterFields] ([DisplayOrder]);
    
    PRINT 'Created AssetMasterFields table';
END
GO

-- =============================================
-- 2. Create AssetCategories Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetCategories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AssetCategories] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [CategoryName] NVARCHAR(100) NOT NULL,
        [CategoryCode] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [Icon] NVARCHAR(50) NULL,
        [ColorCode] NVARCHAR(7) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsPredefined] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_AssetCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_AssetCategories_CategoryCode] UNIQUE ([CategoryCode]),
        CONSTRAINT [UK_AssetCategories_CategoryName] UNIQUE ([CategoryName])
    );
    
    CREATE INDEX [IX_AssetCategories_IsActive] ON [dbo].[AssetCategories] ([IsActive]);
    CREATE INDEX [IX_AssetCategories_DisplayOrder] ON [dbo].[AssetCategories] ([DisplayOrder]);
    
    PRINT 'Created AssetCategories table';
END
GO

-- =============================================
-- 3. Create AssetTypes Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetTypes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AssetTypes] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [CategoryId] INT NOT NULL,
        [TypeName] NVARCHAR(100) NOT NULL,
        [TypeCode] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_AssetTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_AssetTypes_TypeCode] UNIQUE ([TypeCode]),
        CONSTRAINT [FK_AssetTypes_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[AssetCategories]([Id])
    );
    
    CREATE INDEX [IX_AssetTypes_CategoryId] ON [dbo].[AssetTypes] ([CategoryId]);
    CREATE INDEX [IX_AssetTypes_IsActive] ON [dbo].[AssetTypes] ([IsActive]);
    CREATE INDEX [IX_AssetTypes_DisplayOrder] ON [dbo].[AssetTypes] ([DisplayOrder]);
    
    PRINT 'Created AssetTypes table';
END
GO

-- =============================================
-- 4. Create AssetSubTypes Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssetSubTypes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AssetSubTypes] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [TypeId] INT NOT NULL,
        [SubTypeName] NVARCHAR(100) NOT NULL,
        [SubTypeCode] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_AssetSubTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_AssetSubTypes_SubTypeCode] UNIQUE ([SubTypeCode]),
        CONSTRAINT [FK_AssetSubTypes_Types] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[AssetTypes]([Id])
    );
    
    CREATE INDEX [IX_AssetSubTypes_TypeId] ON [dbo].[AssetSubTypes] ([TypeId]);
    CREATE INDEX [IX_AssetSubTypes_IsActive] ON [dbo].[AssetSubTypes] ([IsActive]);
    CREATE INDEX [IX_AssetSubTypes_DisplayOrder] ON [dbo].[AssetSubTypes] ([DisplayOrder]);
    
    PRINT 'Created AssetSubTypes table';
END
GO

-- =============================================
-- 5. Create TypeFieldMappings Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TypeFieldMappings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TypeFieldMappings] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [AssetTypeId] INT NOT NULL,
        [FieldId] INT NOT NULL,
        [IsRequired] BIT NOT NULL DEFAULT 0,
        [DefaultValue] NVARCHAR(500) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsVisible] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        CONSTRAINT [PK_TypeFieldMappings] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_TypeFieldMappings_TypeField] UNIQUE ([AssetTypeId], [FieldId]),
        CONSTRAINT [FK_TypeFieldMappings_AssetTypes] FOREIGN KEY ([AssetTypeId]) REFERENCES [dbo].[AssetTypes]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TypeFieldMappings_Fields] FOREIGN KEY ([FieldId]) REFERENCES [dbo].[AssetMasterFields]([Id])
    );
    
    CREATE INDEX [IX_TypeFieldMappings_AssetTypeId] ON [dbo].[TypeFieldMappings] ([AssetTypeId]);
    CREATE INDEX [IX_TypeFieldMappings_FieldId] ON [dbo].[TypeFieldMappings] ([FieldId]);
    CREATE INDEX [IX_TypeFieldMappings_DisplayOrder] ON [dbo].[TypeFieldMappings] ([DisplayOrder]);
    
    PRINT 'Created TypeFieldMappings table';
END
GO

-- =============================================
-- 6. Insert Predefined Asset Categories
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[AssetCategories] WHERE [CategoryCode] = 'HARDWARE')
BEGIN
    INSERT INTO [dbo].[AssetCategories] ([CategoryName], [CategoryCode], [Description], [Icon], [ColorCode], [DisplayOrder], [IsPredefined], [CreatedBy])
    VALUES 
        ('Hardware', 'HARDWARE', 'Physical hardware assets', 'computer', '#007bff', 1, 1, 1),
        ('Software', 'SOFTWARE', 'Software licenses and applications', 'code', '#28a745', 2, 1, 1),
        ('Digital Assets', 'DIGITAL', 'Digital assets like domains and certificates', 'cloud', '#17a2b8', 3, 1, 1);
    
    PRINT 'Inserted predefined asset categories';
END
GO

-- =============================================
-- 7. Insert Common Asset Master Fields
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[AssetMasterFields] WHERE [FieldCode] = 'ASSET_TAG')
BEGIN
    INSERT INTO [dbo].[AssetMasterFields] 
        ([FieldName], [FieldCode], [DataType], [Description], [IsRequired], [MaxLength], [FieldGroup], [DisplayOrder], [IsSystemField], [CreatedBy])
    VALUES 
        -- Basic Information
        ('Asset Tag', 'ASSET_TAG', 'Text', 'Unique asset identification tag', 1, 50, 'Basic Information', 1, 1, 1),
        ('Serial Number', 'SERIAL_NUMBER', 'Text', 'Manufacturer serial number', 0, 100, 'Basic Information', 2, 1, 1),
        ('Make', 'MAKE', 'Text', 'Manufacturer or brand', 1, 100, 'Basic Information', 3, 1, 1),
        ('Model', 'MODEL', 'Text', 'Model name or number', 1, 100, 'Basic Information', 4, 1, 1),
        
        -- Procurement Details
        ('Procurement Date', 'PROCUREMENT_DATE', 'Date', 'Date of purchase', 0, NULL, 'Procurement Details', 10, 1, 1),
        ('Procurement Cost', 'PROCUREMENT_COST', 'Number', 'Purchase cost', 0, NULL, 'Procurement Details', 11, 1, 1),
        ('Vendor', 'VENDOR', 'Text', 'Supplier or vendor name', 0, 200, 'Procurement Details', 12, 1, 1),
        ('Invoice Number', 'INVOICE_NUMBER', 'Text', 'Purchase invoice number', 0, 100, 'Procurement Details', 13, 0, 1),
        
        -- Warranty Information
        ('Warranty Start Date', 'WARRANTY_START', 'Date', 'Warranty start date', 0, NULL, 'Warranty Information', 20, 1, 1),
        ('Warranty End Date', 'WARRANTY_END', 'Date', 'Warranty expiry date', 0, NULL, 'Warranty Information', 21, 1, 1),
        ('Warranty Period (Months)', 'WARRANTY_PERIOD', 'Number', 'Warranty duration in months', 0, NULL, 'Warranty Information', 22, 0, 1),
        
        -- Hardware Specifications
        ('CPU', 'CPU', 'Text', 'Processor specification', 0, 200, 'Hardware Specifications', 30, 0, 1),
        ('RAM (GB)', 'RAM', 'Number', 'Memory in GB', 0, NULL, 'Hardware Specifications', 31, 0, 1),
        ('Storage (GB)', 'STORAGE', 'Number', 'Storage capacity in GB', 0, NULL, 'Hardware Specifications', 32, 0, 1),
        ('MAC Address', 'MAC_ADDRESS', 'Text', 'Network MAC address', 0, 50, 'Hardware Specifications', 33, 0, 1),
        ('IP Address', 'IP_ADDRESS', 'Text', 'Network IP address', 0, 50, 'Hardware Specifications', 34, 0, 1),
        ('Operating System', 'OS', 'Text', 'Installed operating system', 0, 100, 'Hardware Specifications', 35, 0, 1),
        ('Hostname', 'HOSTNAME', 'Text', 'Computer hostname', 0, 100, 'Hardware Specifications', 36, 0, 1),
        
        -- Software License Information
        ('License Type', 'LICENSE_TYPE', 'Dropdown', 'Type of software license', 0, NULL, 'License Information', 40, 0, 1),
        ('License Key', 'LICENSE_KEY', 'Text', 'Software license key (encrypted)', 0, 500, 'License Information', 41, 0, 1),
        ('Licenses Purchased', 'LICENSES_PURCHASED', 'Number', 'Number of licenses purchased', 0, NULL, 'License Information', 42, 0, 1),
        ('Licenses Consumed', 'LICENSES_CONSUMED', 'Number', 'Number of licenses in use', 0, NULL, 'License Information', 43, 0, 1),
        ('Subscription Start', 'SUBSCRIPTION_START', 'Date', 'Subscription start date', 0, NULL, 'License Information', 44, 0, 1),
        ('Subscription End', 'SUBSCRIPTION_END', 'Date', 'Subscription expiry date', 0, NULL, 'License Information', 45, 0, 1),
        
        -- Digital Asset Information
        ('Domain Name', 'DOMAIN_NAME', 'Text', 'Domain name', 0, 200, 'Digital Asset Information', 50, 0, 1),
        ('SSL Certificate Expiry', 'SSL_EXPIRY', 'Date', 'SSL certificate expiration date', 0, NULL, 'Digital Asset Information', 51, 0, 1),
        ('Registrar', 'REGISTRAR', 'Text', 'Domain registrar', 0, 200, 'Digital Asset Information', 52, 0, 1);
    
    PRINT 'Inserted common asset master fields';
END
GO

PRINT 'Asset Master Fields and Category/Type Tables migration completed successfully';
