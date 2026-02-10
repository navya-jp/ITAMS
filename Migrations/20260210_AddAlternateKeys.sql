-- Migration: Add Alternate Keys to All Tables
-- Date: 2026-02-10
-- Description: Adds sequential business IDs as alternate keys to replace primary key references

PRINT '=====================================================';
PRINT 'STEP 1: ADD ALTERNATE KEY COLUMNS';
PRINT '=====================================================';

-- Users table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'UserId')
BEGIN
    ALTER TABLE Users ADD UserId NVARCHAR(50) NULL;
    PRINT 'Added UserId column to Users table';
END

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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_RbacPermissions_RbacPermissionId')
BEGIN
    ALTER TABLE RbacPermissions ADD CONSTRAINT UQ_RbacPermissions_RbacPermissionId UNIQUE (RbacPermissionId);
    PRINT 'Added unique constraint on RbacPermissions.RbacPermissionId';
END

PRINT '';
PRINT '=====================================================';
PRINT 'STEP 2: GENERATE SEQUENTIAL IDS FOR EXISTING DATA';
PRINT '=====================================================';

-- Generate User IDs (USR00001, USR00002, etc.)
UPDATE Users
SET UserId = 'USR' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE UserId IS NULL;
PRINT 'Generated User IDs';

-- Generate Role IDs (ROL00001, ROL00002, etc.)
UPDATE Roles
SET RoleId = 'ROL' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RoleId IS NULL;
PRINT 'Generated Role IDs';

-- Generate Permission IDs (PER00001, PER00002, etc.)
UPDATE Permissions
SET PermissionId = 'PER' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE PermissionId IS NULL;
PRINT 'Generated Permission IDs';

-- Generate Project IDs (PRJ00001, PRJ00002, etc.)
UPDATE Projects
SET ProjectId = 'PRJ' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE ProjectId IS NULL;
PRINT 'Generated Project IDs';

-- Generate Location IDs (LOC00001, LOC00002, etc.)
UPDATE Locations
SET LocationId = 'LOC' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE LocationId IS NULL;
PRINT 'Generated Location IDs';

-- Generate Asset IDs (AST00001, AST00002, etc.)
UPDATE Assets
SET AssetId = 'AST' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE AssetId IS NULL;
PRINT 'Generated Asset IDs';

-- Generate RBAC Role IDs (RBR00001, RBR00002, etc.)
UPDATE RbacRoles
SET RbacRoleId = 'RBR' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RbacRoleId IS NULL;
PRINT 'Generated RBAC Role IDs';

-- Generate RBAC Permission IDs (RBP00001, RBP00002, etc.)
UPDATE RbacPermissions
SET RbacPermissionId = 'RBP' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RbacPermissionId IS NULL;
PRINT 'Generated RBAC Permission IDs';

PRINT '';
PRINT '=====================================================';
PRINT 'STEP 3: MAKE ALTERNATE KEYS NOT NULL';
PRINT '=====================================================';

ALTER TABLE Users ALTER COLUMN UserId NVARCHAR(50) NOT NULL;
PRINT 'Users.UserId set to NOT NULL';

ALTER TABLE Roles ALTER COLUMN RoleId NVARCHAR(50) NOT NULL;
PRINT 'Roles.RoleId set to NOT NULL';

ALTER TABLE Permissions ALTER COLUMN PermissionId NVARCHAR(50) NOT NULL;
PRINT 'Permissions.PermissionId set to NOT NULL';

ALTER TABLE Projects ALTER COLUMN ProjectId NVARCHAR(50) NOT NULL;
PRINT 'Projects.ProjectId set to NOT NULL';

ALTER TABLE Locations ALTER COLUMN LocationId NVARCHAR(50) NOT NULL;
PRINT 'Locations.LocationId set to NOT NULL';

ALTER TABLE Assets ALTER COLUMN AssetId NVARCHAR(50) NOT NULL;
PRINT 'Assets.AssetId set to NOT NULL';

ALTER TABLE RbacRoles ALTER COLUMN RbacRoleId NVARCHAR(50) NOT NULL;
PRINT 'RbacRoles.RbacRoleId set to NOT NULL';

ALTER TABLE RbacPermissions ALTER COLUMN RbacPermissionId NVARCHAR(50) NOT NULL;
PRINT 'RbacPermissions.RbacPermissionId set to NOT NULL';

PRINT '';
PRINT '=====================================================';
PRINT 'MIGRATION PHASE 1 COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'All tables now have alternate key columns with sequential IDs';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Update Entity Framework models to include alternate keys';
PRINT '2. Update DTOs to use alternate keys';
PRINT '3. Update controllers and services';
PRINT '4. Run Phase 2 migration to update foreign key relationships';
