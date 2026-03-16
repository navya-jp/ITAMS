-- Add Tunnel to AssetPlacings master data
IF NOT EXISTS (SELECT 1 FROM AssetPlacings WHERE Name = 'Tunnel')
BEGIN
    INSERT INTO AssetPlacings (Name) VALUES ('Tunnel');
END
