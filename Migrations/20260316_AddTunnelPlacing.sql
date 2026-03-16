-- Add Tunnel, Scrap Area, Spare Store to AssetPlacings master data
IF NOT EXISTS (SELECT 1 FROM AssetPlacings WHERE Name = 'Tunnel')
    INSERT INTO AssetPlacings (PlacingId, Name, IsActive, CreatedAt) VALUES ('PLC007', 'Tunnel', 1, GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM AssetPlacings WHERE Name = 'Scrap Area')
    INSERT INTO AssetPlacings (PlacingId, Name, IsActive, CreatedAt) VALUES ('PLC008', 'Scrap Area', 1, GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM AssetPlacings WHERE Name = 'Spare Store')
    INSERT INTO AssetPlacings (PlacingId, Name, IsActive, CreatedAt) VALUES ('PLC009', 'Spare Store', 1, GETUTCDATE());
