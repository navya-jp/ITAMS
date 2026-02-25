-- Add text fields for location data to Assets table
-- This allows storing location information directly from Excel without requiring database lookups

ALTER TABLE Assets
ADD Region NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    Site NVARCHAR(200) NULL,
    PlazaName NVARCHAR(200) NULL,
    LocationText NVARCHAR(200) NULL,
    Department NVARCHAR(100) NULL;

