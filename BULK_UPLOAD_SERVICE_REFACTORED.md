# BulkUploadService Refactoring - Key Changes

## Critical Updates Required

### 1. Remove Default Status Behavior
**Current**: Defaults unknown status to "InUse"
**New**: Require explicit status, return error if invalid

### 2. Strict Validation
- Status: Must be one of: inuse, spare, repair, decommissioned
- Criticality: Must be one of: TMS general, TMS critical, IT general, IT critical  
- Placing: Must be one of: lane area, booth area, plaza area, server room, control room, admin building
- Region: Free text, non-empty, max 100 chars
- Location: Free text (state or district), non-empty

### 3. Per-Row Error Reporting
Return structured errors:
```json
{
  "rowNumber": 5,
  "assetTag": "AST-001",
  "field": "status",
  "invalidValue": "broken",
  "errorMessage": "Invalid status value: 'broken'. Expected: inuse, spare, repair, or decommissioned"
}
```

### 4. Partial Success
- Process all valid rows
- Return both successCount and list of failed rows
- Don't abort entire upload on first error

## Implementation Notes

The full implementation requires updating:
1. `Services/BulkUploadService.cs` - validation logic
2. `Controllers/AssetsController.cs` - DTO mappings with Placing
3. Frontend forms - dropdowns and validation
4. Frontend view/edit - tab structure

Due to the extensive scope (20+ files, 1000+ lines of changes), I recommend:
1. Implementing in phases
2. Testing each phase before proceeding
3. Creating feature branch for this refactoring

## Files Ready for Update
- ✅ Database schema (Placing column added)
- ✅ Asset entity (with AssetEnumHelpers)
- ✅ DTOs (with Placing field)
- ⏳ BulkUploadService (needs validation update)
- ⏳ Controllers (needs canonical format mapping)
- ⏳ Frontend (needs form updates and tab structure)
