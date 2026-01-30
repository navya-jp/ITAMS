-- Simplified RBAC System - Core Tables Migration
-- Created: 2026-01-30
-- Description: Creates the core RBAC tables for permission-driven access control

-- =====================================================
-- RBAC CORE TABLES
-- =====================================================

-- Check if RBAC tables already exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RbacRoles')
BEGIN
    -- Roles table (permission templates)
    CREATE TABLE RbacRoles (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        IsSystemRole BIT NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NOT NULL,
        DeactivatedAt DATETIME2 NULL,
        DeactivatedBy INT NULL,
        
        CONSTRAINT CHK_RbacRoles_Status CHECK (Status IN ('ACTIVE', 'INACTIVE')),
        CONSTRAINT FK_RbacRoles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT FK_RbacRoles_DeactivatedBy FOREIGN KEY (DeactivatedBy) REFERENCES Users(Id)
    );
    PRINT 'Created RbacRoles table';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RbacPermissions')
BEGIN
    -- Permissions table (atomic authorization units)
    CREATE TABLE RbacPermissions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Code NVARCHAR(100) NOT NULL UNIQUE,
        Module NVARCHAR(50) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        ResourceType NVARCHAR(50) NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy INT NOT NULL,
        DeactivatedAt DATETIME2 NULL,
        DeactivatedBy INT NULL,
        
        CONSTRAINT CHK_RbacPermissions_Status CHECK (Status IN ('ACTIVE', 'INACTIVE')),
        CONSTRAINT CHK_RbacPermissions_Code CHECK (Code LIKE '%[_]%' AND LEN(Code) >= 5),
        CONSTRAINT FK_RbacPermissions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT FK_RbacPermissions_DeactivatedBy FOREIGN KEY (DeactivatedBy) REFERENCES Users(Id)
    );
    PRINT 'Created RbacPermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RbacRolePermissions')
BEGIN
    -- Role default permissions matrix
    CREATE TABLE RbacRolePermissions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        Allowed BIT NOT NULL DEFAULT 1,
        GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        GrantedBy INT NOT NULL,
        RevokedAt DATETIME2 NULL,
        RevokedBy INT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        
        CONSTRAINT CHK_RbacRolePermissions_Status CHECK (Status IN ('ACTIVE', 'REVOKED')),
        CONSTRAINT FK_RbacRolePermissions_Role FOREIGN KEY (RoleId) REFERENCES RbacRoles(Id),
        CONSTRAINT FK_RbacRolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES RbacPermissions(Id),
        CONSTRAINT FK_RbacRolePermissions_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES Users(Id),
        CONSTRAINT FK_RbacRolePermissions_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES Users(Id),
        CONSTRAINT UQ_RbacRolePermissions UNIQUE (RoleId, PermissionId)
    );
    PRINT 'Created RbacRolePermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RbacUserPermissions')
BEGIN
    -- User-specific permission overrides
    CREATE TABLE RbacUserPermissions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        PermissionId INT NOT NULL,
        Allowed BIT NOT NULL,
        GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        GrantedBy INT NOT NULL,
        RevokedAt DATETIME2 NULL,
        RevokedBy INT NULL,
        Reason NVARCHAR(500),
        ExpiresAt DATETIME2 NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        
        CONSTRAINT CHK_RbacUserPermissions_Status CHECK (Status IN ('ACTIVE', 'REVOKED', 'EXPIRED')),
        CONSTRAINT CHK_RbacUserPermissions_Expiry CHECK (ExpiresAt IS NULL OR ExpiresAt > GrantedAt),
        CONSTRAINT FK_RbacUserPermissions_User FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT FK_RbacUserPermissions_Permission FOREIGN KEY (PermissionId) REFERENCES RbacPermissions(Id),
        CONSTRAINT FK_RbacUserPermissions_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES Users(Id),
        CONSTRAINT FK_RbacUserPermissions_RevokedBy FOREIGN KEY (RevokedBy) REFERENCES Users(Id),
        CONSTRAINT UQ_RbacUserPermissions UNIQUE (UserId, PermissionId)
    );
    PRINT 'Created RbacUserPermissions table';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RbacUserScope')
BEGIN
    -- User scope assignments (Global vs Project-specific)
    CREATE TABLE RbacUserScope (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        ScopeType NVARCHAR(20) NOT NULL,
        ProjectId INT NULL,
        AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        AssignedBy INT NOT NULL,
        RemovedAt DATETIME2 NULL,
        RemovedBy INT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        
        CONSTRAINT CHK_RbacUserScope_Type CHECK (ScopeType IN ('GLOBAL', 'PROJECT')),
        CONSTRAINT CHK_RbacUserScope_Status CHECK (Status IN ('ACTIVE', 'REMOVED')),
        CONSTRAINT CHK_RbacUserScope_Project CHECK (
            (ScopeType = 'GLOBAL' AND ProjectId IS NULL) OR 
            (ScopeType = 'PROJECT' AND ProjectId IS NOT NULL)
        ),
        CONSTRAINT FK_RbacUserScope_User FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT FK_RbacUserScope_Project FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
        CONSTRAINT FK_RbacUserScope_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(Id),
        CONSTRAINT FK_RbacUserScope_RemovedBy FOREIGN KEY (RemovedBy) REFERENCES Users(Id)
    );
    PRINT 'Created RbacUserScope table';
END

-- =====================================================
-- AUDIT TABLES
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RbacPermissionAuditLog')
BEGIN
    -- Comprehensive permission audit trail
    CREATE TABLE RbacPermissionAuditLog (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ActorUserId INT NOT NULL,
        TargetUserId INT NULL,
        RoleId INT NULL,
        PermissionId INT NULL,
        ActionType NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(50) NOT NULL,
        OldValue NVARCHAR(MAX),
        NewValue NVARCHAR(MAX),
        Reason NVARCHAR(500),
        IpAddress NVARCHAR(45),
        UserAgent NVARCHAR(500),
        SessionId NVARCHAR(100),
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT CHK_RbacPermissionAuditLog_ActionType CHECK (ActionType IN (
            'GRANT', 'REVOKE', 'ROLE_ASSIGN', 'SCOPE_CHANGE', 'ROLE_CREATE', 
            'ROLE_UPDATE', 'ROLE_DEACTIVATE', 'PERMISSION_CREATE', 'PERMISSION_DEACTIVATE'
        )),
        CONSTRAINT CHK_RbacPermissionAuditLog_EntityType CHECK (EntityType IN (
            'USER_PERMISSION', 'ROLE_PERMISSION', 'USER_SCOPE', 'ROLE', 'PERMISSION'
        )),
        CONSTRAINT FK_RbacPermissionAuditLog_Actor FOREIGN KEY (ActorUserId) REFERENCES Users(Id),
        CONSTRAINT FK_RbacPermissionAuditLog_Target FOREIGN KEY (TargetUserId) REFERENCES Users(Id),
        CONSTRAINT FK_RbacPermissionAuditLog_Role FOREIGN KEY (RoleId) REFERENCES RbacRoles(Id),
        CONSTRAINT FK_RbacPermissionAuditLog_Permission FOREIGN KEY (PermissionId) REFERENCES RbacPermissions(Id)
    );
    PRINT 'Created RbacPermissionAuditLog table';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RbacAccessAuditLog')
BEGIN
    -- Access attempt logging
    CREATE TABLE RbacAccessAuditLog (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        PermissionCode NVARCHAR(100) NOT NULL,
        ResourceId NVARCHAR(100),
        ResourceType NVARCHAR(50),
        ActionAttempted NVARCHAR(50),
        AccessGranted BIT NOT NULL,
        DenialReason NVARCHAR(500),
        ResolutionMethod NVARCHAR(50), -- 'USER_OVERRIDE', 'ROLE_PERMISSION', 'DEFAULT_DENY'
        ScopeValidated BIT NOT NULL DEFAULT 0,
        IpAddress NVARCHAR(45),
        UserAgent NVARCHAR(500),
        SessionId NVARCHAR(100),
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_RbacAccessAuditLog_User FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT 'Created RbacAccessAuditLog table';
END

-- =====================================================
-- PERFORMANCE INDEXES
-- =====================================================

-- Roles indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacRoles_Status')
    CREATE INDEX IX_RbacRoles_Status ON RbacRoles(Status);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacRoles_CreatedAt')
    CREATE INDEX IX_RbacRoles_CreatedAt ON RbacRoles(CreatedAt);

-- Permissions indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacPermissions_Module')
    CREATE INDEX IX_RbacPermissions_Module ON RbacPermissions(Module);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacPermissions_Status')
    CREATE INDEX IX_RbacPermissions_Status ON RbacPermissions(Status);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacPermissions_Code')
    CREATE INDEX IX_RbacPermissions_Code ON RbacPermissions(Code);

-- Role permissions indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacRolePermissions_Role_Status')
    CREATE INDEX IX_RbacRolePermissions_Role_Status ON RbacRolePermissions(RoleId, Status);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacRolePermissions_Permission')
    CREATE INDEX IX_RbacRolePermissions_Permission ON RbacRolePermissions(PermissionId);

-- User permissions indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacUserPermissions_User_Status')
    CREATE INDEX IX_RbacUserPermissions_User_Status ON RbacUserPermissions(UserId, Status);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacUserPermissions_Permission')
    CREATE INDEX IX_RbacUserPermissions_Permission ON RbacUserPermissions(PermissionId);

-- User scope indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacUserScope_User_Status')
    CREATE INDEX IX_RbacUserScope_User_Status ON RbacUserScope(UserId, Status);

-- Audit log indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacPermissionAuditLog_Timestamp')
    CREATE INDEX IX_RbacPermissionAuditLog_Timestamp ON RbacPermissionAuditLog(Timestamp);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacPermissionAuditLog_Actor')
    CREATE INDEX IX_RbacPermissionAuditLog_Actor ON RbacPermissionAuditLog(ActorUserId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacAccessAuditLog_Timestamp')
    CREATE INDEX IX_RbacAccessAuditLog_Timestamp ON RbacAccessAuditLog(Timestamp);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RbacAccessAuditLog_User')
    CREATE INDEX IX_RbacAccessAuditLog_User ON RbacAccessAuditLog(UserId);

-- =====================================================
-- INITIAL DATA SEEDING
-- =====================================================

-- Insert default RBAC roles (system roles)
IF NOT EXISTS (SELECT * FROM RbacRoles WHERE Name = 'Super Admin')
BEGIN
    INSERT INTO RbacRoles (Name, Description, IsSystemRole, CreatedBy) VALUES
    ('Super Admin', 'System administrator with unrestricted access to all functions', 1, 1),
    ('Admin', 'Project administrator with user and asset management capabilities', 1, 1),
    ('IT Staff', 'Technical staff with asset maintenance and update capabilities', 1, 1),
    ('Auditor', 'Read-only access for compliance and audit purposes', 1, 1),
    ('Project Manager', 'Project oversight with read-only dashboard access', 1, 1);
    PRINT 'Inserted default RBAC roles';
END

-- Insert atomic permissions
IF NOT EXISTS (SELECT * FROM RbacPermissions WHERE Code = 'USER_CREATE')
BEGIN
    INSERT INTO RbacPermissions (Code, Module, Description, ResourceType, Action, CreatedBy) VALUES
    -- User Management Module
    ('USER_CREATE', 'USER_MANAGEMENT', 'Create new users in the system', 'USER', 'CREATE', 1),
    ('USER_VIEW', 'USER_MANAGEMENT', 'View user profiles and information', 'USER', 'VIEW', 1),
    ('USER_EDIT', 'USER_MANAGEMENT', 'Edit user profiles and settings', 'USER', 'EDIT', 1),
    ('USER_DEACTIVATE', 'USER_MANAGEMENT', 'Deactivate user accounts', 'USER', 'DEACTIVATE', 1),
    ('USER_REACTIVATE', 'USER_MANAGEMENT', 'Reactivate deactivated user accounts', 'USER', 'REACTIVATE', 1),
    ('ROLE_ASSIGN', 'USER_MANAGEMENT', 'Assign roles to users', 'USER', 'ROLE_ASSIGN', 1),
    ('PERMISSION_OVERRIDE', 'USER_MANAGEMENT', 'Override user permissions beyond role defaults', 'USER', 'PERMISSION_OVERRIDE', 1),

    -- Asset Management Module
    ('ASSET_CREATE', 'ASSET_MANAGEMENT', 'Create new assets in the system', 'ASSET', 'CREATE', 1),
    ('ASSET_VIEW', 'ASSET_MANAGEMENT', 'View asset details and information', 'ASSET', 'VIEW', 1),
    ('ASSET_EDIT', 'ASSET_MANAGEMENT', 'Edit asset information and properties', 'ASSET', 'EDIT', 1),
    ('ASSET_TRANSFER', 'ASSET_MANAGEMENT', 'Transfer assets between locations/users', 'ASSET', 'TRANSFER', 1),
    ('ASSET_DECOMMISSION', 'ASSET_MANAGEMENT', 'Decommission assets (soft delete)', 'ASSET', 'DECOMMISSION', 1),
    ('ASSET_REACTIVATE', 'ASSET_MANAGEMENT', 'Reactivate decommissioned assets', 'ASSET', 'REACTIVATE', 1),

    -- Lifecycle & Repairs Module
    ('LIFECYCLE_LOG', 'LIFECYCLE_REPAIRS', 'Log lifecycle events for assets', 'LIFECYCLE', 'LOG', 1),
    ('LIFECYCLE_VIEW', 'LIFECYCLE_REPAIRS', 'View asset lifecycle history', 'LIFECYCLE', 'VIEW', 1),
    ('LIFECYCLE_APPROVE', 'LIFECYCLE_REPAIRS', 'Approve lifecycle transitions', 'LIFECYCLE', 'APPROVE', 1),
    ('REPAIR_ADD', 'LIFECYCLE_REPAIRS', 'Add repair records for assets', 'REPAIR', 'ADD', 1),
    ('REPAIR_VIEW', 'LIFECYCLE_REPAIRS', 'View repair history and records', 'REPAIR', 'VIEW', 1),
    ('MAINTENANCE_SCHEDULE', 'LIFECYCLE_REPAIRS', 'Schedule maintenance activities', 'MAINTENANCE', 'SCHEDULE', 1),

    -- Reports & Audits Module
    ('REPORT_VIEW', 'REPORTS_AUDITS', 'View system reports and dashboards', 'REPORT', 'VIEW', 1),
    ('REPORT_EXPORT', 'REPORTS_AUDITS', 'Export reports to external formats', 'REPORT', 'EXPORT', 1),
    ('AUDIT_VIEW', 'REPORTS_AUDITS', 'View audit trails and logs', 'AUDIT', 'VIEW', 1),
    ('AUDIT_DOWNLOAD', 'REPORTS_AUDITS', 'Download audit data for compliance', 'AUDIT', 'DOWNLOAD', 1),
    ('FINANCIAL_VIEW', 'REPORTS_AUDITS', 'View financial reports and cost data', 'FINANCIAL', 'VIEW', 1),

    -- RBAC Management Module
    ('ROLE_CREATE', 'RBAC_MANAGEMENT', 'Create new roles in the system', 'ROLE', 'CREATE', 1),
    ('ROLE_VIEW', 'RBAC_MANAGEMENT', 'View role definitions and permissions', 'ROLE', 'VIEW', 1),
    ('ROLE_EDIT', 'RBAC_MANAGEMENT', 'Edit role permissions and settings', 'ROLE', 'EDIT', 1),
    ('ROLE_DEACTIVATE', 'RBAC_MANAGEMENT', 'Deactivate roles', 'ROLE', 'DEACTIVATE', 1),
    ('PERMISSION_CREATE', 'RBAC_MANAGEMENT', 'Create new permissions', 'PERMISSION', 'CREATE', 1),
    ('PERMISSION_VIEW', 'RBAC_MANAGEMENT', 'View permission definitions', 'PERMISSION', 'VIEW', 1),
    ('PERMISSION_EDIT', 'RBAC_MANAGEMENT', 'Edit permission definitions', 'PERMISSION', 'EDIT', 1);
    PRINT 'Inserted atomic permissions';
END

-- Assign all permissions to Super Admin role
IF NOT EXISTS (SELECT * FROM RbacRolePermissions WHERE RoleId = 1)
BEGIN
    INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedBy)
    SELECT 1, Id, 1, 1 FROM RbacPermissions WHERE Status = 'ACTIVE';
    PRINT 'Assigned all permissions to Super Admin role';
END

-- Set Super Admin users to have global scope
IF NOT EXISTS (SELECT * FROM RbacUserScope WHERE UserId = 1)
BEGIN
    INSERT INTO RbacUserScope (UserId, ScopeType, AssignedBy)
    SELECT Id, 'GLOBAL', 1 FROM Users WHERE RoleId = 1;
    PRINT 'Set Super Admin users to have global scope';
END

PRINT 'Enterprise RBAC System core tables created successfully!';
PRINT 'Created tables: RbacRoles, RbacPermissions, RbacRolePermissions, RbacUserPermissions, RbacUserScope';
PRINT 'Created audit tables: RbacPermissionAuditLog, RbacAccessAuditLog';
PRINT 'Seeded default roles and permissions';