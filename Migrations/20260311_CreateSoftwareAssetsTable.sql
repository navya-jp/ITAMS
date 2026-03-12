-- ============================================================================
-- ITAMS Database - Create SoftwareAssets Table
-- Date: March 11, 2026
-- Purpose: Support Software Asset management alongside Hardware Assets
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SoftwareAssets')
BEGIN
    CREATE TABLE SoftwareAssets (
        Id INT PRIMARY KEY IDENTITY(1,1),
        SoftwareName NVARCHAR(200) NOT NULL,
        Version NVARCHAR(50) NOT NULL,
        LicenseKey NVARCHAR(500) NOT NULL,
        LicenseType NVARCHAR(50) NOT NULL, -- Subscription, Open Source, Per User, Per Core, Per Device
        NumberOfLicenses INT NOT NULL,
        PurchaseDate DATETIME2 NOT NULL,
        ValidityStartDate DATETIME2 NOT NULL,
        ValidityEndDate DATETIME2 NOT NULL,
        ValidityType NVARCHAR(50) NOT NULL, -- Renewable, Perennial
        Vendor NVARCHAR(100) NOT NULL,
        Publisher NVARCHAR(100) NOT NULL,
        AssetTag NVARCHAR(50) NOT NULL UNIQUE,
        Status NVARCHAR(50) NOT NULL, -- Active, Expired, Available
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NOT NULL,
        UpdatedAt DATETIME2,
        UpdatedBy INT,
        
        -- Indexes
        INDEX IX_SoftwareAssets_AssetTag (AssetTag),
        INDEX IX_SoftwareAssets_Status (Status),
        INDEX IX_SoftwareAssets_ValidityEndDate (ValidityEndDate),
        INDEX IX_SoftwareAssets_CreatedAt (CreatedAt)
    );
    
    PRINT 'SoftwareAssets table created successfully'
END
ELSE
BEGIN
    PRINT 'SoftwareAssets table already exists'
END
