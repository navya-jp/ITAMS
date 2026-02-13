-- Update Read Only User role to Project Manager
UPDATE RbacRoles 
SET Name = 'Project Manager'
WHERE Name = 'Read Only User';

-- Also update in the old Roles table if it exists
UPDATE Roles 
SET Name = 'Project Manager'
WHERE Name = 'Read Only User';

-- Verify the changes
SELECT Id, RbacRoleId, Name, Description, Status 
FROM RbacRoles 
WHERE Name = 'Project Manager';
