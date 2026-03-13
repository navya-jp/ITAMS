-- ============================================================================
-- ASSET CATEGORIZATION REFACTORING MIGRATION V2
-- Refactors the asset system from Hardware/Software to Hardware/Licensing/Services
-- ============================================================================

-- Step 1: Create ServiceType table
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceTypes')
BEGIN
    CREATE TABLE ServiceTypes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        TypeName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NOT NULL,
        UpdatedAt DATETIME2,
        UpdatedBy INT
    );
    
    CREATE INDEX IX_ServiceTypes_TypeName ON ServiceTypes(TypeName);
    
    PRINT 'ServiceTypes table created';
END;

-- Step 2: Create ContractType table
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractTypes')
BEGIN
    CREATE TABLE ContractTypes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        TypeName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NOT NULL,
        UpdatedAt DATETIME2,
        UpdatedBy INT
    );
    
    CREATE INDEX IX_ContractTypes_TypeName ON ContractTypes(TypeName);
    
    PRINT 'ContractTypes table created';
END;

-- Step 3: Rename SoftwareAssets to LicensingAssets
-- ============================================================================

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SoftwareAssets')
BEGIN
    -- Rename the table
    EXEC sp_rename 'SoftwareAssets', 'LicensingAssets';
    
    -- Rename the column SoftwareName to LicenseName
    EXEC sp_rename 'LicensingAssets.SoftwareName', 'LicenseName', 'COLUMN';
    
    -- Update AssetId prefix from ASTS to ASTL
    UPDATE LicensingAssets
    SET AssetId = 'ASTL' + SUBSTRING(AssetId, 5, LEN(AssetId))
    WHERE AssetId LIKE 'ASTS%';
    
    PRINT 'SoftwareAssets renamed to LicensingAssets';
END;

-- Step 4: Create ServiceAssets table
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceAssets')
BEGIN
    CREATE TABLE ServiceAssets (
        Id INT PRIMARY KEY IDENTITY(1,1),
        AssetId NVARCHAR(50) NOT NULL UNIQUE,
        ServiceName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        ServiceTypeId INT NOT NULL,
        ContractTypeId INT NOT NULL,
        Vendor NVARCHAR(100) NOT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NOT NULL,
        RenewalCycle NVARCHAR(50) NOT NULL,
        RenewalReminderDays INT NOT NULL,
        LastRenewalDate DATETIME2,
        NextRenewalDate DATETIME2,
        Status NVARCHAR(50) NOT NULL,
        ContractValue DECIMAL(18,2),
        ContractNumber NVARCHAR(500),
        Remarks NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NOT NULL,
        UpdatedAt DATETIME2,
        UpdatedBy INT,
        CONSTRAINT FK_ServiceAssets_ServiceType FOREIGN KEY (ServiceTypeId) REFERENCES ServiceTypes(Id),
        CONSTRAINT FK_ServiceAssets_ContractType FOREIGN KEY (ContractTypeId) REFERENCES ContractTypes(Id)
    );
    
    CREATE UNIQUE INDEX IX_ServiceAssets_AssetId ON ServiceAssets(AssetId);
    CREATE INDEX IX_ServiceAssets_ServiceTypeId ON ServiceAssets(ServiceTypeId);
    CREATE INDEX IX_ServiceAssets_ContractTypeId ON ServiceAssets(ContractTypeId);
    CREATE INDEX IX_ServiceAssets_Status ON ServiceAssets(Status);
    CREATE INDEX IX_ServiceAssets_EndDate ON ServiceAssets(EndDate);
    
    PRINT 'ServiceAssets table created';
END;

-- Step 5: Add AssetCategoryId to Assets table if not exists
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Assets' AND COLUMN_NAME = 'AssetCategoryId')
BEGIN
    ALTER TABLE Assets
    ADD AssetCategoryId INT NULL;
    
    -- Add foreign key constraint
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_AssetCategory FOREIGN KEY (AssetCategoryId) REFERENCES AssetCategories(Id);
    
    CREATE INDEX IX_Assets_AssetCategoryId ON Assets(AssetCategoryId);
    
    PRINT 'AssetCategoryId column added to Assets table';
END;

-- Step 6: Seed ServiceTypes
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM ServiceTypes WHERE TypeName = 'AMC')
BEGIN
    INSERT INTO ServiceTypes (TypeName, Description, CreatedBy)
    VALUES ('AMC', 'Annual Maintenance Contract', 1);
END;

IF NOT EXISTS (SELECT 1 FROM ServiceTypes WHERE TypeName = 'LeasedLine')
BEGIN
    INSERT INTO ServiceTypes (TypeName, Description, CreatedBy)
    VALUES ('LeasedLine', 'Internet leased lines and network leased services', 1);
END;

IF NOT EXISTS (SELECT 1 FROM ServiceTypes WHERE TypeName = 'Maintenance')
BEGIN
    INSERT INTO ServiceTypes (TypeName, Description, CreatedBy)
    VALUES ('Maintenance', 'Maintenance contracts and support services', 1);
END;

PRINT 'ServiceTypes seeded';

-- Step 7: Seed ContractTypes
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM ContractTypes WHERE TypeName = 'Comprehensive AMC')
BEGIN
    INSERT INTO ContractTypes (TypeName, Description, CreatedBy)
    VALUES ('Comprehensive AMC', 'Comprehensive Annual Maintenance Contract', 1);
END;

IF NOT EXISTS (SELECT 1 FROM ContractTypes WHERE TypeName = 'Non-Comprehensive AMC')
BEGIN
    INSERT INTO ContractTypes (TypeName, Description, CreatedBy)
    VALUES ('Non-Comprehensive AMC', 'Non-Comprehensive Annual Maintenance Contract', 1);
END;

IF NOT EXISTS (SELECT 1 FROM ContractTypes WHERE TypeName = 'Breakdown Visits Only')
BEGIN
    INSERT INTO ContractTypes (TypeName, Description, CreatedBy)
    VALUES ('Breakdown Visits Only', 'Breakdown visits only maintenance model', 1);
END;

IF NOT EXISTS (SELECT 1 FROM ContractTypes WHERE TypeName = 'Maintenance + Breakdown Visits')
BEGIN
    INSERT INTO ContractTypes (TypeName, Description, CreatedBy)
    VALUES ('Maintenance + Breakdown Visits', 'Maintenance with breakdown visits included', 1);
END;

PRINT 'ContractTypes seeded';

-- Step 8: Update existing Assets to have AssetCategoryId = 1 (Hardware)
-- ============================================================================

UPDATE Assets
SET AssetCategoryId = (SELECT Id FROM AssetCategories WHERE CategoryName = 'Hardware')
WHERE AssetCategoryId IS NULL;

PRINT 'Assets updated with Hardware category';

-- Step 9: Rename indexes for LicensingAssets
-- ============================================================================

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareAssets_AssetId')
BEGIN
    DROP INDEX IX_SoftwareAssets_AssetId ON LicensingAssets;
    CREATE UNIQUE INDEX IX_LicensingAssets_AssetId ON LicensingAssets(AssetId);
    PRINT 'Indexes renamed for LicensingAssets';
END;

PRINT 'Asset Categorization Refactoring Migration Completed Successfully!';
