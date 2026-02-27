-- Migration: Rename Criticality to AssetClassification
-- Date: 2026-02-27
-- Description: Rename the Criticality column to AssetClassification in Assets table

BEGIN TRANSACTION;

-- Rename the column
EXEC sp_rename 'Assets.Criticality', 'AssetClassification', 'COLUMN';

COMMIT TRANSACTION;

PRINT 'Successfully renamed Criticality to AssetClassification';
