-- Add AssetId column to SoftwareAssets table
ALTER TABLE SoftwareAssets
ADD AssetId NVARCHAR(50) NULL;

GO

-- Update existing records with generated IDs
DECLARE @Counter INT = 1;
DECLARE @Id INT;

DECLARE asset_cursor CURSOR FOR
SELECT Id FROM SoftwareAssets ORDER BY Id;

OPEN asset_cursor;
FETCH NEXT FROM asset_cursor INTO @Id;

WHILE @@FETCH_STATUS = 0
BEGIN
    UPDATE SoftwareAssets
    SET AssetId = 'ASTS' + RIGHT('00000' + CAST(@Counter AS NVARCHAR(5)), 5)
    WHERE Id = @Id;
    
    SET @Counter = @Counter + 1;
    FETCH NEXT FROM asset_cursor INTO @Id;
END;

CLOSE asset_cursor;
DEALLOCATE asset_cursor;

GO

-- Make AssetId NOT NULL
ALTER TABLE SoftwareAssets
ALTER COLUMN AssetId NVARCHAR(50) NOT NULL;

GO

-- Create a unique index on AssetId
CREATE UNIQUE INDEX IX_SoftwareAssets_AssetId ON SoftwareAssets(AssetId);



