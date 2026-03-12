# Phase 3: Service & Controller Updates Guide

## Overview

Phase 2 (Entity Model Updates) is complete. Phase 3 requires updating all services, controllers, and DTOs to work with the new FK-based structure instead of text fields.

## Compilation Errors Summary

**Total Errors:** 104  
**Categories:**
1. **LoginAudit.Username removed** - 4 errors
2. **LoginAudit.Status removed** - 4 errors  
3. **AuditEntry.UserName removed** - 2 errors
4. **Asset text fields removed** - 60+ errors
5. **AssetEnumHelpers removed** - 8 errors
6. **AssetStatus enum removed** - 4 errors

---

## Files Requiring Updates

### 1. Controllers/AuthController.cs
**Errors:** 4 (LoginAudit.Username, LoginAudit.Status)

**Changes Needed:**
```csharp
// OLD (WRONG):
loginAudit.Username = user.Username;
loginAudit.Status = "ACTIVE";

// NEW (CORRECT):
// Username comes from User navigation property
// Status now uses FK to SessionStatus lookup table
var activeStatus = await context.SessionStatuses
    .FirstOrDefaultAsync(s => s.Name == "ACTIVE");
loginAudit.SessionStatusId = activeStatus?.Id;
```

### 2. Controllers/AssetsController.cs
**Errors:** 60+ (Asset text fields, AssetEnumHelpers, AssetStatus enum)

**Key Changes:**
- Replace `asset.AssetType` (string) with `asset.AssetTypeId` (int FK)
- Replace `asset.Status` (enum) with `asset.AssetStatusId` (int FK)
- Replace `asset.Classification` (string) with `asset.AssetClassificationId` (int FK)
- Replace `asset.OSType` (string) with `asset.OperatingSystemId` (int FK)
- Replace `asset.DBType` (string) with `asset.DatabaseTypeId` (int FK)
- Replace `asset.PatchStatus` (string) with `asset.PatchStatusId` (int FK)
- Replace `asset.USBBlockingStatus` (string) with `asset.USBBlockingStatusId` (int FK)
- Replace `asset.Placing` (string) with `asset.AssetPlacingId` (int FK)
- Replace `asset.Vendor` (string) with `asset.VendorId` (int FK)
- Remove `asset.AssignedUserRole` (use User.Role instead)
- Remove `asset.AssignedUserText` (use AssignedUserId FK)
- Remove `AssetEnumHelpers` calls

**Example Refactoring:**
```csharp
// OLD (WRONG):
var assetDto = new AssetDto
{
    AssetType = asset.AssetType,
    Status = asset.Status.ToDisplayString(),
    Classification = asset.Classification,
    Placing = asset.Placing
};

// NEW (CORRECT):
var assetDto = new AssetDto
{
    AssetTypeId = asset.AssetTypeId,
    AssetTypeName = asset.AssetType?.Name,
    AssetStatusId = asset.AssetStatusId,
    AssetStatusName = asset.AssetStatus?.Name,
    AssetClassificationId = asset.AssetClassificationId,
    AssetClassificationName = asset.Classification?.Name,
    AssetPlacingId = asset.AssetPlacingId,
    AssetPlacingName = asset.Placing?.Name
};
```

### 3. Controllers/SuperAdminController.cs
**Errors:** 3 (AuditEntry.UserName, LoginAudit.Username, LoginAudit.Status)

**Changes:**
- Remove `auditEntry.UserName` - use `auditEntry.User?.Username` instead
- Remove `loginAudit.Username` - use `loginAudit.User?.Username` instead
- Replace `loginAudit.Status` with `loginAudit.SessionStatus?.Name`

### 4. Services/AuditService.cs
**Errors:** 1 (AuditEntry.UserName)

**Change:**
```csharp
// OLD (WRONG):
auditEntry.UserName = user.Username;

// NEW (CORRECT):
// UserName is removed - username comes from User navigation property
```

### 5. Services/SessionCleanupService.cs
**Errors:** 1 (LoginAudit.Status)

**Change:**
```csharp
// OLD (WRONG):
if (loginAudit.Status == "ACTIVE")

// NEW (CORRECT):
if (loginAudit.SessionStatus?.Name == "ACTIVE")
```

### 6. Services/BulkUploadService.cs
**Errors:** 20+ (Asset text fields, AssetEnumHelpers, AssetStatus enum)

**Major Refactoring Needed:**
- Replace all text field assignments with FK lookups
- Remove AssetEnumHelpers usage
- Query lookup tables to get IDs before assigning

**Example:**
```csharp
// OLD (WRONG):
asset.AssetType = row["AssetType"]?.ToString();
asset.Status = AssetEnumHelpers.ParseStatus(row["Status"]?.ToString());
asset.Classification = row["Classification"]?.ToString();

// NEW (CORRECT):
var assetTypeName = row["AssetType"]?.ToString();
if (!string.IsNullOrEmpty(assetTypeName))
{
    var assetType = await context.AssetTypes
        .FirstOrDefaultAsync(at => at.Name == assetTypeName);
    asset.AssetTypeId = assetType?.Id;
}

var statusName = row["Status"]?.ToString();
if (!string.IsNullOrEmpty(statusName))
{
    var status = await context.AssetStatuses
        .FirstOrDefaultAsync(s => s.Name == statusName);
    asset.AssetStatusId = status?.Id;
}

var classificationName = row["Classification"]?.ToString();
if (!string.IsNullOrEmpty(classificationName))
{
    var classification = await context.AssetClassifications
        .FirstOrDefaultAsync(c => c.Name == classificationName);
    asset.AssetClassificationId = classification?.Id;
}
```

### 7. Models/AssetDtos.cs
**Changes Needed:**

```csharp
public class AssetDto
{
    // Add FK properties
    public int? AssetTypeId { get; set; }
    public string? AssetTypeName { get; set; }
    
    public int? AssetStatusId { get; set; }
    public string? AssetStatusName { get; set; }
    
    public int? AssetClassificationId { get; set; }
    public string? AssetClassificationName { get; set; }
    
    public int? OperatingSystemId { get; set; }
    public string? OperatingSystemName { get; set; }
    
    public int? DatabaseTypeId { get; set; }
    public string? DatabaseTypeName { get; set; }
    
    public int? PatchStatusId { get; set; }
    public string? PatchStatusName { get; set; }
    
    public int? USBBlockingStatusId { get; set; }
    public string? USBBlockingStatusName { get; set; }
    
    public int? AssetPlacingId { get; set; }
    public string? AssetPlacingName { get; set; }
    
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
    
    // Remove old text fields
    // - AssetType (string)
    // - Status (string)
    // - Classification (string)
    // - OSType (string)
    // - DBType (string)
    // - PatchStatus (string)
    // - USBBlockingStatus (string)
    // - Placing (string)
    // - Vendor (string)
    // - AssignedUserRole (string)
    // - AssignedUserText (string)
    // - UserRole (string)
}
```

---

## Implementation Strategy

### Step 1: Update DTOs First
- Add FK properties to AssetDto
- Add Name properties for display
- Remove old text fields

### Step 2: Update Services
- BulkUploadService - Most complex, needs FK lookups
- AuditService - Simple, just remove UserName
- SessionCleanupService - Simple, use navigation property

### Step 3: Update Controllers
- AssetsController - Most complex, needs DTO mapping updates
- AuthController - Simple, handle SessionStatus FK
- SuperAdminController - Simple, use navigation properties

### Step 4: Update Queries
- Replace text-based WHERE clauses with ID-based
- Add Include() for navigation properties
- Use eager loading to avoid N+1 queries

---

## Query Pattern Examples

### Before (WRONG):
```csharp
var assets = context.Assets
    .Where(a => a.AssetType == "Server" && a.Status == "InUse")
    .ToList();
```

### After (CORRECT):
```csharp
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
```

---

## Testing Checklist

- [ ] All 104 compilation errors resolved
- [ ] Unit tests for BulkUploadService
- [ ] Integration tests for asset creation
- [ ] Integration tests for asset updates
- [ ] Integration tests for asset queries
- [ ] End-to-end tests for asset operations
- [ ] Verify audit logging works
- [ ] Verify login audit works
- [ ] Test asset filtering by type, status, etc.
- [ ] Test asset assignment to users

---

## Performance Considerations

1. **Eager Loading:** Use `.Include()` to load related entities
2. **Caching:** Cache lookup tables (AssetType, AssetStatus, etc.)
3. **Indexing:** Ensure FK columns are indexed (done in migration)
4. **Batch Operations:** Use batch updates for bulk operations

---

## Rollback Plan

If issues arise:
1. Keep old text columns temporarily
2. Populate both old and new columns during transition
3. Gradually migrate code to use new FKs
4. Drop old columns after full migration

---

## Timeline Estimate

- **DTOs:** 1-2 hours
- **Services:** 3-4 hours
- **Controllers:** 4-5 hours
- **Testing:** 2-3 hours
- **Total:** 10-14 hours

---

## Next Steps

1. Execute migration script: `Migrations/20260311_DatabaseNormalization_Phase1.sql`
2. Update DTOs in `Models/AssetDtos.cs`
3. Update services (start with AuditService - simplest)
4. Update controllers (start with AuthController - simpler)
5. Update BulkUploadService (most complex)
6. Run tests
7. Deploy

---

**Status:** Ready for Phase 3 Implementation  
**Date:** March 11, 2026
