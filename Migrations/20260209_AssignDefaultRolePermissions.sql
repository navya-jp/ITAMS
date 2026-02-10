-- =============================================
-- Assign Default Permissions to RBAC Roles
-- Based on Permission Matrix
-- =============================================

-- USE ITAMS_Shared;
-- GO

-- Clear existing role permissions (except Super Admin which was already set)
DELETE FROM RbacRolePermissions WHERE RoleId != 1;
GO

-- =============================================
-- ROLE 2: Admin
-- Full access to Users, Assets, Lifecycle, Repairs, Reports
-- =============================================

-- Users Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Users' AND Status = 'ACTIVE';

-- Assets Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Assets' AND Status = 'ACTIVE';

-- Lifecycle Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Lifecycle' AND Status = 'ACTIVE';

-- Repairs Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Repairs' AND Status = 'ACTIVE';

-- Reports Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 2, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Reports' AND Status = 'ACTIVE';

-- =============================================
-- ROLE 3: IT Staff
-- Full access to Assets, Lifecycle, Repairs
-- View only for Reports
-- =============================================

-- Assets Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 3, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Assets' AND Status = 'ACTIVE';

-- Lifecycle Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 3, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Lifecycle' AND Status = 'ACTIVE';

-- Repairs Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 3, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Repairs' AND Status = 'ACTIVE';

-- Reports Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 3, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Reports' AND Code IN ('REPORT_VIEW', 'REPORT_EXPORT') AND Status = 'ACTIVE';

-- =============================================
-- ROLE 4: Auditor
-- View only for all modules
-- Full access to Reports
-- =============================================

-- Users Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Users' AND Code = 'USER_VIEW' AND Status = 'ACTIVE';

-- Assets Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Assets' AND Code = 'ASSET_VIEW' AND Status = 'ACTIVE';

-- Lifecycle Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Lifecycle' AND Code = 'LIFECYCLE_VIEW' AND Status = 'ACTIVE';

-- Repairs Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Repairs' AND Code = 'REPAIR_VIEW' AND Status = 'ACTIVE';

-- Reports Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 4, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Reports' AND Status = 'ACTIVE';

-- =============================================
-- ROLE 5: Project Manager
-- View only for Users, Assets, Lifecycle
-- Full access to Repairs and Reports
-- =============================================

-- Users Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Users' AND Code = 'USER_VIEW' AND Status = 'ACTIVE';

-- Assets Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Assets' AND Code = 'ASSET_VIEW' AND Status = 'ACTIVE';

-- Lifecycle Module (View only)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Lifecycle' AND Code = 'LIFECYCLE_VIEW' AND Status = 'ACTIVE';

-- Repairs Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Repairs' AND Status = 'ACTIVE';

-- Reports Module (All permissions)
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'Reports' AND Status = 'ACTIVE';

GO

-- Verify the assignments
SELECT 
    r.Name AS RoleName,
    COUNT(rp.Id) AS PermissionCount
FROM RbacRoles r
LEFT JOIN RbacRolePermissions rp ON r.Id = rp.RoleId AND rp.Status = 'ACTIVE'
WHERE r.Status = 'ACTIVE'
GROUP BY r.Id, r.Name
ORDER BY r.Id;

-- Show detailed permissions per role
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

PRINT 'Default role permissions assigned successfully!';
GO
