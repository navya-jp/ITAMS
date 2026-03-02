-- Drop and recreate Assets table to start fresh
-- WARNING: This will delete all existing asset data!

-- Drop the table if it exists
IF OBJECT_ID('dbo.Assets', 'U') IS NOT NULL
    DROP TABLE dbo.Assets;
GO

-- Recreate the Assets table
CREATE TABLE dbo.Assets (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AssetId NVARCHAR(50) NOT NULL,
    AssetTag NVARCHAR(50) NOT NULL,
    AssetType NVARCHAR(100),
    Make NVARCHAR(100),
    Model NVARCHAR(100),
    SerialNumber NVARCHAR(100),
    Status INT NOT NULL DEFAULT 0,
    Criticality INT NOT NULL DEFAULT 0,
    UsageCategory INT NOT NULL DEFAULT 0,
    Classification NVARCHAR(100),
    SubType NVARCHAR(100),
    IPAddress NVARCHAR(50),
    Region NVARCHAR(100),
    State NVARCHAR(100),
    Site NVARCHAR(200),
    LocationText NVARCHAR(200),
    PlazaName NVARCHAR(200),
    Department NVARCHAR(100),
    AssignedUserId INT NULL,
    AssignedUserText NVARCHAR(200),
    AssignedUserRole NVARCHAR(100),
    UserRole NVARCHAR(100),
    ProcuredBy NVARCHAR(200),
    ProcurementCost DECIMAL(18,2) NULL,
    ProcurementDate DATETIME2 NULL,
    CommissioningDate DATETIME2 NULL,
    WarrantyStartDate DATETIME2 NULL,
    WarrantyEndDate DATETIME2 NULL,
    Vendor NVARCHAR(200),
    OSType NVARCHAR(100),
    OSVersion NVARCHAR(100),
    DBType NVARCHAR(100),
    DBVersion NVARCHAR(100),
    PatchStatus NVARCHAR(100),
    USBBlockingStatus NVARCHAR(100),
    Remarks NVARCHAR(4000),
    ProjectId INT NOT NULL,
    ProjectIdRef NVARCHAR(50),
    LocationId INT NOT NULL,
    LocationIdRef NVARCHAR(50),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy INT NULL,
    
    -- Unique constraints
    CONSTRAINT UQ_Assets_AssetId UNIQUE (AssetId),
    CONSTRAINT IX_Assets_AssetTag UNIQUE (AssetTag)
);
GO

-- Verify table was created
SELECT COUNT(*) AS AssetCount FROM dbo.Assets;
GO
