-- Reset IsCustom flag: only the standard base subtypes should have IsCustom = 0
-- All others (old imported data) are reset so they don't appear in the dropdown
UPDATE AssetSubTypes
SET IsCustom = 0
WHERE SubTypeName NOT IN (
    'Desktop','Laptop','Server','Monitor','Printer','Scanner',
    'UPS','Switch','Router','Firewall','Keyboard','Camera','NVR'
);
