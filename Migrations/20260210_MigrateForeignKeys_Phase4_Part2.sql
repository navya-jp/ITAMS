-- Migration Phase 4 Part 2: Drop Old FKs and Create New Ones for Remaining Tables
-- Date: 2026-02-10
-- Description: Drops old foreign keys and creates new ones that reference alternate keys

PRINT '=====================================================';
PRINT 'PHASE 4 PART 2: DROP OLD FOREIGN KEYS';
PRINT '=====================================================';

-- Drop FKs from Users table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    ALTER TABLE Users DROP CONSTRAINT FK_Users_CreatedBy;
PRINT 'Dropped FK_Users_CreatedBy';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_DeactivatedBy')
    ALTER TABLE Users DROP CONSTRAINT FK_Users_DeactivatedBy;
PRINT 'Dropped FK_Users_DeactivatedBy';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Users_DeactivatedByUserId')
    ALTER TABLE Users DROP CONSTRAINT FK_Users_Users_DeactivatedByUserId;
PRINT 'Dropped FK_Users_Users_DeactivatedByUserId';
GO

-- Drop FKs from Roles table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Roles_CreatedBy')
    ALTER TABLE Roles DROP CONSTRAINT FK_Roles_CreatedBy;
PRINT 'Dropped FK_Roles_CreatedBy';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Roles_Users_DeactivatedByUserId')
    ALTER TABLE Roles DROP CONSTRAINT FK_Roles_Users_DeactivatedByUserId;
PRINT 'Dropped FK_Roles_Users_DeactivatedByUserId';
GO

-- Drop FKs from RbacRoles table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacRoles_CreatedBy')
    ALTER TABLE RbacRoles DROP CONSTRAINT FK_RbacRoles_CreatedBy;
PRINT 'Dropped FK_RbacRoles_CreatedBy';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacRoles_DeactivatedBy')
    ALTER TABLE RbacRoles DROP CONSTRAINT FK_RbacRoles_DeactivatedBy;
PRINT 'Dropped FK_RbacRoles_DeactivatedBy';
GO

-- Drop FKs from RbacPermissions table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacPermissions_CreatedBy')
    ALTER TABLE RbacPermissions DROP CONSTRAINT FK_RbacPermissions_CreatedBy;
PRINT 'Dropped FK_RbacPermissions_CreatedBy';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacPermissions_DeactivatedBy')
    ALTER TABLE RbacPermissions DROP CONSTRAINT FK_RbacPermissions_DeactivatedBy;
PRINT 'Dropped FK_RbacPermissions_DeactivatedBy';
GO

-- Drop FKs from RbacRolePermissions table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacRolePermissions_GrantedBy')
    ALTER TABLE RbacRolePermissions DROP CONSTRAINT FK_RbacRolePermissions_GrantedBy;
PRINT 'Dropped FK_RbacRolePermissions_GrantedBy';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacRolePermissions_RevokedBy')
    ALTER TABLE RbacRolePermissions DROP CONSTRAINT FK_RbacRolePermissions_RevokedBy;
PRINT 'Dropped FK_RbacRolePermissions_RevokedBy';
GO

-- Drop FKs from RbacUserPermissions table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacUserPermissions_GrantedBy')
    ALTER TABLE RbacUserPermissions DROP CONSTRAINT FK_RbacUserPermissions_GrantedBy;
PRINT 'Dropped FK_RbacUserPermissions_GrantedBy';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacUserPermissions_RevokedBy')
    ALTER TABLE RbacUserPermissions DROP CONSTRAINT FK_RbacUserPermissions_RevokedBy;
PRINT 'Dropped FK_RbacUserPermissions_RevokedBy';
GO

-- Drop FKs from RbacUserScope table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacUserScope_AssignedBy')
    ALTER TABLE RbacUserScope DROP CONSTRAINT FK_RbacUserScope_AssignedBy;
PRINT 'Dropped FK_RbacUserScope_AssignedBy';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacUserScope_RemovedBy')
    ALTER TABLE RbacUserScope DROP CONSTRAINT FK_RbacUserScope_RemovedBy;
PRINT 'Dropped FK_RbacUserScope_RemovedBy';
GO

-- Drop FKs from UserPermissions table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_Users_UserId')
    ALTER TABLE UserPermissions DROP CONSTRAINT FK_UserPermissions_Users_UserId;
PRINT 'Dropped FK_UserPermissions_Users_UserId';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_Projects_ProjectId')
    ALTER TABLE UserPermissions DROP CONSTRAINT FK_UserPermissions_Projects_ProjectId;
PRINT 'Dropped FK_UserPermissions_Projects_ProjectId';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_Permissions_PermissionId')
    ALTER TABLE UserPermissions DROP CONSTRAINT FK_UserPermissions_Permissions_PermissionId;
PRINT 'Dropped FK_UserPermissions_Permissions_PermissionId';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_Users_GrantedByUserId')
    ALTER TABLE UserPermissions DROP CONSTRAINT FK_UserPermissions_Users_GrantedByUserId;
PRINT 'Dropped FK_UserPermissions_Users_GrantedByUserId';
GO

-- Drop FKs from UserScopes table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserScopes_Users_UserId')
    ALTER TABLE UserScopes DROP CONSTRAINT FK_UserScopes_Users_UserId;
PRINT 'Dropped FK_UserScopes_Users_UserId';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserScopes_Projects_ProjectId')
    ALTER TABLE UserScopes DROP CONSTRAINT FK_UserScopes_Projects_ProjectId;
PRINT 'Dropped FK_UserScopes_Projects_ProjectId';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserScopes_Users_AssignedByUserId')
    ALTER TABLE UserScopes DROP CONSTRAINT FK_UserScopes_Users_AssignedByUserId;
PRINT 'Dropped FK_UserScopes_Users_AssignedByUserId';
GO

-- Drop FKs from AuditLogs table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AuditLogs_ActorId')
    ALTER TABLE AuditLogs DROP CONSTRAINT FK_AuditLogs_ActorId;
PRINT 'Dropped FK_AuditLogs_ActorId';
GO

-- Drop FKs from RbacAccessAuditLog table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacAccessAuditLog_User')
    ALTER TABLE RbacAccessAuditLog DROP CONSTRAINT FK_RbacAccessAuditLog_User;
PRINT 'Dropped FK_RbacAccessAuditLog_User';
GO

-- Drop FKs from RbacPermissionAuditLog table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacPermissionAuditLog_Actor')
    ALTER TABLE RbacPermissionAuditLog DROP CONSTRAINT FK_RbacPermissionAuditLog_Actor;
PRINT 'Dropped FK_RbacPermissionAuditLog_Actor';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacPermissionAuditLog_Target')
    ALTER TABLE RbacPermissionAuditLog DROP CONSTRAINT FK_RbacPermissionAuditLog_Target;
PRINT 'Dropped FK_RbacPermissionAuditLog_Target';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacPermissionAuditLog_Role')
    ALTER TABLE RbacPermissionAuditLog DROP CONSTRAINT FK_RbacPermissionAuditLog_Role;
PRINT 'Dropped FK_RbacPermissionAuditLog_Role';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RbacPermissionAuditLog_Permission')
    ALTER TABLE RbacPermissionAuditLog DROP CONSTRAINT FK_RbacPermissionAuditLog_Permission;
PRINT 'Dropped FK_RbacPermissionAuditLog_Permission';
GO

-- Drop FKs from SecurityAlerts table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SecurityAlerts_Users_UserId')
    ALTER TABLE SecurityAlerts DROP CONSTRAINT FK_SecurityAlerts_Users_UserId;
PRINT 'Dropped FK_SecurityAlerts_Users_UserId';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SecurityAlerts_Users_AssignedToUserId')
    ALTER TABLE SecurityAlerts DROP CONSTRAINT FK_SecurityAlerts_Users_AssignedToUserId;
PRINT 'Dropped FK_SecurityAlerts_Users_AssignedToUserId';
GO

-- Drop FKs from SecurityAuditLogs table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SecurityAuditLogs_Users_ActorUserId')
    ALTER TABLE SecurityAuditLogs DROP CONSTRAINT FK_SecurityAuditLogs_Users_ActorUserId;
PRINT 'Dropped FK_SecurityAuditLogs_Users_ActorUserId';
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SecurityAuditLogs_Users_TargetUserId')
    ALTER TABLE SecurityAuditLogs DROP CONSTRAINT FK_SecurityAuditLogs_Users_TargetUserId;
PRINT 'Dropped FK_SecurityAuditLogs_Users_TargetUserId';
GO

PRINT '';
PRINT '=====================================================';
PRINT 'PHASE 4 PART 2: CREATE NEW FOREIGN KEYS';
PRINT '=====================================================';

-- Create new FKs for Users table
ALTER TABLE Users ADD CONSTRAINT FK_Users_Users_CreatedByRef
    FOREIGN KEY (CreatedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_Users_Users_CreatedByRef';
GO

ALTER TABLE Users ADD CONSTRAINT FK_Users_Users_DeactivatedByRef
    FOREIGN KEY (DeactivatedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_Users_Users_DeactivatedByRef';
GO

ALTER TABLE Users ADD CONSTRAINT FK_Users_Users_DeactivatedByUserIdRef
    FOREIGN KEY (DeactivatedByUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_Users_Users_DeactivatedByUserIdRef';
GO

-- Create new FKs for Roles table
ALTER TABLE Roles ADD CONSTRAINT FK_Roles_Users_CreatedByRef
    FOREIGN KEY (CreatedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_Roles_Users_CreatedByRef';
GO

ALTER TABLE Roles ADD CONSTRAINT FK_Roles_Users_DeactivatedByUserIdRef
    FOREIGN KEY (DeactivatedByUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_Roles_Users_DeactivatedByUserIdRef';
GO

-- Create new FKs for RbacRoles table
ALTER TABLE RbacRoles ADD CONSTRAINT FK_RbacRoles_Users_CreatedByRef
    FOREIGN KEY (CreatedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacRoles_Users_CreatedByRef';
GO

ALTER TABLE RbacRoles ADD CONSTRAINT FK_RbacRoles_Users_DeactivatedByRef
    FOREIGN KEY (DeactivatedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacRoles_Users_DeactivatedByRef';
GO

-- Create new FKs for RbacPermissions table
ALTER TABLE RbacPermissions ADD CONSTRAINT FK_RbacPermissions_Users_CreatedByRef
    FOREIGN KEY (CreatedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacPermissions_Users_CreatedByRef';
GO

ALTER TABLE RbacPermissions ADD CONSTRAINT FK_RbacPermissions_Users_DeactivatedByRef
    FOREIGN KEY (DeactivatedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacPermissions_Users_DeactivatedByRef';
GO

-- Create new FKs for RbacRolePermissions table
ALTER TABLE RbacRolePermissions ADD CONSTRAINT FK_RbacRolePermissions_Users_GrantedByRef
    FOREIGN KEY (GrantedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacRolePermissions_Users_GrantedByRef';
GO

ALTER TABLE RbacRolePermissions ADD CONSTRAINT FK_RbacRolePermissions_Users_RevokedByRef
    FOREIGN KEY (RevokedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacRolePermissions_Users_RevokedByRef';
GO

-- Create new FKs for RbacUserPermissions table
ALTER TABLE RbacUserPermissions ADD CONSTRAINT FK_RbacUserPermissions_Users_GrantedByRef
    FOREIGN KEY (GrantedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacUserPermissions_Users_GrantedByRef';
GO

ALTER TABLE RbacUserPermissions ADD CONSTRAINT FK_RbacUserPermissions_Users_RevokedByRef
    FOREIGN KEY (RevokedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacUserPermissions_Users_RevokedByRef';
GO

-- Create new FKs for RbacUserScope table
ALTER TABLE RbacUserScope ADD CONSTRAINT FK_RbacUserScope_Users_AssignedByRef
    FOREIGN KEY (AssignedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacUserScope_Users_AssignedByRef';
GO

ALTER TABLE RbacUserScope ADD CONSTRAINT FK_RbacUserScope_Users_RemovedByRef
    FOREIGN KEY (RemovedByRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacUserScope_Users_RemovedByRef';
GO

-- Create new FKs for UserPermissions table
ALTER TABLE UserPermissions ADD CONSTRAINT FK_UserPermissions_Users_UserIdRef
    FOREIGN KEY (UserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_UserPermissions_Users_UserIdRef';
GO

ALTER TABLE UserPermissions ADD CONSTRAINT FK_UserPermissions_Projects_ProjectIdRef
    FOREIGN KEY (ProjectIdRef) REFERENCES Projects(ProjectId);
PRINT 'Created FK_UserPermissions_Projects_ProjectIdRef';
GO

ALTER TABLE UserPermissions ADD CONSTRAINT FK_UserPermissions_Permissions_PermissionIdRef
    FOREIGN KEY (PermissionIdRef) REFERENCES Permissions(PermissionId);
PRINT 'Created FK_UserPermissions_Permissions_PermissionIdRef';
GO

ALTER TABLE UserPermissions ADD CONSTRAINT FK_UserPermissions_Users_GrantedByUserIdRef
    FOREIGN KEY (GrantedByUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_UserPermissions_Users_GrantedByUserIdRef';
GO

-- Create new FKs for UserScopes table
ALTER TABLE UserScopes ADD CONSTRAINT FK_UserScopes_Users_UserIdRef
    FOREIGN KEY (UserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_UserScopes_Users_UserIdRef';
GO

ALTER TABLE UserScopes ADD CONSTRAINT FK_UserScopes_Projects_ProjectIdRef
    FOREIGN KEY (ProjectIdRef) REFERENCES Projects(ProjectId);
PRINT 'Created FK_UserScopes_Projects_ProjectIdRef';
GO

ALTER TABLE UserScopes ADD CONSTRAINT FK_UserScopes_Users_AssignedByUserIdRef
    FOREIGN KEY (AssignedByUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_UserScopes_Users_AssignedByUserIdRef';
GO

-- Create new FKs for AuditLogs table
ALTER TABLE AuditLogs ADD CONSTRAINT FK_AuditLogs_Users_ActorIdRef
    FOREIGN KEY (ActorIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_AuditLogs_Users_ActorIdRef';
GO

-- Create new FKs for RbacAccessAuditLog table
ALTER TABLE RbacAccessAuditLog ADD CONSTRAINT FK_RbacAccessAuditLog_Users_UserIdRef
    FOREIGN KEY (UserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacAccessAuditLog_Users_UserIdRef';
GO

-- Create new FKs for RbacPermissionAuditLog table
ALTER TABLE RbacPermissionAuditLog ADD CONSTRAINT FK_RbacPermissionAuditLog_Users_ActorUserIdRef
    FOREIGN KEY (ActorUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacPermissionAuditLog_Users_ActorUserIdRef';
GO

ALTER TABLE RbacPermissionAuditLog ADD CONSTRAINT FK_RbacPermissionAuditLog_Users_TargetUserIdRef
    FOREIGN KEY (TargetUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_RbacPermissionAuditLog_Users_TargetUserIdRef';
GO

ALTER TABLE RbacPermissionAuditLog ADD CONSTRAINT FK_RbacPermissionAuditLog_RbacRoles_RoleIdRef
    FOREIGN KEY (RoleIdRef) REFERENCES RbacRoles(RbacRoleId);
PRINT 'Created FK_RbacPermissionAuditLog_RbacRoles_RoleIdRef';
GO

ALTER TABLE RbacPermissionAuditLog ADD CONSTRAINT FK_RbacPermissionAuditLog_RbacPermissions_PermissionIdRef
    FOREIGN KEY (PermissionIdRef) REFERENCES RbacPermissions(RbacPermissionId);
PRINT 'Created FK_RbacPermissionAuditLog_RbacPermissions_PermissionIdRef';
GO

-- Create new FKs for SecurityAlerts table
ALTER TABLE SecurityAlerts ADD CONSTRAINT FK_SecurityAlerts_Users_UserIdRef
    FOREIGN KEY (UserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_SecurityAlerts_Users_UserIdRef';
GO

ALTER TABLE SecurityAlerts ADD CONSTRAINT FK_SecurityAlerts_Users_AssignedToUserIdRef
    FOREIGN KEY (AssignedToUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_SecurityAlerts_Users_AssignedToUserIdRef';
GO

-- Create new FKs for SecurityAuditLogs table
ALTER TABLE SecurityAuditLogs ADD CONSTRAINT FK_SecurityAuditLogs_Users_ActorUserIdRef
    FOREIGN KEY (ActorUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_SecurityAuditLogs_Users_ActorUserIdRef';
GO

ALTER TABLE SecurityAuditLogs ADD CONSTRAINT FK_SecurityAuditLogs_Users_TargetUserIdRef
    FOREIGN KEY (TargetUserIdRef) REFERENCES Users(UserId);
PRINT 'Created FK_SecurityAuditLogs_Users_TargetUserIdRef';
GO

PRINT '';
PRINT '=====================================================';
PRINT 'PHASE 4 PART 2 COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'All remaining foreign keys now reference alternate keys';
PRINT 'Migration is now COMPLETE!';
GO
