-- Migration: Add Alternate Keys - Simple Version
-- Date: 2026-02-10
-- Description: Adds alternate key columns to all tables

PRINT '=====================================================';
PRINT 'ADDING ALTERNATE KEY COLUMNS';
PRINT '=====================================================';

-- Roles table
ALTER TABLE Roles ADD RoleId NVARCHAR(50) NULL;
PRINT 'Added RoleId column';
GO

UPDATE Roles SET RoleId = 'ROL' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated Role IDs';
GO

ALTER TABLE Roles ADD CONSTRAINT UQ_Roles_RoleId UNIQUE (RoleId);
PRINT 'Added unique constraint on Roles.RoleId';
GO

-- Permissions table
ALTER TABLE Permissions ADD PermissionId NVARCHAR(50) NULL;
PRINT 'Added PermissionId column';
GO

UPDATE Permissions SET PermissionId = 'PER' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated Permission IDs';
GO

ALTER TABLE Permissions ADD CONSTRAINT UQ_Permissions_PermissionId UNIQUE (PermissionId);
PRINT 'Added unique constraint on Permissions.PermissionId';
GO

-- Projects table
ALTER TABLE Projects ADD ProjectId NVARCHAR(50) NULL;
PRINT 'Added ProjectId column';
GO

UPDATE Projects SET ProjectId = 'PRJ' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated Project IDs';
GO

ALTER TABLE Projects ADD CONSTRAINT UQ_Projects_ProjectId UNIQUE (ProjectId);
PRINT 'Added unique constraint on Projects.ProjectId';
GO

-- Locations table
ALTER TABLE Locations ADD LocationId NVARCHAR(50) NULL;
PRINT 'Added LocationId column';
GO

UPDATE Locations SET LocationId = 'LOC' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated Location IDs';
GO

ALTER TABLE Locations ADD CONSTRAINT UQ_Locations_LocationId UNIQUE (LocationId);
PRINT 'Added unique constraint on Locations.LocationId';
GO

-- Assets table
ALTER TABLE Assets ADD AssetId NVARCHAR(50) NULL;
PRINT 'Added AssetId column';
GO

UPDATE Assets SET AssetId = 'AST' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated Asset IDs';
GO

ALTER TABLE Assets ADD CONSTRAINT UQ_Assets_AssetId UNIQUE (AssetId);
PRINT 'Added unique constraint on Assets.AssetId';
GO

-- RBAC Roles table
ALTER TABLE RbacRoles ADD RbacRoleId NVARCHAR(50) NULL;
PRINT 'Added RbacRoleId column';
GO

UPDATE RbacRoles SET RbacRoleId = 'RBR' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated RBAC Role IDs';
GO

ALTER TABLE RbacRoles ADD CONSTRAINT UQ_RbacRoles_RbacRoleId UNIQUE (RbacRoleId);
PRINT 'Added unique constraint on RbacRoles.RbacRoleId';
GO

-- RBAC Permissions table
ALTER TABLE RbacPermissions ADD RbacPermissionId NVARCHAR(50) NULL;
PRINT 'Added RbacPermissionId column';
GO

UPDATE RbacPermissions SET RbacPermissionId = 'RBP' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated RBAC Permission IDs';
GO

ALTER TABLE RbacPermissions ADD CONSTRAINT UQ_RbacPermissions_RbacPermissionId UNIQUE (RbacPermissionId);
PRINT 'Added unique constraint on RbacPermissions.RbacPermissionId';
GO

-- Make all columns NOT NULL
ALTER TABLE Users ALTER COLUMN UserId NVARCHAR(50) NOT NULL;
ALTER TABLE Roles ALTER COLUMN RoleId NVARCHAR(50) NOT NULL;
ALTER TABLE Permissions ALTER COLUMN PermissionId NVARCHAR(50) NOT NULL;
ALTER TABLE Projects ALTER COLUMN ProjectId NVARCHAR(50) NOT NULL;
ALTER TABLE Locations ALTER COLUMN LocationId NVARCHAR(50) NOT NULL;
ALTER TABLE Assets ALTER COLUMN AssetId NVARCHAR(50) NOT NULL;
ALTER TABLE RbacRoles ALTER COLUMN RbacRoleId NVARCHAR(50) NOT NULL;
ALTER TABLE RbacPermissions ALTER COLUMN RbacPermissionId NVARCHAR(50) NOT NULL;
GO

PRINT '';
PRINT '=====================================================';
PRINT 'MIGRATION COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'All tables now have alternate key columns with sequential IDs';
GO
