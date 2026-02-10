-- =============================================
-- Add Audit Columns to All Master Tables
-- Columns: IPAddress, UpdatedBy, UpdatedAt
-- =============================================

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IPAddress')
BEGIN
    ALTER TABLE Users ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to Users table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE Users ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to Users table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Users ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to Users table';
END
GO

-- Roles Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Roles') AND name = 'IPAddress')
BEGIN
    ALTER TABLE Roles ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to Roles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Roles') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE Roles ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to Roles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Roles') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Roles ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to Roles table';
END
GO

-- Permissions Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'IPAddress')
BEGIN
    ALTER TABLE Permissions ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to Permissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE Permissions ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to Permissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Permissions ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to Permissions table';
END
GO

-- Projects Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'IPAddress')
BEGIN
    ALTER TABLE Projects ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to Projects table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE Projects ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to Projects table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Projects ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to Projects table';
END
GO

-- Locations Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Locations') AND name = 'IPAddress')
BEGIN
    ALTER TABLE Locations ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to Locations table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Locations') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE Locations ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to Locations table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Locations') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Locations ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to Locations table';
END
GO

-- Assets Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'IPAddress')
BEGIN
    ALTER TABLE Assets ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to Assets table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE Assets ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to Assets table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Assets ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to Assets table';
END
GO

-- RbacRoles Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacRoles') AND name = 'IPAddress')
BEGIN
    ALTER TABLE RbacRoles ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to RbacRoles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacRoles') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE RbacRoles ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to RbacRoles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacRoles') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE RbacRoles ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to RbacRoles table';
END
GO

-- RbacPermissions Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacPermissions') AND name = 'IPAddress')
BEGIN
    ALTER TABLE RbacPermissions ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to RbacPermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacPermissions') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE RbacPermissions ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to RbacPermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacPermissions') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE RbacPermissions ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to RbacPermissions table';
END
GO

-- RbacRolePermissions Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacRolePermissions') AND name = 'IPAddress')
BEGIN
    ALTER TABLE RbacRolePermissions ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to RbacRolePermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacRolePermissions') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE RbacRolePermissions ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to RbacRolePermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacRolePermissions') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE RbacRolePermissions ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to RbacRolePermissions table';
END
GO

-- RbacUserPermissions Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacUserPermissions') AND name = 'IPAddress')
BEGIN
    ALTER TABLE RbacUserPermissions ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to RbacUserPermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacUserPermissions') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE RbacUserPermissions ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to RbacUserPermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacUserPermissions') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE RbacUserPermissions ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to RbacUserPermissions table';
END
GO

-- RbacUserScope Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacUserScope') AND name = 'IPAddress')
BEGIN
    ALTER TABLE RbacUserScope ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to RbacUserScope table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacUserScope') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE RbacUserScope ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to RbacUserScope table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RbacUserScope') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE RbacUserScope ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to RbacUserScope table';
END
GO

-- RolePermissions Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RolePermissions') AND name = 'IPAddress')
BEGIN
    ALTER TABLE RolePermissions ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to RolePermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RolePermissions') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE RolePermissions ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to RolePermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RolePermissions') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE RolePermissions ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to RolePermissions table';
END
GO

-- UserProjects Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProjects') AND name = 'IPAddress')
BEGIN
    ALTER TABLE UserProjects ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to UserProjects table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProjects') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE UserProjects ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to UserProjects table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProjects') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE UserProjects ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to UserProjects table';
END
GO

-- UserProjectPermissions Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProjectPermissions') AND name = 'IPAddress')
BEGIN
    ALTER TABLE UserProjectPermissions ADD IPAddress NVARCHAR(45) NULL;
    PRINT 'Added IPAddress to UserProjectPermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProjectPermissions') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE UserProjectPermissions ADD UpdatedBy INT NULL;
    PRINT 'Added UpdatedBy to UserProjectPermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProjectPermissions') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE UserProjectPermissions ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt to UserProjectPermissions table';
END
GO

PRINT '';
PRINT '=== Audit Columns Added Successfully ===';
PRINT 'All master tables now have IPAddress, UpdatedBy, and UpdatedAt columns';
GO
