-- Migration: Assign all users to default project
-- Date: 2026-02-13
-- Description: Ensure all users have a ProjectId assigned

-- Step 1: Create default project if none exists
IF NOT EXISTS (SELECT 1 FROM Projects WHERE Code = 'DEFAULT')
BEGIN
    INSERT INTO Projects (ProjectId, Name, PreferredName, SpvName, Code, States, Description, IsActive, CreatedAt, CreatedBy)
    VALUES ('PRJ00001', 'Default Project', 'Default', 'Default SPV', 'DEFAULT', '', 'Default project for existing users', 1, GETUTCDATE(), 1);
    PRINT 'Default project created';
END
ELSE
BEGIN
    PRINT 'Default project already exists';
END

GO

-- Step 2: Assign all users without ProjectId to the default project
DECLARE @DefaultProjectId INT;
SELECT @DefaultProjectId = Id FROM Projects WHERE Code = 'DEFAULT';

UPDATE Users 
SET ProjectId = @DefaultProjectId 
WHERE ProjectId IS NULL;

PRINT 'All users assigned to default project';

GO

-- Step 3: Verify all users have ProjectId
SELECT 
    COUNT(*) as TotalUsers,
    COUNT(ProjectId) as UsersWithProject,
    COUNT(*) - COUNT(ProjectId) as UsersWithoutProject
FROM Users;

GO
