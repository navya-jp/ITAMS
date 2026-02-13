-- Check role IDs and their mappings
SELECT 'Roles Table' as Source, Id, Name FROM Roles ORDER BY Id;
SELECT 'RbacRoles Table' as Source, Id, Name FROM RbacRoles ORDER BY Id;

-- Check what permissions each role has in RbacRolePermissions
SELECT 
    r.Name as RoleName,
    r.Id as RoleId,
    COUNT(DISTINCT rp.PermissionId) as PermissionCount
FROM RbacRoles r
LEFT JOIN RbacRolePermissions rp ON r.Id = rp.RoleId AND rp.Status = 'ACTIVE' AND rp.Allowed = 1
GROUP BY r.Id, r.Name
ORDER BY r.Name;

-- Check specific roles
SELECT 
    'Auditor Permissions' as Info,
    r.Id as RoleId,
    r.Name as RoleName,
    p.Id as PermissionId,
    p.Code as PermissionCode,
    rp.Status,
    rp.Allowed
FROM RbacRoles r
INNER JOIN RbacRolePermissions rp ON r.Id = rp.RoleId
INNER JOIN RbacPermissions p ON rp.PermissionId = p.Id
WHERE r.Name = 'Auditor' AND rp.Status = 'ACTIVE' AND rp.Allowed = 1 AND p.Status = 'ACTIVE'
ORDER BY p.Code;

SELECT 
    'Project Manager Permissions' as Info,
    r.Id as RoleId,
    r.Name as RoleName,
    p.Id as PermissionId,
    p.Code as PermissionCode,
    rp.Status,
    rp.Allowed
FROM RbacRoles r
INNER JOIN RbacRolePermissions rp ON r.Id = rp.RoleId
INNER JOIN RbacPermissions p ON rp.PermissionId = p.Id
WHERE r.Name = 'Project Manager' AND rp.Status = 'ACTIVE' AND rp.Allowed = 1 AND p.Status = 'ACTIVE'
ORDER BY p.Code;
