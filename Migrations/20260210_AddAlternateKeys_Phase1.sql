-- Migration Phase 1: Add Alternate Key Columns
-- Date: 2026-02-10
-- Description: Adds alternate key columns to all tables and populates them

PRINT '=====================================================';
PRINT 'PHASE 1: ADD COLUMNS AND GENERATE IDS';
PRINT '=====================================================';

-- Users table (already added, just populate and add constraint)
UPDATE Users
SET UserId = 'USR' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE UserId IS NULL;
PRINT 'Generated User IDs';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_UserId')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT UQ_Users_UserId UNIQUE (UserId);
    PRINT 'Added unique constraint on Users.UserId';
END

-- Roles table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Roles') AND name = 'RoleId')
BEGIN
    ALTER TABLE Roles ADD RoleId NVARCHAR(50) NULL;
    PRINT 'Added RoleId column to Roles table';
END

UPDATE Roles
SET RoleId = 'ROL' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RoleId IS NULL;
PRINT 'Generated Role IDs';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Roles_RoleId')
BEGIN
    ALTER TABLE Roles ADD CONSTRAINT UQ_Roles_RoleId UNIQUE (RoleId);
    PRINT 'Added unique constraint on Roles.RoleId';
END

-- Permissions table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'PermissionId')
BEGIN
    ALTER TABLE Permissions ADD PermissionId NVARCHAR(50) NULL;
    PRINT 'Added PermissionId column to Permissions table';
END

UPDATE Permissions
SET PermissionId = 'PER' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE PermissionId IS NULL;
PRINT 'Generated Permission IDs';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Permissions_PermissionId')
BEGIN
    ALTER TABLE Permissions ADD CONSTRAINT UQ_Permissions_PermissionId UNIQUE (PermissionId);
    PRINT 'Added unique constraint on Permissions.PermissionId';
END

-- Projects table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'ProjectId')
BEGIN
    ALTER TABLE Projects ADD ProjectId NVARCHAR(50) NULL;
    PRINT 'Added ProjectId column to Projects table';
END

UPDATE Projects
SET ProjectId = 'PRJ' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE ProjectId IS NULL;
PRINT 'Generated Project IDs';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Projects_ProjectId')
BEGIN
    ALTER TABLE Projects ADD CONSTRAINT UQ_Projects_ProjectId UNIQUE (ProjectId);
    PRINT 'Added unique constraint on Projects.ProjectId';
END

-- Locations table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Locations') AND name = 'LocationId')
BEGIN
    ALTER TABLE Locations ADD LocationId NVARCHAR(50) NULL;
    PRINT 'Added LocationId column to Locations table';
END

UPDATE Locations
SET LocationId = 'LOC' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE LocationId IS NULL;
PRINT 'Generated Location IDs';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Locations_LocationId')
BEGIN
    ALTER TABLE Locations ADD CONSTRAINT UQ_Locations_LocationId UNIQUE (LocationId);
    PRINT 'Added unique constraint on Locations.LocationId';
END

-- Assets table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'AssetId')
BEGIN
    ALTER TABLE Assets ADD AssetId NVARCHAR(50) NULL;
    PRINT 'Added AssetId column to Assets table';
END

UPDATE Assets
SET AssetId = 'AST' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE AssetId IS NULL;
PRINT 'Generated Asset IDs';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Assets_AssetId')
BEGIN
    ALTER TABLE Assets ADD CONSTRAINT UQ_Assets_AssetId UNIQUE (AssetId);
    PRINT 'Added unique constraint on Assets.AssetId';
END

-- RBAC tables
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacRoles') AND name = 'RbacRoleId')
BEGIN
    ALTER TABLE RbacRoles ADD RbacRoleId NVARCHAR(50) NULL;
    PRINT 'Added RbacRoleId column to RbacRoles table';
END

UPDATE RbacRoles
SET RbacRoleId = 'RBR' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RbacRoleId IS NULL;
PRINT 'Generated RBAC Role IDs';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_RbacRoles_RbacRoleId')
BEGIN
    ALTER TABLE RbacRoles ADD CONSTRAINT UQ_RbacRoles_RbacRoleId UNIQUE (RbacRoleId);
    PRINT 'Added unique constraint on RbacRoles.RbacRoleId';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacPermissions') AND name = 'RbacPermissionId')
BEGIN
    ALTER TABLE RbacPermissions ADD RbacPermissionId NVARCHAR(50) NULL;
    PRINT 'Added RbacPermissionId column to RbacPermissions table';
END

UPDATE RbacPermissions
SET RbacPermissionId = 'RBP' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RbacPermissionId IS NULL;
PRINT 'Generated RBAC Permission IDs';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_RbacPermissions_RbacPermissionId')
BEGIN
    ALTER TABLE RbacPermissions ADD CONSTRAINT UQ_RbacPermissions_RbacPermissionId UNIQUE (RbacPermissionId);
    PRINT 'Added unique constraint on RbacPermissions.RbacPermissionId';
END

PRINT '';
PRINT 'Phase 1 completed - All alternate key columns added and populated';
GO
