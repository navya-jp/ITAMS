-- Migration Phase 2: Migrate Foreign Keys to Use Alternate Keys
-- Date: 2026-02-10
-- Description: Adds new foreign key columns that reference alternate keys and populates them

PRINT '=====================================================';
PRINT 'PHASE 2: ADD NEW FOREIGN KEY COLUMNS';
PRINT '=====================================================';

-- =====================================================
-- USERS TABLE
-- =====================================================
-- Users.RoleId → Roles.RoleId (alternate key)
ALTER TABLE Users ADD RoleIdRef NVARCHAR(50) NULL;
PRINT 'Added Users.RoleIdRef column';
GO

UPDATE Users 
SET RoleIdRef = (SELECT RoleId FROM Roles WHERE Roles.Id = Users.RoleId);
PRINT 'Populated Users.RoleIdRef';
GO

ALTER TABLE Users ALTER COLUMN RoleIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set Users.RoleIdRef to NOT NULL';
GO

-- =====================================================
-- ASSETS TABLE
-- =====================================================
-- Assets.ProjectId → Projects.ProjectId (alternate key)
ALTER TABLE Assets ADD ProjectIdRef NVARCHAR(50) NULL;
PRINT 'Added Assets.ProjectIdRef column';
GO

UPDATE Assets 
SET ProjectIdRef = (SELECT ProjectId FROM Projects WHERE Projects.Id = Assets.ProjectId);
PRINT 'Populated Assets.ProjectIdRef';
GO

ALTER TABLE Assets ALTER COLUMN ProjectIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set Assets.ProjectIdRef to NOT NULL';
GO

-- Assets.LocationId → Locations.LocationId (alternate key)
ALTER TABLE Assets ADD LocationIdRef NVARCHAR(50) NULL;
PRINT 'Added Assets.LocationIdRef column';
GO

UPDATE Assets 
SET LocationIdRef = (SELECT LocationId FROM Locations WHERE Locations.Id = Assets.LocationId);
PRINT 'Populated Assets.LocationIdRef';
GO

ALTER TABLE Assets ALTER COLUMN LocationIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set Assets.LocationIdRef to NOT NULL';
GO

-- Assets.AssignedUserId → Users.UserId (alternate key) - nullable
ALTER TABLE Assets ADD AssignedUserIdRef NVARCHAR(50) NULL;
PRINT 'Added Assets.AssignedUserIdRef column';
GO

UPDATE Assets 
SET AssignedUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = Assets.AssignedUserId)
WHERE Assets.AssignedUserId IS NOT NULL;
PRINT 'Populated Assets.AssignedUserIdRef';
GO

-- =====================================================
-- LOCATIONS TABLE
-- =====================================================
-- Locations.ProjectId → Projects.ProjectId (alternate key)
ALTER TABLE Locations ADD ProjectIdRef NVARCHAR(50) NULL;
PRINT 'Added Locations.ProjectIdRef column';
GO

UPDATE Locations 
SET ProjectIdRef = (SELECT ProjectId FROM Projects WHERE Projects.Id = Locations.ProjectId);
PRINT 'Populated Locations.ProjectIdRef';
GO

ALTER TABLE Locations ALTER COLUMN ProjectIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set Locations.ProjectIdRef to NOT NULL';
GO

-- =====================================================
-- USERPROJECTS TABLE
-- =====================================================
-- UserProjects.UserId → Users.UserId (alternate key)
ALTER TABLE UserProjects ADD UserIdRef NVARCHAR(50) NULL;
PRINT 'Added UserProjects.UserIdRef column';
GO

UPDATE UserProjects 
SET UserIdRef = (SELECT UserId FROM Users WHERE Users.Id = UserProjects.UserId);
PRINT 'Populated UserProjects.UserIdRef';
GO

ALTER TABLE UserProjects ALTER COLUMN UserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set UserProjects.UserIdRef to NOT NULL';
GO

-- UserProjects.ProjectId → Projects.ProjectId (alternate key)
ALTER TABLE UserProjects ADD ProjectIdRef NVARCHAR(50) NULL;
PRINT 'Added UserProjects.ProjectIdRef column';
GO

UPDATE UserProjects 
SET ProjectIdRef = (SELECT ProjectId FROM Projects WHERE Projects.Id = UserProjects.ProjectId);
PRINT 'Populated UserProjects.ProjectIdRef';
GO

ALTER TABLE UserProjects ALTER COLUMN ProjectIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set UserProjects.ProjectIdRef to NOT NULL';
GO

-- =====================================================
-- ROLEPERMISSIONS TABLE
-- =====================================================
-- RolePermissions.RoleId → Roles.RoleId (alternate key)
ALTER TABLE RolePermissions ADD RoleIdRef NVARCHAR(50) NULL;
PRINT 'Added RolePermissions.RoleIdRef column';
GO

UPDATE RolePermissions 
SET RoleIdRef = (SELECT RoleId FROM Roles WHERE Roles.Id = RolePermissions.RoleId);
PRINT 'Populated RolePermissions.RoleIdRef';
GO

ALTER TABLE RolePermissions ALTER COLUMN RoleIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RolePermissions.RoleIdRef to NOT NULL';
GO

-- RolePermissions.PermissionId → Permissions.PermissionId (alternate key)
ALTER TABLE RolePermissions ADD PermissionIdRef NVARCHAR(50) NULL;
PRINT 'Added RolePermissions.PermissionIdRef column';
GO

UPDATE RolePermissions 
SET PermissionIdRef = (SELECT PermissionId FROM Permissions WHERE Permissions.Id = RolePermissions.PermissionId);
PRINT 'Populated RolePermissions.PermissionIdRef';
GO

ALTER TABLE RolePermissions ALTER COLUMN PermissionIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RolePermissions.PermissionIdRef to NOT NULL';
GO

-- =====================================================
-- USERPROJECTPERMISSIONS TABLE
-- =====================================================
-- UserProjectPermissions.PermissionId → Permissions.PermissionId (alternate key)
ALTER TABLE UserProjectPermissions ADD PermissionIdRef NVARCHAR(50) NULL;
PRINT 'Added UserProjectPermissions.PermissionIdRef column';
GO

UPDATE UserProjectPermissions 
SET PermissionIdRef = (SELECT PermissionId FROM Permissions WHERE Permissions.Id = UserProjectPermissions.PermissionId);
PRINT 'Populated UserProjectPermissions.PermissionIdRef';
GO

ALTER TABLE UserProjectPermissions ALTER COLUMN PermissionIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set UserProjectPermissions.PermissionIdRef to NOT NULL';
GO

-- =====================================================
-- AUDITENTRIES TABLE
-- =====================================================
-- AuditEntries.UserId → Users.UserId (alternate key)
ALTER TABLE AuditEntries ADD UserIdRef NVARCHAR(50) NULL;
PRINT 'Added AuditEntries.UserIdRef column';
GO

UPDATE AuditEntries 
SET UserIdRef = (SELECT UserId FROM Users WHERE Users.Id = AuditEntries.UserId);
PRINT 'Populated AuditEntries.UserIdRef';
GO

ALTER TABLE AuditEntries ALTER COLUMN UserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set AuditEntries.UserIdRef to NOT NULL';
GO

-- =====================================================
-- RBAC TABLES
-- =====================================================
-- RbacRolePermissions.RoleId → RbacRoles.RbacRoleId (alternate key)
ALTER TABLE RbacRolePermissions ADD RoleIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacRolePermissions.RoleIdRef column';
GO

UPDATE RbacRolePermissions 
SET RoleIdRef = (SELECT RbacRoleId FROM RbacRoles WHERE RbacRoles.Id = RbacRolePermissions.RoleId);
PRINT 'Populated RbacRolePermissions.RoleIdRef';
GO

ALTER TABLE RbacRolePermissions ALTER COLUMN RoleIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacRolePermissions.RoleIdRef to NOT NULL';
GO

-- RbacRolePermissions.PermissionId → RbacPermissions.RbacPermissionId (alternate key)
ALTER TABLE RbacRolePermissions ADD PermissionIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacRolePermissions.PermissionIdRef column';
GO

UPDATE RbacRolePermissions 
SET PermissionIdRef = (SELECT RbacPermissionId FROM RbacPermissions WHERE RbacPermissions.Id = RbacRolePermissions.PermissionId);
PRINT 'Populated RbacRolePermissions.PermissionIdRef';
GO

ALTER TABLE RbacRolePermissions ALTER COLUMN PermissionIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacRolePermissions.PermissionIdRef to NOT NULL';
GO

-- RbacUserPermissions.UserId → Users.UserId (alternate key)
ALTER TABLE RbacUserPermissions ADD UserIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacUserPermissions.UserIdRef column';
GO

UPDATE RbacUserPermissions 
SET UserIdRef = (SELECT UserId FROM Users WHERE Users.Id = RbacUserPermissions.UserId);
PRINT 'Populated RbacUserPermissions.UserIdRef';
GO

ALTER TABLE RbacUserPermissions ALTER COLUMN UserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacUserPermissions.UserIdRef to NOT NULL';
GO

-- RbacUserPermissions.PermissionId → RbacPermissions.RbacPermissionId (alternate key)
ALTER TABLE RbacUserPermissions ADD PermissionIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacUserPermissions.PermissionIdRef column';
GO

UPDATE RbacUserPermissions 
SET PermissionIdRef = (SELECT RbacPermissionId FROM RbacPermissions WHERE RbacPermissions.Id = RbacUserPermissions.PermissionId);
PRINT 'Populated RbacUserPermissions.PermissionIdRef';
GO

ALTER TABLE RbacUserPermissions ALTER COLUMN PermissionIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacUserPermissions.PermissionIdRef to NOT NULL';
GO

-- RbacUserScope.UserId → Users.UserId (alternate key)
ALTER TABLE RbacUserScope ADD UserIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacUserScope.UserIdRef column';
GO

UPDATE RbacUserScope 
SET UserIdRef = (SELECT UserId FROM Users WHERE Users.Id = RbacUserScope.UserId);
PRINT 'Populated RbacUserScope.UserIdRef';
GO

ALTER TABLE RbacUserScope ALTER COLUMN UserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacUserScope.UserIdRef to NOT NULL';
GO

-- RbacUserScope.ProjectId → Projects.ProjectId (alternate key)
ALTER TABLE RbacUserScope ADD ProjectIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacUserScope.ProjectIdRef column';
GO

UPDATE RbacUserScope 
SET ProjectIdRef = (SELECT ProjectId FROM Projects WHERE Projects.Id = RbacUserScope.ProjectId);
PRINT 'Populated RbacUserScope.ProjectIdRef';
GO

ALTER TABLE RbacUserScope ALTER COLUMN ProjectIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacUserScope.ProjectIdRef to NOT NULL';
GO

PRINT '';
PRINT '=====================================================';
PRINT 'PHASE 2 COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'All new foreign key reference columns have been added and populated';
PRINT 'Next: Run Phase 3 to drop old foreign keys and create new ones';
GO
