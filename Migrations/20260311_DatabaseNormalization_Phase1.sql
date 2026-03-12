-- ============================================================================
-- ITAMS Database Normalization - Phase 1
-- Date: March 11, 2026
-- Purpose: Normalize Asset table and related entities to use FK references
--          instead of storing repeated text values
-- ============================================================================

-- Step 1: Create new lookup tables
-- ============================================================================

-- AssetPlacing table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AssetPlacings')
BEGIN
    CREATE TABLE AssetPlacings (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PlacingId NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    -- Seed data
    INSERT INTO AssetPlacings (PlacingId, Name, IsActive, CreatedAt)
    VALUES 
        ('PLC001', 'Lane Area', 1, GETUTCDATE()),
        ('PLC002', 'Booth Area', 1, GETUTCDATE()),
        ('PLC003', 'Plaza Area', 1, GETUTCDATE()),
        ('PLC004', 'Server Room', 1, GETUTCDATE()),
        ('PLC005', 'Control Room', 1, GETUTCDATE()),
        ('PLC006', 'Admin Building', 1, GETUTCDATE());
END

-- PatchStatus table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PatchStatuses')
BEGIN
    CREATE TABLE PatchStatuses (
        Id INT PRIMARY KEY IDENTITY(1,1),
        StatusId NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

-- USBBlockingStatus table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'USBBlockingStatuses')
BEGIN
    CREATE TABLE USBBlockingStatuses (
        Id INT PRIMARY KEY IDENTITY(1,1),
        StatusId NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

-- AssetClassification table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AssetClassifications')
BEGIN
    CREATE TABLE AssetClassifications (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ClassificationId NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

-- OperatingSystem table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OperatingSystems')
BEGIN
    CREATE TABLE OperatingSystems (
        Id INT PRIMARY KEY IDENTITY(1,1),
        OSId NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

-- DatabaseType table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DatabaseTypes')
BEGIN
    CREATE TABLE DatabaseTypes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        DBTypeId NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

-- SessionStatus table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SessionStatuses')
BEGIN
    CREATE TABLE SessionStatuses (
        Id INT PRIMARY KEY IDENTITY(1,1),
        StatusId NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    -- Seed data
    INSERT INTO SessionStatuses (StatusId, Name, IsActive, CreatedAt)
    VALUES 
        ('SS001', 'ACTIVE', 1, GETUTCDATE()),
        ('SS002', 'LOGGED_OUT', 1, GETUTCDATE()),
        ('SS003', 'SESSION_TIMEOUT', 1, GETUTCDATE()),
        ('SS004', 'FORCED_LOGOUT', 1, GETUTCDATE());
END

-- Step 2: Add new FK columns to Assets table
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'AssetTypeId')
BEGIN
    ALTER TABLE Assets ADD AssetTypeId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'AssetSubTypeId')
BEGIN
    ALTER TABLE Assets ADD AssetSubTypeId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'AssetStatusId')
BEGIN
    ALTER TABLE Assets ADD AssetStatusId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'AssetClassificationId')
BEGIN
    ALTER TABLE Assets ADD AssetClassificationId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'OperatingSystemId')
BEGIN
    ALTER TABLE Assets ADD OperatingSystemId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'DatabaseTypeId')
BEGIN
    ALTER TABLE Assets ADD DatabaseTypeId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'PatchStatusId')
BEGIN
    ALTER TABLE Assets ADD PatchStatusId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'USBBlockingStatusId')
BEGIN
    ALTER TABLE Assets ADD USBBlockingStatusId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'AssetPlacingId')
BEGIN
    ALTER TABLE Assets ADD AssetPlacingId INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'VendorId')
BEGIN
    ALTER TABLE Assets ADD VendorId INT NULL;
END

-- Step 3: Add SessionStatusId to LoginAudit table
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoginAudit') AND name = 'SessionStatusId')
BEGIN
    ALTER TABLE LoginAudit ADD SessionStatusId INT NULL;
END

-- Step 4: Remove duplicate UserName from AuditEntry table
-- ============================================================================

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AuditEntries') AND name = 'UserName')
BEGIN
    ALTER TABLE AuditEntries DROP COLUMN UserName;
END

-- Step 5: Remove duplicate Username from LoginAudit table
-- ============================================================================

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoginAudit') AND name = 'Username')
BEGIN
    ALTER TABLE LoginAudit DROP COLUMN Username;
END

-- Step 6: Add FK constraints
-- ============================================================================

-- Asset FKs
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetTypes')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_AssetTypes
    FOREIGN KEY (AssetTypeId) REFERENCES AssetTypes(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetSubTypes')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_AssetSubTypes
    FOREIGN KEY (AssetSubTypeId) REFERENCES AssetSubTypes(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetStatuses')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_AssetStatuses
    FOREIGN KEY (AssetStatusId) REFERENCES AssetStatuses(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetClassifications')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_AssetClassifications
    FOREIGN KEY (AssetClassificationId) REFERENCES AssetClassifications(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_OperatingSystems')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_OperatingSystems
    FOREIGN KEY (OperatingSystemId) REFERENCES OperatingSystems(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_DatabaseTypes')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_DatabaseTypes
    FOREIGN KEY (DatabaseTypeId) REFERENCES DatabaseTypes(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_PatchStatuses')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_PatchStatuses
    FOREIGN KEY (PatchStatusId) REFERENCES PatchStatuses(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_USBBlockingStatuses')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_USBBlockingStatuses
    FOREIGN KEY (USBBlockingStatusId) REFERENCES USBBlockingStatuses(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetPlacings')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_AssetPlacings
    FOREIGN KEY (AssetPlacingId) REFERENCES AssetPlacings(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_Vendors')
BEGIN
    ALTER TABLE Assets
    ADD CONSTRAINT FK_Assets_Vendors
    FOREIGN KEY (VendorId) REFERENCES Vendors(Id) ON DELETE SET NULL;
END

-- LoginAudit FKs
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_LoginAudit_SessionStatuses')
BEGIN
    ALTER TABLE LoginAudit
    ADD CONSTRAINT FK_LoginAudit_SessionStatuses
    FOREIGN KEY (SessionStatusId) REFERENCES SessionStatuses(Id) ON DELETE SET NULL;
END

-- Step 7: Create indexes for better query performance
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_AssetTypeId')
BEGIN
    CREATE INDEX IX_Assets_AssetTypeId ON Assets(AssetTypeId);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_AssetStatusId')
BEGIN
    CREATE INDEX IX_Assets_AssetStatusId ON Assets(AssetStatusId);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_AssetPlacingId')
BEGIN
    CREATE INDEX IX_Assets_AssetPlacingId ON Assets(AssetPlacingId);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LoginAudit_SessionStatusId')
BEGIN
    CREATE INDEX IX_LoginAudit_SessionStatusId ON LoginAudit(SessionStatusId);
END

-- ============================================================================
-- Migration Complete
-- ============================================================================
-- Note: Old text columns (AssetType, Status, Placing, etc.) are kept for now
-- for backward compatibility. They will be removed in Phase 2 after data
-- validation and migration to the new FK-based structure.
-- ============================================================================
