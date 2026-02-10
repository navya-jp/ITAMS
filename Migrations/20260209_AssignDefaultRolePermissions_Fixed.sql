-- =============================================
-- Assign Default Permissions to RBAC Roles
-- Based on Permission Matrix (CORRECTED)
-- =============================================

-- Clear existing role permissions (except Super Admin which was already set)
DELETE FROM RbacRolePermissions WHERE RoleId != 1;
GO

-- =============================================
-- ROLE 2: Admin
-- Full access to USER_MANAGEMENT, ASSET_MANAGEMENT, LIFECYCLE_REPAIRS, REPORTS_AUDITS
-- =============================================

-- USER_MANAGEMENT Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'USER_MANAGEMENT' AND Status = 'ACTIVE';

-- ASSET_MANAGEMENT Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'ASSET_MANAGEMENT' AND Status = 'ACTIVE';

-- LIFECYCLE_REPAIRS Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'LIFECYCLE_REPAIRS' AND Status = 'ACTIVE';

-- REPORTS_AUDITS Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'REPORTS_AUDITS' AND Status = 'ACTIVE';

-- =============================================
-- ROLE 3: IT Staff
-- Full access to ASSET_MANAGEMENT, LIFECYCLE_REPAIRS
-- View only for REPORTS_AUDITS
-- =============================================

-- ASSET_MANAGEMENT Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 3, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'ASSET_MANAGEMENT' AND Status = 'ACTIVE';

-- LIFECYCLE_REPAIRS Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 3, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'LIFECYCLE_REPAIRS' AND Status = 'ACTIVE';

-- REPORTS_AUDITS Module (View and Export only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 3, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'REPORTS_AUDITS' AND Code IN ('REPORT_VIEW', 'REPORT_EXPORT') AND Status = 'ACTIVE';

-- =============================================
-- ROLE 4: Auditor
-- View only for USER_MANAGEMENT, ASSET_MANAGEMENT, LIFECYCLE_REPAIRS
-- Full access to REPORTS_AUDITS
-- =============================================

-- USER_MANAGEMENT Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'USER_MANAGEMENT' AND Code = 'USER_VIEW' AND Status = 'ACTIVE';

-- ASSET_MANAGEMENT Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'ASSET_MANAGEMENT' AND Code = 'ASSET_VIEW' AND Status = 'ACTIVE';

-- LIFECYCLE_REPAIRS Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'LIFECYCLE_REPAIRS' AND Code IN ('LIFECYCLE_VIEW', 'REPAIR_VIEW') AND Status = 'ACTIVE';

-- REPORTS_AUDITS Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'REPORTS_AUDITS' AND Status = 'ACTIVE';

-- =============================================
-- ROLE 5: Project Manager
-- View only for USER_MANAGEMENT, ASSET_MANAGEMENT, LIFECYCLE_REPAIRS
-- Full access to REPORTS_AUDITS
-- =============================================

-- USER_MANAGEMENT Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'USER_MANAGEMENT' AND Code = 'USER_VIEW' AND Status = 'ACTIVE';

-- ASSET_MANAGEMENT Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'ASSET_MANAGEMENT' AND Code = 'ASSET_VIEW' AND Status = 'ACTIVE';

-- LIFECYCLE_REPAIRS Module (View and Add Repairs)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'LIFECYCLE_REPAIRS' AND Code IN ('LIFECYCLE_VIEW', 'REPAIR_VIEW', 'REPAIR_ADD', 'MAINTENANCE_SCHEDULE') AND Status = 'ACTIVE';

-- REPORTS_AUDITS Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'REPORTS_AUDITS' AND Status = 'ACTIVE';

GO

-- Verify the assignments
PRINT '';
PRINT '=== Permission Assignment Summary ===';
PRINT '';

SELECT 
    r.Name AS RoleName,
    COUNT(rp.Id) AS PermissionCount
FROM RbacRoles r
LEFT JOIN RbacRolePermissions rp ON r.Id = rp.RoleId AND rp.Status = 'ACTIVE'
WHERE r.Status = 'ACTIVE'
GROUP BY r.Id, r.Name
ORDER BY r.Id;

PRINT '';
PRINT '=== Detailed Permissions by Role ===';
PRINT '';

SELECT 
    r.Name AS RoleName,
    p.Module,
    p.Code AS PermissionCode,
    p.Description
FROM RbacRoles r
INNER JOIN RbacRolePermissions rp ON r.Id = rp.RoleId
INNER JOIN RbacPermissions p ON rp.PermissionId = p.Id
WHERE r.Status = 'ACTIVE' AND rp.Status = 'ACTIVE' AND p.Status = 'ACTIVE'
ORDER BY r.Id, p.Module, p.Code;

PRINT '';
PRINT 'Default role permissions assigned successfully!';
GO
