-- Migration: Make Users.ProjectId nullable for SuperAdmins
-- Date: 2026-02-23
-- Description: SuperAdmins should have access to all projects without being assigned to a specific one

-- Make ProjectId nullable
ALTER TABLE Users
ALTER COLUMN ProjectId INT NULL;

-- Update existing SuperAdmin users to have NULL ProjectId
UPDATE Users
SET ProjectId = NULL
WHERE RoleId = (SELECT Id FROM Roles WHERE Name = 'Super Admin');

PRINT 'Migration completed: Users.ProjectId is now nullable for SuperAdmins';
