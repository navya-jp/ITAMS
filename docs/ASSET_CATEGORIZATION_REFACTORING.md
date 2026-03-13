# Asset Categorization Refactoring - Complete Implementation

## Overview
The ITAMS system has been refactored to support three asset categories instead of two:
- **Hardware** - Physical devices (servers, workstations, switches, routers, etc.)
- **Licensing** - Digital entitlements (software licenses, SSL certificates, application licenses)
- **Services** - Operational contracts and recurring services (AMC, leased lines, maintenance)

## Database Changes

### New Tables Created

#### 1. ServiceTypes
Master data table for service types
```sql
CREATE TABLE ServiceTypes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIME2,
    UpdatedBy INT
);
```

**Seeded Values:**
- AMC (Annual Maintenance Contract)
- LeasedLine (Internet leased lines and network leased services)
- Maintenance (Maintenance contracts and support services)

#### 2. ContractTypes
Master data table for contract types
```sql
CREATE TABLE ContractTypes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NOT NULL,
    UpdatedAt DATETIME2,
    UpdatedBy INT
);
```

**Seeded Values:**
- Comprehensive AMC
- Non-Comprehensive AMC
- Breakdown Visits Only
- Maintenance + Breakdown Visits

#### 3. ServiceAssets
New table for service assets with renewal tracking
```sql
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
```

### Table Modifications

#### 1. SoftwareAssets → LicensingAssets
- Table renamed from `SoftwareAssets` to `LicensingAssets`
- Column `SoftwareName` renamed to `LicenseName`
- AssetId prefix changed from `ASTS` to `ASTL`
- All existing data migrated automatically

#### 2. Assets Table
- Added `AssetCategoryId` column (FK to AssetCategories)
- All existing assets assigned to "Hardware" category
- New foreign key constraint: `FK_Assets_AssetCategory`

## Entity Model Changes

### New Entities

#### 1. LicensingAsset (renamed from SoftwareAsset)
```csharp
public class LicensingAsset
{
    public int Id { get; set; }
    public string AssetId { get; set; } // ASTL00001, ASTL00002, etc.
    public string LicenseName { get; set; }
    public string Version { get; set; }
    public string LicenseKey { get; set; }
    public string LicenseType { get; set; }
    public int NumberOfLicenses { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ValidityStartDate { get; set; }
    public DateTime ValidityEndDate { get; set; }
    public string ValidityType { get; set; }
    public string Vendor { get; set; }
    public string Publisher { get; set; }
    public string AssetTag { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
```

#### 2. ServiceAsset (new)
```csharp
public class ServiceAsset
{
    public int Id { get; set; }
    public string AssetId { get; set; } // ASTV00001, ASTV00002, etc.
    public string ServiceName { get; set; }
    public string Description { get; set; }
    public int ServiceTypeId { get; set; }
    public int ContractTypeId { get; set; }
    public string Vendor { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RenewalCycle { get; set; }
    public int RenewalReminderDays { get; set; }
    public DateTime? LastRenewalDate { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public string Status { get; set; }
    public decimal? ContractValue { get; set; }
    public string? ContractNumber { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual ServiceType? ServiceType { get; set; }
    public virtual ContractType? ContractType { get; set; }
}
```

#### 3. ServiceType (new master data)
```csharp
public class ServiceType
{
    public int Id { get; set; }
    public string TypeName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
```

#### 4. ContractType (new master data)
```csharp
public class ContractType
{
    public int Id { get; set; }
    public string TypeName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
```

### Modified Entities

#### Asset Entity
```csharp
public class Asset
{
    // ... existing fields ...
    
    // NEW: Asset Category (Hardware, Licensing, Services)
    public int? AssetCategoryId { get; set; }
    
    // NEW: Navigation property
    public virtual MasterData.AssetCategory? Category { get; set; }
}
```

## API Changes

### Controllers

#### 1. LicensingAssetsController (renamed from SoftwareAssetsController)
- Route: `/api/licensingassets`
- Endpoints:
  - `GET /api/licensingassets` - Get all licensing assets
  - `GET /api/licensingassets/{id}` - Get specific licensing asset
  - `POST /api/licensingassets` - Create new licensing asset
  - `PUT /api/licensingassets/{id}` - Update licensing asset
  - `DELETE /api/licensingassets/{id}` - Delete licensing asset

#### 2. ServiceAssetsController (new)
- Route: `/api/serviceassets`
- Endpoints:
  - `GET /api/serviceassets` - Get all service assets
  - `GET /api/serviceassets/{id}` - Get specific service asset
  - `POST /api/serviceassets` - Create new service asset
  - `PUT /api/serviceassets/{id}` - Update service asset
  - `DELETE /api/serviceassets/{id}` - Delete service asset

### DTOs

#### LicensingAssetDtos (renamed from SoftwareAssetDtos)
```csharp
public class LicensingAssetDto { ... }
public class CreateLicensingAssetDto { ... }
public class UpdateLicensingAssetDto { ... }
```

#### ServiceAssetDtos (new)
```csharp
public class ServiceAssetDto { ... }
public class CreateServiceAssetDto { ... }
public class UpdateServiceAssetDto { ... }
```

## Asset ID Generation

### Updated AssetIdGeneratorService

**Prefixes:**
- Hardware: `ASTH` (e.g., ASTH00001, ASTH00002)
- Licensing: `ASTL` (e.g., ASTL00001, ASTL00002)
- Services: `ASTV` (e.g., ASTV00001, ASTV00002)

**Methods:**
- `GenerateHardwareAssetIdAsync()` - Generates ASTH prefix IDs
- `GenerateLicensingAssetIdAsync()` - Generates ASTL prefix IDs
- `GenerateServiceAssetIdAsync()` - Generates ASTV prefix IDs

## Database Context Updates

### ITAMSDbContext Changes
```csharp
// Renamed
public DbSet<LicensingAsset> LicensingAssets { get; set; }

// New
public DbSet<ServiceAsset> ServiceAssets { get; set; }
public DbSet<MasterData.ServiceType> ServiceTypes { get; set; }
public DbSet<MasterData.ContractType> ContractTypes { get; set; }
```

## Migrations Applied

### Migration: 20260313_RefactorAssetCategorization_v2.sql

**Changes:**
1. Created ServiceTypes table with master data
2. Created ContractTypes table with master data
3. Renamed SoftwareAssets to LicensingAssets
4. Renamed SoftwareName column to LicenseName
5. Updated AssetId prefixes from ASTS to ASTL
6. Created ServiceAssets table with all required fields
7. Added AssetCategoryId to Assets table
8. Updated all existing assets to Hardware category
9. Seeded master data for ServiceTypes and ContractTypes

## Backward Compatibility

- All existing hardware assets remain unchanged
- All existing licensing assets (formerly software) are automatically migrated
- AssetId prefixes updated automatically in database
- No data loss during migration
- Existing API clients need to update endpoints:
  - `/api/softwareassets` → `/api/licensingassets`

## Frontend Updates Required

The frontend needs to be updated to:
1. Display three asset categories: Hardware, Licensing, Services
2. Update API calls to use new endpoints
3. Create forms for service asset creation with:
   - Service type selection
   - Contract type selection
   - Renewal cycle and reminder configuration
   - Start/end date tracking
4. Update asset ID display to show correct prefixes

## Normalization Structure Maintained

The refactoring maintains the normalized structure:
- Master data tables for ServiceType and ContractType
- Foreign key relationships properly established
- Audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) on all tables
- Proper indexing on frequently queried columns
- Consistent naming conventions across all entities

## Future Enhancements

1. Renewal reminder notifications for service assets
2. Service renewal workflow automation
3. Contract value tracking and reporting
4. Service asset lifecycle management
5. Integration with calendar/notification system for renewal dates
