-- Shift assets after ASTH00096 with no placing to site (Lane Area = 1)
-- Assets up to ASTH00096 (Ahmedabad) remain in head office
UPDATE Assets 
SET AssetPlacingId = 1 
WHERE AssetPlacingId IS NULL 
  AND AssetId > 'ASTH00096';

SELECT @@ROWCOUNT AS UpdatedCount;
