-- Migration: Drop AssetClassification column
-- Date: 2026-02-27
-- Description: Remove AssetClassification enum column, use Classification text field instead

BEGIN TRANSACTION;

-- Drop the AssetClassification column
ALTER TABLE Assets DROP COLUMN AssetClassification;

COMMIT TRANSACTION;

PRINT 'Successfully dropped AssetClassification column';
