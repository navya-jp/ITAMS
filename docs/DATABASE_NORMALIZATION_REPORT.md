# ITAMS Database Normalization Report

## Executive Summary

The ITAMS database contains significant denormalization issues that violate 3NF (Third Normal Form) principles. This report documents the issues, implementation strategy, and changes made to enforce proper relational database structure.

## Critical Issues Identified

### 1. Asset Table - Massive Text Field Duplication (CRITICAL)

**Problem:**
- Location data duplicated: `Region`, `State`, `Site`, `PlazaName`, `LocationText`, `Department`
- User assignment duplicated: `AssignedUserText`, `UserRole`, `AssignedUserRole`
- Asset type stored as text instead of FK: `AssetType`, `SubType`
- Status stored as enum instead of FK: `Status`
- Extended fields stored as text: `Classification`, `OSType`, `DBType`, `PatchStatus`, `USBBlockingStatus`, `Placing`

**Impact:**
- Data inconsistency when location/user/type names change
- Orphaned/stale data when related records are deleted
- Larger table size (bloated with redundant text)
- Inefficient queries (text comparisons vs. integer lookups)
- Violates 3NF - non-key attributes depend on non-key attributes

**Solution:**
- Create lookup tables for all repeated text values
- Replace text fields with FK references
- Add navigation properties for relationships

### 2. Audit Tables - Username Denormalization

**Problem:**
- `AuditEntry.UserName` duplicates `User.Username`
- `LoginAudit.Username` duplicates `User.Username`

**Impact:**
- Stale data when usernames change
- Violates 3NF

**Solution:**
- Keep only FK reference to User
- Query User table when display name needed

### 3. User Table - Location Restriction Text Fields

**Problem:**
- `RestrictedRegion`, `RestrictedState`, `RestrictedPlaza`, `RestrictedOffice` stored as text
- No FK constraints to validate values exist

**Impact:**
- Inconsistency when location names change
- No referential integrity

**Solution:**
- Create `UserLocationRestriction` junction table
- Reference Location hierarchy instead of text

### 4. Project.States - CSV Anti-Pattern

**Problem:**
- `Project.States` stored as comma-separated string

**Impact:**
- Difficult to query
- Cannot index efficiently
- No referential integrity

**Solution:**
- Create `ProjectState` junction table
- Normalize to proper many-to-many relationship

## Normalization Changes

### Phase 1: Create Lookup Tables (IMPLEMENTED)

New master data tables created:

| Table | Purpose | Fields |
|-------|---------|--------|
| `AssetPlacing` | Asset placement locations | Id, PlacingId (AK), Name, Description, IsActive, CreatedAt |
| `PatchStatus` | Patch status values | Id, StatusId (AK), Name, Description, IsActive, CreatedAt |
| `USBBlockingStatus` | USB blocking status | Id, StatusId (AK), Name, Description, IsActive, CreatedAt |
| `AssetClassification` | Asset classification levels | Id, ClassificationId (AK), Name, Description, IsActive, CreatedAt |
| `OperatingSystem` | OS types | Id, OSId (AK), Name, Description, IsActive, CreatedAt |
| `DatabaseType` | Database types | Id, DBTypeId (AK), Name, Description, IsActive, CreatedAt |
| `SessionStatus` | Login session status | Id, StatusId (AK), Name, Description, IsActive, CreatedAt |

### Phase 2: Update Asset Entity

**Removed Text Fields:**
- `AssetType` → `AssetTypeId` (FK)
- `SubType` → `AssetSubTypeId` (FK)
- `Status` → `AssetStatusId` (FK)
- `Classification` → `AssetClassificationId` (FK)
- `OSType` → `OperatingSystemId` (FK)
- `DBType` → `DatabaseTypeId` (FK)
- `PatchStatus` → `PatchStatusId` (FK)
- `USBBlockingStatus` → `USBBlockingStatusId` (FK)
- `Placing` → `AssetPlacingId` (FK)
- `AssignedUserRole` → Removed (query from User.Role)
- `AssignedUserText` → Removed (use AssignedUserId FK)
- `Vendor` → `VendorId` (FK to existing Vendor table)

**Kept for Display (from Excel import):**
- `Region`, `State`, `Site`, `PlazaName`, `LocationText`, `Department` - These are kept as read-only display fields from Excel import, not used for queries

**New FK Properties:**
```csharp
public int? AssetTypeId { get; set; }
public int? AssetSubTypeId { get; set; }
public int? AssetStatusId { get; set; }
public int? AssetClassificationId { get; set; }
public int? OperatingSystemId { get; set; }
public int? OSVersionId { get; set; }
public int? DatabaseTypeId { get; set; }
public int? DBVersionId { get; set; }
public int? PatchStatusId { get; set; }
public int? USBBlockingStatusId { get; set; }
public int? AssetPlacingId { get; set; }
public int? VendorId { get; set; }
```

### Phase 3: Update LoginAudit Entity

**Removed:**
- `Username` (duplicate of User.Username)

**Added:**
- `SessionStatusId` (FK to SessionStatus)

### Phase 4: Create Junction Tables

**ProjectState Table:**
```sql
CREATE TABLE ProjectStates (
    Id INT PRIMARY KEY IDENTITY,
    ProjectId INT NOT NULL,
    State NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    UNIQUE(ProjectId, State)
);
```

**UserLocationRestriction Table:**
```sql
CREATE TABLE UserLocationRestrictions (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    LocationId INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (LocationId) REFERENCES Locations(Id),
    UNIQUE(UserId, LocationId)
);
```

## Query Pattern Changes

### Before (Text-based - INCORRECT):
```csharp
// BAD: Text comparison
var assets = context.Assets
    .Where(a => a.AssetType == "Server" && a.Status == "InUse")
    .ToList();

// BAD: String parsing
var placing = asset.Placing.ToLower();
```

### After (ID-based - CORRECT):
```csharp
// GOOD: ID-based query
var serverTypeId = context.AssetTypes
    .Where(t => t.Name == "Server")
    .Select(t => t.Id)
    .FirstOrDefault();

var inUseStatusId = context.AssetStatuses
    .Where(s => s.Name == "In Use")
    .Select(s => s.Id)
    .FirstOrDefault();

var assets = context.Assets
    .Where(a => a.AssetTypeId == serverTypeId && a.AssetStatusId == inUseStatusId)
    .Include(a => a.AssetType)
    .Include(a => a.AssetStatus)
    .ToList();

// GOOD: Use navigation property
var assetType = asset.AssetType?.Name;
var status = asset.AssetStatus?.Name;
```

## Data Migration Strategy

### Step 1: Create Lookup Tables with Seed Data
```sql
INSERT INTO AssetPlacings (PlacingId, Name, IsActive, CreatedAt)
VALUES 
    ('PLC001', 'Lane Area', 1, GETUTCDATE()),
    ('PLC002', 'Booth Area', 1, GETUTCDATE()),
    ('PLC003', 'Plaza Area', 1, GETUTCDATE()),
    ('PLC004', 'Server Room', 1, GETUTCDATE()),
    ('PLC005', 'Control Room', 1, GETUTCDATE()),
    ('PLC006', 'Admin Building', 1, GETUTCDATE());
```

### Step 2: Migrate Data from Text to IDs
```sql
-- Migrate AssetType
UPDATE Assets
SET AssetTypeId = (
    SELECT Id FROM AssetTypes 
    WHERE Name = Assets.AssetType
)
WHERE AssetType IS NOT NULL;

-- Migrate Status
UPDATE Assets
SET AssetStatusId = (
    SELECT Id FROM AssetStatuses 
    WHERE Name = Assets.Status
)
WHERE Status IS NOT NULL;

-- Similar for other fields...
```

### Step 3: Add FK Constraints
```sql
ALTER TABLE Assets
ADD CONSTRAINT FK_Assets_AssetTypes 
    FOREIGN KEY (AssetTypeId) REFERENCES AssetTypes(Id);

ALTER TABLE Assets
ADD CONSTRAINT FK_Assets_AssetStatuses 
    FOREIGN KEY (AssetStatusId) REFERENCES AssetStatuses(Id);

-- Similar for other FKs...
```

### Step 4: Drop Old Text Columns
```sql
ALTER TABLE Assets DROP COLUMN AssetType;
ALTER TABLE Assets DROP COLUMN Status;
-- Similar for other columns...
```

## Benefits of Normalization

### Data Integrity
- ✅ Referential integrity enforced by FK constraints
- ✅ No orphaned or stale data
- ✅ Single source of truth for each value

### Query Performance
- ✅ Integer FK lookups faster than text comparisons
- ✅ Better indexing on numeric columns
- ✅ Reduced table size (smaller I/O)
- ✅ Easier query optimization

### Maintainability
- ✅ Consistent naming (no "Server", "server", "SERVER")
- ✅ Centralized value management
- ✅ Easier to add new values
- ✅ Clearer entity relationships

### Scalability
- ✅ Supports millions of records efficiently
- ✅ Better for reporting and analytics
- ✅ Easier to implement caching strategies

## Implementation Timeline

| Phase | Task | Status |
|-------|------|--------|
| 1 | Create lookup table entities | ✅ DONE |
| 2 | Update Asset entity with FKs | 🔄 IN PROGRESS |
| 3 | Update LoginAudit entity | 🔄 IN PROGRESS |
| 4 | Create migration script | ⏳ PENDING |
| 5 | Update services/queries | ⏳ PENDING |
| 6 | Update DTOs | ⏳ PENDING |
| 7 | Update controllers | ⏳ PENDING |
| 8 | Test and validate | ⏳ PENDING |

## Backward Compatibility

### API Changes
- DTOs will include both ID and Name for display
- Queries will accept either ID or Name for flexibility
- Gradual migration of frontend to use IDs

### Database Changes
- Old text columns will be kept temporarily for migration
- Dropped after data validation
- Rollback plan: Keep backup of old schema

## Conclusion

This normalization effort will significantly improve data quality, query performance, and system maintainability. The phased approach allows for gradual migration without disrupting current operations.

---

**Report Generated:** March 11, 2026
**Status:** In Progress
**Next Review:** After Phase 2 completion
