-- Migration: Update existing assets with default Placing value
-- Date: 2026-02-27
-- Description: Sets default 'Server Room' for existing assets that have NULL or empty Placing

-- Update all assets with NULL or empty Placing to have a default value
UPDATE Assets
SET Placing = 'Server Room'
WHERE Placing IS NULL OR Placing = '';

-- Verify the update
SELECT 
    COUNT(*) as TotalAssets,
    COUNT(CASE WHEN Placing IS NOT NULL AND Placing != '' THEN 1 END) as AssetsWithPlacing,
    COUNT(CASE WHEN Placing IS NULL OR Placing = '' THEN 1 END) as AssetsWithoutPlacing
FROM Assets;

PRINT 'Migration completed: All existing assets now have Placing value';
