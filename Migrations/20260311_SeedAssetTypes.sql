-- ============================================================================
-- ITAMS Database - Seed AssetTypes from existing Asset data
-- Date: March 11, 2026
-- Purpose: Populate AssetTypes table with distinct values from Assets
-- ============================================================================

-- First, get the AssetCategory for IT Equipment (assuming it exists or create it)
DECLARE @CategoryId INT
SELECT @CategoryId = Id FROM AssetCategories WHERE CategoryName = 'IT Equipment'
IF @CategoryId IS NULL
BEGIN
    INSERT INTO AssetCategories (CategoryName, CategoryCode, IsActive, CreatedAt, CreatedBy)
    VALUES ('IT Equipment', 'IT_EQP', 1, GETUTCDATE(), 1)
    SET @CategoryId = SCOPE_IDENTITY()
END

-- Insert distinct AssetTypes from Assets table
INSERT INTO AssetTypes (CategoryId, TypeName, TypeCode, IsActive, CreatedAt, CreatedBy)
SELECT DISTINCT
    @CategoryId,
    AssetType as TypeName,
    UPPER(REPLACE(REPLACE(AssetType, ' ', '_'), '-', '_')) as TypeCode,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    1 as CreatedBy
FROM Assets
WHERE AssetType IS NOT NULL AND AssetType != ''
AND NOT EXISTS (
    SELECT 1 FROM AssetTypes at WHERE at.TypeName = Assets.AssetType
)

-- Verify
SELECT 'AssetTypes seeded:' as Message, COUNT(*) as Count FROM AssetTypes
