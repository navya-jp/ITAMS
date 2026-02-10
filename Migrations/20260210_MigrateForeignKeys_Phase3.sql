-- Migration Phase 3: Drop Old Foreign Keys and Create New Ones
-- Date: 2026-02-10
-- Description: Drops foreign keys that reference primary keys and creates new ones that reference alternate keys

PRINT '=====================================================';
PRINT 'PHASE 3: DROP OLD FOREIGN KEYS';
PRINT '=====================================================';

-- Drop foreign keys from Users table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Roles_RoleId')
BEGIN
    ALTER TABLE Users DROP CONSTRAINT FK_Users_Roles_RoleId;
    PRINT 'Dropped FK_Users_Roles_RoleId';
END
GO

-- Drop foreign keys from Assets table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_Projects_ProjectId')
BEGIN
    ALTER TABLE Assets DROP CONSTRAINT FK_Assets_Projects_ProjectId;
    PRINT 'Dropped FK_Assets_Projects_ProjectId';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_Locations_LocationId')
BEGIN
    ALTER TABLE Assets DROP CONSTRAINT FK_Assets_Locations_LocationId;
    PRINT 'Dropped FK_Assets_Locations_LocationId';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Assets_Users_AssignedUserId')
BEGIN
    ALTER TABLE Assets DROP CONSTRAINT FK_Assets_Users_AssignedUserId;
    PRINT 'Dropped FK_Assets_Users_AssignedUserId';
END
GO

-- Drop foreign keys from Locations table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Locations_Projects_ProjectId')
BEGIN
    ALTER TABLE Locations DROP CONSTRAINT FK_Locations_Projects_ProjectId;
    PRINT 'Dropped FK_Locations_Projects_ProjectId';
END
GO

-- Drop foreign keys from UserProjects table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserProjects_Users_UserId')
BEGIN
    ALTER TABLE UserProjects DROP CONSTRAINT FK_UserProjects_Users_UserId;
    PRINT 'Dropped FK_UserProjects_Users_UserId';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserProjects_Projects_ProjectId')
BEGIN
    ALTER TABLE UserProjects DROP CONSTRAINT FK_UserProjects_Projects_ProjectId;
    PRINT 'Dropped FK_UserProjects_Projects_ProjectId';
END
GO

-- Drop foreign keys from RolePermissions table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_Roles_RoleId')
BEGIN
    ALTER TABLE RolePermissions DROP CONSTRAINT FK_RolePermissions_Roles_RoleId;
    PRINT 'Dropped FK_RolePermissions_Roles_RoleId';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_Permissions_PermissionId')
BEGIN
    ALTER TABLE RolePermissions DROP CONSTRAINT FK_RolePermissions_Permissions_PermissionId;
    PRINT 'Dropped FK_RolePermissions_Permissions_PermissionId';
END
GO

-- Drop foreign keys from UserProjectPermissions table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserProjectPermissions_Permissions_PermissionId')
BEGIN
    ALTER TABLE UserProjectPermissions DROP CONSTRAINT FK_UserProjectPermissions_Permissions_PermissionId;
    PRINT 'Dropped FK_UserProjectPermissions_Permissions_PermissionId';
END
GO

-- Drop foreign keys from AuditEntries table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AuditEntries_Users_UserId')
BEGIN
    ALTER TABLE AuditEntries DROP CONSTRAINT FK_AuditEntries_Users_UserId;
    PRINT 'Dropped FK_AuditEntries_Users_UserId';
END
GO

-- Drop foreign keys from RbacRolePermissions table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacRolePermissions_Role')
BEGIN
    ALTER TABLE RbacRolePermissions DROP CONSTRAINT FK_RbacRolePermissions_Role;
    PRINT 'Dropped FK_RbacRolePermissions_Role';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacRolePermissions_Permission')
BEGIN
    ALTER TABLE RbacRolePermissions DROP CONSTRAINT FK_RbacRolePermissions_Permission;
    PRINT 'Dropped FK_RbacRolePermissions_Permission';
END
GO

-- Drop foreign keys from RbacUserPermissions table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacUserPermissions_User')
BEGIN
    ALTER TABLE RbacUserPermissions DROP CONSTRAINT FK_RbacUserPermissions_User;
    PRINT 'Dropped FK_RbacUserPermissions_User';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacUserPermissions_Permission')
BEGIN
    ALTER TABLE RbacUserPermissions DROP CONSTRAINT FK_RbacUserPermissions_Permission;
    PRINT 'Dropped FK_RbacUserPermissions_Permission';
END
GO

-- Drop foreign keys from RbacUserScope table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacUserScope_User')
BEGIN
    ALTER TABLE RbacUserScope DROP CONSTRAINT FK_RbacUserScope_User;
    PRINT 'Dropped FK_RbacUserScope_User';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacUserScope_Project')
BEGIN
    ALTER TABLE RbacUserScope DROP CONSTRAINT FK_RbacUserScope_Project;
    PRINT 'Dropped FK_RbacUserScope_Project';
END
GO

PRINT '';
PRINT '=====================================================';
PRINT 'PHASE 3: CREATE NEW FOREIGN KEYS';
PRINT '=====================================================';

-- Create new foreign keys for Users table
ALTER TABLE Users ADD CONSTRAINT FK_Users_Roles_RoleIdRef
    FOREIGN KEY (RoleIdRef) REFERENCES Roles(RoleId);
PRINT 'Created FK_Users_Roles_RoleIdRef';
GO

-- Create new foreign keys for Assets table
ALTER TABLE Assets ADD CONSTRAINT FK_Assets_Projects_ProjectIdRef
    FOREIGN KEY (ProjectIdRef) REFERENCES Projects(ProjectId);
PRINT 'Created FK_Assets_Projects_ProjectIdRef';
GO

ALTER TABLE Assets ADD CONSTRAINT FK_Assets_Locations_LocationIdRef
    FOREIGN KEY (LocationIdRef) REFERENCES Locations(LocationId);
PRINT 'Created FK_Assets_Locations_LocationIdRef';
GO

ALTER TABLE Assets ADD CONSTRAINT FK_Assets_Users_AssignedUserIdRef
    FOREIGN KEY (AssignedUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_Assets_Users_AssignedUserIdRef';
GO

-- Create new foreign keys for Locations table
ALTER TABLE Locations ADD CONSTRAINT FK_Locations_Projects_ProjectIdRef
    FOREIGN KEY (ProjectIdRef) REFERENCES Projects(ProjectId);
PRINT 'Created FK_Locations_Projects_ProjectIdRef';
GO

-- Create new foreign keys for UserProjects table
ALTER TABLE UserProjects ADD CONSTRAINT FK_UserProjects_Users_UserIdRef
    FOREIGN KEY (UserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_UserProjects_Users_UserIdRef';
GO

ALTER TABLE UserProjects ADD CONSTRAINT FK_UserProjects_Projects_ProjectIdRef
    FOREIGN KEY (ProjectIdRef) REFERENCES Projects(ProjectId);
PRINT 'Created FK_UserProjects_Projects_ProjectIdRef';
GO

-- Create new foreign keys for RolePermissions table
ALTER TABLE RolePermissions ADD CONSTRAINT FK_RolePermissions_Roles_RoleIdRef
    FOREIGN KEY (RoleIdRef) REFERENCES Roles(RoleId);
PRINT 'Created FK_RolePermissions_Roles_RoleIdRef';
GO

ALTER TABLE RolePermissions ADD CONSTRAINT FK_RolePermissions_Permissions_PermissionIdRef
    FOREIGN KEY (PermissionIdRef) REFERENCES Permissions(PermissionId);
PRINT 'Created FK_RolePermissions_Permissions_PermissionIdRef';
GO

-- Create new foreign keys for UserProjectPermissions table
ALTER TABLE UserProjectPermissions ADD CONSTRAINT FK_UserProjectPermissions_Permissions_PermissionIdRef
    FOREIGN KEY (PermissionIdRef) REFERENCES Permissions(PermissionId);
PRINT 'Created FK_UserProjectPermissions_Permissions_PermissionIdRef';
GO

-- Create new foreign keys for AuditEntries table
ALTER TABLE AuditEntries ADD CONSTRAINT FK_AuditEntries_Users_UserIdRef
    FOREIGN KEY (UserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_AuditEntries_Users_UserIdRef';
GO

-- Create new foreign keys for RbacRolePermissions table
ALTER TABLE RbacRolePermissions ADD CONSTRAINT FK_RbacRolePermissions_Roles_RoleIdRef
    FOREIGN KEY (RoleIdRef) REFERENCES RbacRoles(RbacRoleId);
PRINT 'Created FK_RbacRolePermissions_Roles_RoleIdRef';
GO

ALTER TABLE RbacRolePermissions ADD CONSTRAINT FK_RbacRolePermissions_Permissions_PermissionIdRef
    FOREIGN KEY (PermissionIdRef) REFERENCES RbacPermissions(RbacPermissionId);
PRINT 'Created FK_RbacRolePermissions_Permissions_PermissionIdRef';
GO

-- Create new foreign keys for RbacUserPermissions table
ALTER TABLE RbacUserPermissions ADD CONSTRAINT FK_RbacUserPermissions_Users_UserIdRef
    FOREIGN KEY (UserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacUserPermissions_Users_UserIdRef';
GO

ALTER TABLE RbacUserPermissions ADD CONSTRAINT FK_RbacUserPermissions_Permissions_PermissionIdRef
    FOREIGN KEY (PermissionIdRef) REFERENCES RbacPermissions(RbacPermissionId);
PRINT 'Created FK_RbacUserPermissions_Permissions_PermissionIdRef';
GO

-- Create new foreign keys for RbacUserScope table
ALTER TABLE RbacUserScope ADD CONSTRAINT FK_RbacUserScope_Users_UserIdRef
    FOREIGN KEY (UserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacUserScope_Users_UserIdRef';
GO

ALTER TABLE RbacUserScope ADD CONSTRAINT FK_RbacUserScope_Projects_ProjectIdRef
    FOREIGN KEY (ProjectIdRef) REFERENCES Projects(ProjectId);
PRINT 'Created FK_RbacUserScope_Projects_ProjectIdRef';
GO

PRINT '';
PRINT '=====================================================';
PRINT 'PHASE 3 COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'Old foreign keys dropped and new foreign keys created';
PRINT 'All foreign keys now reference alternate keys instead of primary keys';
PRINT '';
PRINT 'IMPORTANT: The old FK columns (RoleId, ProjectId, etc.) still exist';
PRINT 'They can be kept for backward compatibility or dropped in a future migration';
GO
