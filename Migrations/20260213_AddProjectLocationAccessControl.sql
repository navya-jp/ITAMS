-- Migration: Add Project and Location-Based Access Control
-- Date: 2026-02-13
-- Description: Implement project isolation and hierarchical location filtering

-- Step 1: Add ProjectId and location restriction columns to Users table
ALTER TABLE Users 
ADD ProjectId INT NULL,
    RestrictedRegion NVARCHAR(100) NULL,
    RestrictedState NVARCHAR(100) NULL,
    RestrictedPlaza NVARCHAR(100) NULL,
    RestrictedOffice NVARCHAR(100) NULL;

GO

-- Step 2: Add foreign key constraint for ProjectId
ALTER TABLE Users 
ADD CONSTRAINT FK_Users_Projects 
FOREIGN KEY (ProjectId) REFERENCES Projects(Id);

GO

-- Step 3: Create default project if none exists
IF NOT EXISTS (SELECT 1 FROM Projects WHERE Code = 'DEFAULT')
BEGIN
    INSERT INTO Projects (ProjectId, Name, PreferredName, SpvName, Code, States, Description, IsActive, CreatedAt, CreatedBy)
    VALUES ('PRJ00001', 'Default Project', 'Default', 'Default SPV', 'DEFAULT', '', 'Default project for existing users', 1, GETUTCDATE(), 1);
END

GO

-- Step 4: Assign all existing users to the default project
DECLARE @DefaultProjectId INT;
SELECT @DefaultProjectId = Id FROM Projects WHERE Code = 'DEFAULT';

UPDATE Users 
SET ProjectId = @DefaultProjectId 
WHERE ProjectId IS NULL;

GO

-- Step 5: Make ProjectId required (NOT NULL) after assigning all users
ALTER TABLE Users 
ALTER COLUMN ProjectId INT NOT NULL;

GO

-- Step 6: Create index for better query performance
CREATE INDEX IX_Users_ProjectId ON Users(ProjectId);
CREATE INDEX IX_Users_LocationRestrictions ON Users(RestrictedRegion, RestrictedState, RestrictedPlaza, RestrictedOffice);

GO

-- Step 7: Add IsSensitive flag to Locations table (for future use)
ALTER TABLE Locations 
ADD IsSensitive BIT NOT NULL DEFAULT 0,
    SensitiveReason NVARCHAR(500) NULL;

GO

PRINT 'Project and Location Access Control schema updated successfully';
PRINT 'All existing users assigned to DEFAULT project';
PRINT 'Location restrictions are NULL (full access within project)';
