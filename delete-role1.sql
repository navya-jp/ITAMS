-- Check if Role1 exists and if any users are assigned to it
SELECT Id, Name, Description FROM Roles WHERE Name = 'Role1';

SELECT COUNT(*) as UserCount FROM Users WHERE RoleId = 6;

-- Delete Role1 if no users are assigned to it
DELETE FROM Roles WHERE Name = 'Role1' AND Id = 6;

-- Verify deletion
SELECT Id, Name, Description FROM Roles WHERE IsActive = 1;
