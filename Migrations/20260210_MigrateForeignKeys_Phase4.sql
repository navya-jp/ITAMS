-- Migration Phase 4: Migrate Remaining Foreign Keys to Use Alternate Keys
-- Date: 2026-02-10
-- Description: Migrates audit columns and remaining tables to use alternate keys

PRINT '=====================================================';
PRINT 'PHASE 4: MIGRATE REMAINING TABLES';
PRINT '=====================================================';

-- =====================================================
-- AUDIT COLUMNS IN MASTER TABLES
-- =====================================================

-- Users table audit columns
ALTER TABLE Users ADD CreatedByRef NVARCHAR(50) NULL;
PRINT 'Added Users.CreatedByRef column';
GO

UPDATE Users 
SET CreatedByRef = (SELECT UserId FROM Users u WHERE u.Id = Users.CreatedBy)
WHERE CreatedBy IS NOT NULL;
PRINT 'Populated Users.CreatedByRef';
GO

ALTER TABLE Users ADD DeactivatedByRef NVARCHAR(50) NULL;
PRINT 'Added Users.DeactivatedByRef column';
GO

UPDATE Users 
SET DeactivatedByRef = (SELECT UserId FROM Users u WHERE u.Id = Users.DeactivatedBy)
WHERE DeactivatedBy IS NOT NULL;
PRINT 'Populated Users.DeactivatedByRef';
GO

ALTER TABLE Users ADD DeactivatedByUserIdRef NVARCHAR(50) NULL;
PRINT 'Added Users.DeactivatedByUserIdRef column';
GO

UPDATE Users 
SET DeactivatedByUserIdRef = (SELECT UserId FROM Users u WHERE u.Id = Users.DeactivatedByUserId)
WHERE DeactivatedByUserId IS NOT NULL;
PRINT 'Populated Users.DeactivatedByUserIdRef';
GO

-- Roles table audit columns
ALTER TABLE Roles ADD CreatedByRef NVARCHAR(50) NULL;
PRINT 'Added Roles.CreatedByRef column';
GO

UPDATE Roles 
SET CreatedByRef = (SELECT UserId FROM Users WHERE Users.Id = Roles.CreatedBy)
WHERE CreatedBy IS NOT NULL;
PRINT 'Populated Roles.CreatedByRef';
GO

ALTER TABLE Roles ADD DeactivatedByUserIdRef NVARCHAR(50) NULL;
PRINT 'Added Roles.DeactivatedByUserIdRef column';
GO

UPDATE Roles 
SET DeactivatedByUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = Roles.DeactivatedByUserId)
WHERE DeactivatedByUserId IS NOT NULL;
PRINT 'Populated Roles.DeactivatedByUserIdRef';
GO

-- RbacRoles table audit columns
ALTER TABLE RbacRoles ADD CreatedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacRoles.CreatedByRef column';
GO

UPDATE RbacRoles 
SET CreatedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacRoles.CreatedBy);
PRINT 'Populated RbacRoles.CreatedByRef';
GO

ALTER TABLE RbacRoles ALTER COLUMN CreatedByRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacRoles.CreatedByRef to NOT NULL';
GO

ALTER TABLE RbacRoles ADD DeactivatedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacRoles.DeactivatedByRef column';
GO

UPDATE RbacRoles 
SET DeactivatedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacRoles.DeactivatedBy)
WHERE DeactivatedBy IS NOT NULL;
PRINT 'Populated RbacRoles.DeactivatedByRef';
GO

-- RbacPermissions table audit columns
ALTER TABLE RbacPermissions ADD CreatedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacPermissions.CreatedByRef column';
GO

UPDATE RbacPermissions 
SET CreatedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacPermissions.CreatedBy);
PRINT 'Populated RbacPermissions.CreatedByRef';
GO

ALTER TABLE RbacPermissions ALTER COLUMN CreatedByRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacPermissions.CreatedByRef to NOT NULL';
GO

ALTER TABLE RbacPermissions ADD DeactivatedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacPermissions.DeactivatedByRef column';
GO

UPDATE RbacPermissions 
SET DeactivatedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacPermissions.DeactivatedBy)
WHERE DeactivatedBy IS NOT NULL;
PRINT 'Populated RbacPermissions.DeactivatedByRef';
GO

-- RbacRolePermissions table audit columns
ALTER TABLE RbacRolePermissions ADD GrantedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacRolePermissions.GrantedByRef column';
GO

UPDATE RbacRolePermissions 
SET GrantedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacRolePermissions.GrantedBy);
PRINT 'Populated RbacRolePermissions.GrantedByRef';
GO

ALTER TABLE RbacRolePermissions ALTER COLUMN GrantedByRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacRolePermissions.GrantedByRef to NOT NULL';
GO

ALTER TABLE RbacRolePermissions ADD RevokedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacRolePermissions.RevokedByRef column';
GO

UPDATE RbacRolePermissions 
SET RevokedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacRolePermissions.RevokedBy)
WHERE RevokedBy IS NOT NULL;
PRINT 'Populated RbacRolePermissions.RevokedByRef';
GO

-- RbacUserPermissions table audit columns
ALTER TABLE RbacUserPermissions ADD GrantedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacUserPermissions.GrantedByRef column';
GO

UPDATE RbacUserPermissions 
SET GrantedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacUserPermissions.GrantedBy)
WHERE GrantedBy IS NOT NULL;
PRINT 'Populated RbacUserPermissions.GrantedByRef';
GO

ALTER TABLE RbacUserPermissions ADD RevokedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacUserPermissions.RevokedByRef column';
GO

UPDATE RbacUserPermissions 
SET RevokedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacUserPermissions.RevokedBy)
WHERE RevokedBy IS NOT NULL;
PRINT 'Populated RbacUserPermissions.RevokedByRef';
GO

-- RbacUserScope table audit columns
ALTER TABLE RbacUserScope ADD AssignedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacUserScope.AssignedByRef column';
GO

UPDATE RbacUserScope 
SET AssignedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacUserScope.AssignedBy);
PRINT 'Populated RbacUserScope.AssignedByRef';
GO

ALTER TABLE RbacUserScope ALTER COLUMN AssignedByRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacUserScope.AssignedByRef to NOT NULL';
GO

ALTER TABLE RbacUserScope ADD RemovedByRef NVARCHAR(50) NULL;
PRINT 'Added RbacUserScope.RemovedByRef column';
GO

UPDATE RbacUserScope 
SET RemovedByRef = (SELECT UserId FROM Users WHERE Users.Id = RbacUserScope.RemovedBy)
WHERE RemovedBy IS NOT NULL;
PRINT 'Populated RbacUserScope.RemovedByRef';
GO

-- =====================================================
-- USERPERMISSIONS TABLE
-- =====================================================
ALTER TABLE UserPermissions ADD UserIdRef NVARCHAR(50) NULL;
PRINT 'Added UserPermissions.UserIdRef column';
GO

UPDATE UserPermissions 
SET UserIdRef = (SELECT UserId FROM Users WHERE Users.Id = UserPermissions.UserId);
PRINT 'Populated UserPermissions.UserIdRef';
GO

ALTER TABLE UserPermissions ALTER COLUMN UserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set UserPermissions.UserIdRef to NOT NULL';
GO

ALTER TABLE UserPermissions ADD ProjectIdRef NVARCHAR(50) NULL;
PRINT 'Added UserPermissions.ProjectIdRef column';
GO

UPDATE UserPermissions 
SET ProjectIdRef = (SELECT ProjectId FROM Projects WHERE Projects.Id = UserPermissions.ProjectId);
PRINT 'Populated UserPermissions.ProjectIdRef';
GO

ALTER TABLE UserPermissions ALTER COLUMN ProjectIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set UserPermissions.ProjectIdRef to NOT NULL';
GO

ALTER TABLE UserPermissions ADD PermissionIdRef NVARCHAR(50) NULL;
PRINT 'Added UserPermissions.PermissionIdRef column';
GO

UPDATE UserPermissions 
SET PermissionIdRef = (SELECT PermissionId FROM Permissions WHERE Permissions.Id = UserPermissions.PermissionId);
PRINT 'Populated UserPermissions.PermissionIdRef';
GO

ALTER TABLE UserPermissions ALTER COLUMN PermissionIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set UserPermissions.PermissionIdRef to NOT NULL';
GO

ALTER TABLE UserPermissions ADD GrantedByUserIdRef NVARCHAR(50) NULL;
PRINT 'Added UserPermissions.GrantedByUserIdRef column';
GO

UPDATE UserPermissions 
SET GrantedByUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = UserPermissions.GrantedByUserId)
WHERE GrantedByUserId IS NOT NULL;
PRINT 'Populated UserPermissions.GrantedByUserIdRef';
GO

-- =====================================================
-- USERSCOPES TABLE
-- =====================================================
ALTER TABLE UserScopes ADD UserIdRef NVARCHAR(50) NULL;
PRINT 'Added UserScopes.UserIdRef column';
GO

UPDATE UserScopes 
SET UserIdRef = (SELECT UserId FROM Users WHERE Users.Id = UserScopes.UserId);
PRINT 'Populated UserScopes.UserIdRef';
GO

ALTER TABLE UserScopes ALTER COLUMN UserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set UserScopes.UserIdRef to NOT NULL';
GO

ALTER TABLE UserScopes ADD ProjectIdRef NVARCHAR(50) NULL;
PRINT 'Added UserScopes.ProjectIdRef column';
GO

UPDATE UserScopes 
SET ProjectIdRef = (SELECT ProjectId FROM Projects WHERE Projects.Id = UserScopes.ProjectId)
WHERE ProjectId IS NOT NULL;
PRINT 'Populated UserScopes.ProjectIdRef';
GO

ALTER TABLE UserScopes ADD AssignedByUserIdRef NVARCHAR(50) NULL;
PRINT 'Added UserScopes.AssignedByUserIdRef column';
GO

UPDATE UserScopes 
SET AssignedByUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = UserScopes.AssignedByUserId)
WHERE AssignedByUserId IS NOT NULL;
PRINT 'Populated UserScopes.AssignedByUserIdRef';
GO

-- =====================================================
-- AUDIT LOG TABLES
-- =====================================================

-- AuditLogs table
ALTER TABLE AuditLogs ADD ActorIdRef NVARCHAR(50) NULL;
PRINT 'Added AuditLogs.ActorIdRef column';
GO

UPDATE AuditLogs 
SET ActorIdRef = (SELECT UserId FROM Users WHERE Users.Id = AuditLogs.ActorId);
PRINT 'Populated AuditLogs.ActorIdRef';
GO

ALTER TABLE AuditLogs ALTER COLUMN ActorIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set AuditLogs.ActorIdRef to NOT NULL';
GO

-- RbacAccessAuditLog table
ALTER TABLE RbacAccessAuditLog ADD UserIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacAccessAuditLog.UserIdRef column';
GO

UPDATE RbacAccessAuditLog 
SET UserIdRef = (SELECT UserId FROM Users WHERE Users.Id = RbacAccessAuditLog.UserId);
PRINT 'Populated RbacAccessAuditLog.UserIdRef';
GO

ALTER TABLE RbacAccessAuditLog ALTER COLUMN UserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacAccessAuditLog.UserIdRef to NOT NULL';
GO

-- RbacPermissionAuditLog table
ALTER TABLE RbacPermissionAuditLog ADD ActorUserIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacPermissionAuditLog.ActorUserIdRef column';
GO

UPDATE RbacPermissionAuditLog 
SET ActorUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = RbacPermissionAuditLog.ActorUserId);
PRINT 'Populated RbacPermissionAuditLog.ActorUserIdRef';
GO

ALTER TABLE RbacPermissionAuditLog ALTER COLUMN ActorUserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set RbacPermissionAuditLog.ActorUserIdRef to NOT NULL';
GO

ALTER TABLE RbacPermissionAuditLog ADD TargetUserIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacPermissionAuditLog.TargetUserIdRef column';
GO

UPDATE RbacPermissionAuditLog 
SET TargetUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = RbacPermissionAuditLog.TargetUserId)
WHERE TargetUserId IS NOT NULL;
PRINT 'Populated RbacPermissionAuditLog.TargetUserIdRef';
GO

ALTER TABLE RbacPermissionAuditLog ADD RoleIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacPermissionAuditLog.RoleIdRef column';
GO

UPDATE RbacPermissionAuditLog 
SET RoleIdRef = (SELECT RbacRoleId FROM RbacRoles WHERE RbacRoles.Id = RbacPermissionAuditLog.RoleId)
WHERE RoleId IS NOT NULL;
PRINT 'Populated RbacPermissionAuditLog.RoleIdRef';
GO

ALTER TABLE RbacPermissionAuditLog ADD PermissionIdRef NVARCHAR(50) NULL;
PRINT 'Added RbacPermissionAuditLog.PermissionIdRef column';
GO

UPDATE RbacPermissionAuditLog 
SET PermissionIdRef = (SELECT RbacPermissionId FROM RbacPermissions WHERE RbacPermissions.Id = RbacPermissionAuditLog.PermissionId)
WHERE PermissionId IS NOT NULL;
PRINT 'Populated RbacPermissionAuditLog.PermissionIdRef';
GO

-- SecurityAlerts table
ALTER TABLE SecurityAlerts ADD UserIdRef NVARCHAR(50) NULL;
PRINT 'Added SecurityAlerts.UserIdRef column';
GO

UPDATE SecurityAlerts 
SET UserIdRef = (SELECT UserId FROM Users WHERE Users.Id = SecurityAlerts.UserId);
PRINT 'Populated SecurityAlerts.UserIdRef';
GO

ALTER TABLE SecurityAlerts ALTER COLUMN UserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set SecurityAlerts.UserIdRef to NOT NULL';
GO

ALTER TABLE SecurityAlerts ADD AssignedToUserIdRef NVARCHAR(50) NULL;
PRINT 'Added SecurityAlerts.AssignedToUserIdRef column';
GO

UPDATE SecurityAlerts 
SET AssignedToUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = SecurityAlerts.AssignedToUserId)
WHERE AssignedToUserId IS NOT NULL;
PRINT 'Populated SecurityAlerts.AssignedToUserIdRef';
GO

-- SecurityAuditLogs table
ALTER TABLE SecurityAuditLogs ADD ActorUserIdRef NVARCHAR(50) NULL;
PRINT 'Added SecurityAuditLogs.ActorUserIdRef column';
GO

UPDATE SecurityAuditLogs 
SET ActorUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = SecurityAuditLogs.ActorUserId);
PRINT 'Populated SecurityAuditLogs.ActorUserIdRef';
GO

ALTER TABLE SecurityAuditLogs ALTER COLUMN ActorUserIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set SecurityAuditLogs.ActorUserIdRef to NOT NULL';
GO

ALTER TABLE SecurityAuditLogs ADD TargetUserIdRef NVARCHAR(50) NULL;
PRINT 'Added SecurityAuditLogs.TargetUserIdRef column';
GO

UPDATE SecurityAuditLogs 
SET TargetUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = SecurityAuditLogs.TargetUserId)
WHERE TargetUserId IS NOT NULL;
PRINT 'Populated SecurityAuditLogs.TargetUserIdRef';
GO

PRINT '';
PRINT '=====================================================';
PRINT 'PHASE 4 PART 1 COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'All reference columns added and populated';
PRINT 'Next: Run Phase 4 Part 2 to drop old FKs and create new ones';
GO
