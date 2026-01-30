-- Enterprise RBAC System - Core Tables Migration
-- Created: 2026-01-30
-- Description: Creates the core RBAC tables for permission-driven access control

-- =====================================================
-- RBAC CORE TABLES
-- =====================================================

-- Roles table (permission templates)
CREATE TABLE rbac_roles (
    role_id INT PRIMARY KEY IDENTITY(1,1),
    role_name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(500),
    is_system_role BIT NOT NULL DEFAULT 0,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    created_by INT NOT NULL,
    deactivated_at DATETIME2 NULL,
    deactivated_by INT NULL,
    
    CONSTRAINT CHK_rbac_roles_status CHECK (status IN ('ACTIVE', 'INACTIVE')),
    CONSTRAINT FK_rbac_roles_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT FK_rbac_roles_deactivated_by FOREIGN KEY (deactivated_by) REFERENCES users(id)
);

-- Permissions table (atomic authorization units)
CREATE TABLE rbac_permissions (
    permission_id INT PRIMARY KEY IDENTITY(1,1),
    permission_code NVARCHAR(100) NOT NULL UNIQUE,
    module NVARCHAR(50) NOT NULL,
    description NVARCHAR(500) NOT NULL,
    resource_type NVARCHAR(50) NOT NULL,
    action NVARCHAR(50) NOT NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    created_by INT NOT NULL,
    deactivated_at DATETIME2 NULL,
    deactivated_by INT NULL,
    
    CONSTRAINT CHK_rbac_permissions_status CHECK (status IN ('ACTIVE', 'INACTIVE')),
    CONSTRAINT CHK_rbac_permissions_code CHECK (permission_code LIKE '%[_]%' AND LEN(permission_code) >= 5),
    CONSTRAINT FK_rbac_permissions_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT FK_rbac_permissions_deactivated_by FOREIGN KEY (deactivated_by) REFERENCES users(id)
);

-- Role default permissions matrix
CREATE TABLE rbac_role_permissions (
    role_permission_id INT PRIMARY KEY IDENTITY(1,1),
    role_id INT NOT NULL,
    permission_id INT NOT NULL,
    allowed BIT NOT NULL DEFAULT 1,
    granted_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    granted_by INT NOT NULL,
    revoked_at DATETIME2 NULL,
    revoked_by INT NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    
    CONSTRAINT CHK_rbac_role_permissions_status CHECK (status IN ('ACTIVE', 'REVOKED')),
    CONSTRAINT FK_rbac_role_permissions_role FOREIGN KEY (role_id) REFERENCES rbac_roles(role_id),
    CONSTRAINT FK_rbac_role_permissions_permission FOREIGN KEY (permission_id) REFERENCES rbac_permissions(permission_id),
    CONSTRAINT FK_rbac_role_permissions_granted_by FOREIGN KEY (granted_by) REFERENCES users(id),
    CONSTRAINT FK_rbac_role_permissions_revoked_by FOREIGN KEY (revoked_by) REFERENCES users(id),
    CONSTRAINT UQ_rbac_role_permissions UNIQUE (role_id, permission_id)
);

-- User-specific permission overrides
CREATE TABLE rbac_user_permissions (
    user_permission_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    permission_id INT NOT NULL,
    allowed BIT NOT NULL,
    granted_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    granted_by INT NOT NULL,
    revoked_at DATETIME2 NULL,
    revoked_by INT NULL,
    reason NVARCHAR(500),
    expires_at DATETIME2 NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    
    CONSTRAINT CHK_rbac_user_permissions_status CHECK (status IN ('ACTIVE', 'REVOKED', 'EXPIRED')),
    CONSTRAINT CHK_rbac_user_permissions_expiry CHECK (expires_at IS NULL OR expires_at > granted_at),
    CONSTRAINT FK_rbac_user_permissions_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_rbac_user_permissions_permission FOREIGN KEY (permission_id) REFERENCES rbac_permissions(permission_id),
    CONSTRAINT FK_rbac_user_permissions_granted_by FOREIGN KEY (granted_by) REFERENCES users(id),
    CONSTRAINT FK_rbac_user_permissions_revoked_by FOREIGN KEY (revoked_by) REFERENCES users(id),
    CONSTRAINT UQ_rbac_user_permissions UNIQUE (user_id, permission_id)
);

-- User scope assignments (Global vs Project-specific)
CREATE TABLE rbac_user_scope (
    user_scope_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    scope_type NVARCHAR(20) NOT NULL,
    project_id INT NULL,
    assigned_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    assigned_by INT NOT NULL,
    removed_at DATETIME2 NULL,
    removed_by INT NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    
    CONSTRAINT CHK_rbac_user_scope_type CHECK (scope_type IN ('GLOBAL', 'PROJECT')),
    CONSTRAINT CHK_rbac_user_scope_status CHECK (status IN ('ACTIVE', 'REMOVED')),
    CONSTRAINT CHK_rbac_user_scope_project CHECK (
        (scope_type = 'GLOBAL' AND project_id IS NULL) OR 
        (scope_type = 'PROJECT' AND project_id IS NOT NULL)
    ),
    CONSTRAINT FK_rbac_user_scope_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_rbac_user_scope_project FOREIGN KEY (project_id) REFERENCES projects(id),
    CONSTRAINT FK_rbac_user_scope_assigned_by FOREIGN KEY (assigned_by) REFERENCES users(id),
    CONSTRAINT FK_rbac_user_scope_removed_by FOREIGN KEY (removed_by) REFERENCES users(id)
);

-- =====================================================
-- AUDIT TABLES
-- =====================================================

-- Comprehensive permission audit trail
CREATE TABLE rbac_permission_audit_log (
    audit_id INT PRIMARY KEY IDENTITY(1,1),
    actor_user_id INT NOT NULL,
    target_user_id INT NULL,
    role_id INT NULL,
    permission_id INT NULL,
    action_type NVARCHAR(50) NOT NULL,
    entity_type NVARCHAR(50) NOT NULL,
    old_value NVARCHAR(MAX),
    new_value NVARCHAR(MAX),
    reason NVARCHAR(500),
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(500),
    session_id NVARCHAR(100),
    timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT CHK_rbac_audit_action_type CHECK (action_type IN (
        'GRANT', 'REVOKE', 'ROLE_ASSIGN', 'SCOPE_CHANGE', 'ROLE_CREATE', 
        'ROLE_UPDATE', 'ROLE_DEACTIVATE', 'PERMISSION_CREATE', 'PERMISSION_DEACTIVATE'
    )),
    CONSTRAINT CHK_rbac_audit_entity_type CHECK (entity_type IN (
        'USER_PERMISSION', 'ROLE_PERMISSION', 'USER_SCOPE', 'ROLE', 'PERMISSION'
    )),
    CONSTRAINT FK_rbac_audit_actor FOREIGN KEY (actor_user_id) REFERENCES users(id),
    CONSTRAINT FK_rbac_audit_target FOREIGN KEY (target_user_id) REFERENCES users(id),
    CONSTRAINT FK_rbac_audit_role FOREIGN KEY (role_id) REFERENCES rbac_roles(role_id),
    CONSTRAINT FK_rbac_audit_permission FOREIGN KEY (permission_id) REFERENCES rbac_permissions(permission_id)
);

-- Access attempt logging
CREATE TABLE rbac_access_audit_log (
    access_audit_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    permission_code NVARCHAR(100) NOT NULL,
    resource_id NVARCHAR(100),
    resource_type NVARCHAR(50),
    action_attempted NVARCHAR(50),
    access_granted BIT NOT NULL,
    denial_reason NVARCHAR(500),
    resolution_method NVARCHAR(50), -- 'USER_OVERRIDE', 'ROLE_PERMISSION', 'DEFAULT_DENY'
    scope_validated BIT NOT NULL DEFAULT 0,
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(500),
    session_id NVARCHAR(100),
    timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_rbac_access_audit_user FOREIGN KEY (user_id) REFERENCES users(id)
);

-- =====================================================
-- PERFORMANCE INDEXES
-- =====================================================

-- Roles indexes
CREATE INDEX IX_rbac_roles_status ON rbac_roles(status);
CREATE INDEX IX_rbac_roles_created_at ON rbac_roles(created_at);

-- Permissions indexes
CREATE INDEX IX_rbac_permissions_module ON rbac_permissions(module);
CREATE INDEX IX_rbac_permissions_status ON rbac_permissions(status);
CREATE INDEX IX_rbac_permissions_code ON rbac_permissions(permission_code);

-- Role permissions indexes
CREATE INDEX IX_rbac_role_permissions_role_status ON rbac_role_permissions(role_id, status);
CREATE INDEX IX_rbac_role_permissions_permission ON rbac_role_permissions(permission_id);

-- User permissions indexes
CREATE INDEX IX_rbac_user_permissions_user_status ON rbac_user_permissions(user_id, status);
CREATE INDEX IX_rbac_user_permissions_permission ON rbac_user_permissions(permission_id);
CREATE INDEX IX_rbac_user_permissions_expires ON rbac_user_permissions(expires_at) WHERE expires_at IS NOT NULL;

-- User scope indexes
CREATE INDEX IX_rbac_user_scope_user_status ON rbac_user_scope(user_id, status);
CREATE INDEX IX_rbac_user_scope_project ON rbac_user_scope(project_id) WHERE project_id IS NOT NULL;

-- Audit log indexes
CREATE INDEX IX_rbac_permission_audit_timestamp ON rbac_permission_audit_log(timestamp);
CREATE INDEX IX_rbac_permission_audit_actor ON rbac_permission_audit_log(actor_user_id);
CREATE INDEX IX_rbac_permission_audit_target ON rbac_permission_audit_log(target_user_id);
CREATE INDEX IX_rbac_permission_audit_action ON rbac_permission_audit_log(action_type);

CREATE INDEX IX_rbac_access_audit_timestamp ON rbac_access_audit_log(timestamp);
CREATE INDEX IX_rbac_access_audit_user ON rbac_access_audit_log(user_id);
CREATE INDEX IX_rbac_access_audit_permission ON rbac_access_audit_log(permission_code);
CREATE INDEX IX_rbac_access_audit_granted ON rbac_access_audit_log(access_granted);

-- =====================================================
-- DATA IMMUTABILITY UPDATES TO EXISTING TABLES
-- =====================================================

-- Update users table for data immutability
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('users') AND name = 'status')
BEGIN
    ALTER TABLE users ADD status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE';
    ALTER TABLE users ADD CONSTRAINT CHK_users_status CHECK (status IN ('ACTIVE', 'INACTIVE', 'SUSPENDED', 'LOCKED'));
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('users') AND name = 'deactivated_at')
BEGIN
    ALTER TABLE users ADD deactivated_at DATETIME2 NULL;
    ALTER TABLE users ADD deactivated_by INT NULL;
    ALTER TABLE users ADD CONSTRAINT FK_users_deactivated_by FOREIGN KEY (deactivated_by) REFERENCES users(id);
END

-- Update assets table for data immutability (if exists)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'assets')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('assets') AND name = 'status')
    BEGIN
        ALTER TABLE assets ADD status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE';
        ALTER TABLE assets ADD CONSTRAINT CHK_assets_status CHECK (status IN ('ACTIVE', 'IN_REPAIR', 'TRANSFERRED', 'DECOMMISSIONED'));
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('assets') AND name = 'decommissioned_at')
    BEGIN
        ALTER TABLE assets ADD decommissioned_at DATETIME2 NULL;
        ALTER TABLE assets ADD decommissioned_by INT NULL;
        ALTER TABLE assets ADD CONSTRAINT FK_assets_decommissioned_by FOREIGN KEY (decommissioned_by) REFERENCES users(id);
    END
END

-- Update projects table for data immutability (if exists)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'projects')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('projects') AND name = 'status')
    BEGIN
        ALTER TABLE projects ADD status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE';
        ALTER TABLE projects ADD CONSTRAINT CHK_projects_status CHECK (status IN ('ACTIVE', 'INACTIVE', 'COMPLETED', 'CANCELLED'));
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('projects') AND name = 'deactivated_at')
    BEGIN
        ALTER TABLE projects ADD deactivated_at DATETIME2 NULL;
        ALTER TABLE projects ADD deactivated_by INT NULL;
        ALTER TABLE projects ADD CONSTRAINT FK_projects_deactivated_by FOREIGN KEY (deactivated_by) REFERENCES users(id);
    END
END

-- =====================================================
-- INITIAL DATA SEEDING
-- =====================================================

-- Insert default roles (system roles)
INSERT INTO rbac_roles (role_name, description, is_system_role, created_by) VALUES
('Super Admin', 'System administrator with unrestricted access to all functions', 1, 1),
('Admin', 'Project administrator with user and asset management capabilities', 1, 1),
('IT Staff', 'Technical staff with asset maintenance and update capabilities', 1, 1),
('Auditor', 'Read-only access for compliance and audit purposes', 1, 1),
('Project Manager', 'Project oversight with read-only dashboard access', 1, 1);

-- Insert atomic permissions
INSERT INTO rbac_permissions (permission_code, module, description, resource_type, action, created_by) VALUES
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

-- Assign all permissions to Super Admin role
INSERT INTO rbac_role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 1, permission_id, 1, 1 FROM rbac_permissions WHERE status = 'ACTIVE';

-- Assign Admin role permissions (project-specific user and asset management)
INSERT INTO rbac_role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 2, permission_id, 1, 1 FROM rbac_permissions 
WHERE permission_code IN (
    'USER_CREATE', 'USER_VIEW', 'USER_EDIT', 'USER_DEACTIVATE', 'ROLE_ASSIGN',
    'ASSET_CREATE', 'ASSET_VIEW', 'ASSET_EDIT', 'ASSET_TRANSFER',
    'LIFECYCLE_LOG', 'LIFECYCLE_VIEW', 'REPAIR_ADD', 'REPAIR_VIEW',
    'REPORT_VIEW', 'REPORT_EXPORT'
);

-- Assign IT Staff role permissions (project-specific asset operations only)
INSERT INTO rbac_role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 3, permission_id, 1, 1 FROM rbac_permissions 
WHERE permission_code IN (
    'ASSET_VIEW', 'ASSET_EDIT',
    'LIFECYCLE_LOG', 'LIFECYCLE_VIEW', 'REPAIR_ADD', 'REPAIR_VIEW',
    'MAINTENANCE_SCHEDULE'
);

-- Assign Auditor role permissions (global scope, read-only access)
INSERT INTO rbac_role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 4, permission_id, 1, 1 FROM rbac_permissions 
WHERE action IN ('VIEW', 'DOWNLOAD') AND status = 'ACTIVE';

-- Assign Project Manager role permissions (project scope, read-only dashboards)
INSERT INTO rbac_role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 5, permission_id, 1, 1 FROM rbac_permissions 
WHERE permission_code IN ('REPORT_VIEW', 'ASSET_VIEW', 'LIFECYCLE_VIEW');

-- Set Super Admin users to have global scope
INSERT INTO rbac_user_scope (user_id, scope_type, assigned_by)
SELECT id, 'GLOBAL', 1 FROM users WHERE role_id = 1;

PRINT 'Enterprise RBAC System core tables created successfully!';
PRINT 'Created tables: rbac_roles, rbac_permissions, rbac_role_permissions, rbac_user_permissions, rbac_user_scope';
PRINT 'Created audit tables: rbac_permission_audit_log, rbac_access_audit_log';
PRINT 'Updated existing tables for data immutability';
PRINT 'Seeded default roles and permissions';