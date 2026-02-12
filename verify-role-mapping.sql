-- Verify the exact role mapping
SELECT 
    r.Id as RolesTableId,
    r.RoleId as RolesAlternateKey,
    r.Name as RolesTableName,
    rbr.Id as RbacRolesTableId,
    rbr.RbacRoleId as RbacRolesAlternateKey,
    rbr.Name as RbacRolesTableName,
    (SELECT COUNT(*) FROM RbacRolePermissions WHERE RoleId = rbr.Id AND Status = 'ACTIVE' AND Allowed = 1) as PermissionCount
FROM Roles r
LEFT JOIN RbacRoles rbr ON r.RoleId = rbr.RbacRoleId
ORDER BY r.Id;
