# Asset Management Refactoring - Implementation Plan

## Overview
Comprehensive end-to-end refactoring of asset management system including bulk upload fixes, form updates, and UI improvements.

## Phase 1: Database Schema ✅ COMPLETED
- [x] Added `Placing` field (NVARCHAR(50))
- [x] Migration created and executed
- [x] Updated Asset entity with Placing field
- [x] Created AssetEnumHelpers for canonical string conversions

## Phase 2: Backend API Updates (IN PROGRESS)
### 2.1 BulkUploadService Updates
- [ ] Remove default status to 'inuse'
- [ ] Add strict validation for status, criticality, placing
- [ ] Return structured per-row errors
- [ ] Support partial success (process valid rows even if some fail)
- [ ] Fix region mapping
- [ ] Support free-text location (state/district)

### 2.2 Controller Updates
- [ ] Update AssetsController to use canonical string formats
- [ ] Add Placing field to all DTO mappings
- [ ] Update validation logic

### 2.3 DTO Updates
- [x] Added Placing to AssetDto
- [x] Added Placing to CreateAssetDto
- [x] Added Placing to UpdateAssetDto
- [x] Changed default status to "inuse"

## Phase 3: Frontend Updates (PENDING)
### 3.1 Manual Form Updates
- [ ] Update status dropdown: inuse, spare, repair, decommissioned
- [ ] Update criticality dropdown: TMS general, TMS critical, IT general, IT critical
- [ ] Add Placing dropdown with 6 options
- [ ] Change Location to free-text input
- [ ] Add validation

### 3.2 View/Edit Details - Tab Structure
- [ ] Create tab-based layout (General, Location & Placement, Operational, Audit/Meta)
- [ ] Implement tab switching without data loss
- [ ] Add sticky action bar
- [ ] Show validation errors per tab

### 3.3 Bulk Upload UI
- [ ] Update error display to show per-row errors
- [ ] Show partial success results

## Phase 4: Testing (PENDING)
- [ ] Bulk upload parser tests
- [ ] Manual form validation tests
- [ ] Tab switching tests
- [ ] Migration rollback test

## Phase 5: Documentation (PENDING)
- [ ] API documentation updates
- [ ] Migration notes
- [ ] Rollback procedures
- [ ] Changelog

## Current Status
**Phase 1 Complete**: Database schema updated, Placing field added, enum helpers created.
**Phase 2 In Progress**: Need to update BulkUploadService and Controllers.

## Next Steps
1. Update BulkUploadService with new validation logic
2. Update Controllers to use canonical formats
3. Update frontend forms and add tab structure
4. Implement comprehensive testing
5. Document all changes

## Breaking Changes
- Status values now lowercase: "inuse" instead of "InUse"
- Criticality display format: "TMS general" instead of "TMSGeneral"
- Placing field now required for new assets
- Bulk upload no longer defaults missing status to "inuse"

## Backward Compatibility
- Legacy status values (InUse, Spare, etc.) still accepted via ParseStatus()
- Typo "decommitioned" mapped to "decommissioned"
- Old criticality formats still parsed correctly
