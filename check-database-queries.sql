-- Queries to check ITAMS_Shared database content
-- Run these in SQL Server Management Studio

USE ITAMS_Shared;
GO

-- Check all users
SELECT * FROM Users;

-- Check all roles  
SELECT * FROM Roles;

-- Check all permissions
SELECT * FROM Permissions;

-- Check role permissions
SELECT 
    r.Name as RoleName,
    p.Name as PermissionName,
    rp.IsGranted
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.Id
JOIN Permissions p ON rp.PermissionId = p.Id
ORDER BY r.Name, p.Name;

-- Check projects (should be empty)
SELECT * FROM Projects;

-- Check locations (should be empty)
SELECT * FROM Locations;

-- Database info
SELECT 
    'Database: ' + DB_NAME() as Info
UNION ALL
SELECT 
    'Tables count: ' + CAST(COUNT(*) as VARCHAR) as Info
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';