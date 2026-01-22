-- Connect to your SQL Server and run these queries to check data

-- Check all tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Check Users table
SELECT TOP 10 * FROM Users;

-- Check Roles table
SELECT * FROM Roles;

-- Check Projects table
SELECT * FROM Projects;

-- Check Locations table
SELECT * FROM Locations;

-- Check Users with their Roles (JOIN query)
SELECT 
    u.Id,
    u.Username,
    u.Email,
    u.FirstName,
    u.LastName,
    r.Name as RoleName,
    u.IsActive,
    u.CreatedAt
FROM Users u
LEFT JOIN Roles r ON u.RoleId = r.Id
ORDER BY u.CreatedAt DESC;

-- Check audit trail
SELECT TOP 20 * FROM AuditEntries ORDER BY Timestamp DESC;