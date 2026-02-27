-- Migration: Add Placing Field and Update Status/Criticality Enums
-- Date: 2026-02-27
-- Description: Add Placing field, normalize status values, update criticality mapping

-- Step 1: Add Placing column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Assets' AND COLUMN_NAME = 'Placing')
BEGIN
    ALTER TABLE Assets ADD Placing NVARCHAR(50) NULL;
    PRINT 'Added Placing column';
END
ELSE
BEGIN
    PRINT 'Placing column already exists';
END
GO

-- Step 2: Update Status enum values to match new canonical format
-- Current: InUse=1, Spare=2, Repair=3, Decommissioned=4, Unknown=5
-- New canonical string values: 'inuse', 'spare', 'repair', 'decommissioned'
-- Note: We keep the integer enum in code but ensure string representations are lowercase

-- Step 3: Normalize any existing status data (if needed)
-- This is handled in the application layer for backward compatibility

-- Step 4: Update Criticality enum mapping
-- Current: TMSCritical=1, TMSGeneral=2, ITCritical=3, ITGeneral=4
-- New display: 'TMS critical', 'TMS general', 'IT critical', 'IT general'
-- Note: Enum values remain the same, only display format changes

-- Step 5: Add check constraints for Placing (optional, can be enforced in app layer)
-- Allowed values: 'lane area', 'booth area', 'plaza area', 'server room', 'control room', 'admin building'

PRINT 'Migration completed successfully';
GO
