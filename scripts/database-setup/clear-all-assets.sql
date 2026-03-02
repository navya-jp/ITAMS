-- Clear all assets from the database
-- This will reset the asset table so bulk upload can start from AST00001

DELETE FROM Assets;

-- Verify the table is empty
SELECT COUNT(*) AS RemainingAssets FROM Assets;
