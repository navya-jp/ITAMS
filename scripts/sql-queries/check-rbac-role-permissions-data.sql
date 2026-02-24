-- Check RbacRolePermissions to see what RoleId values are stored
SELECT DISTINCT 
    rp.RoleId,
    r.Id as RbacRoleTableId,
    r.RbacRoleId as RbacRoleAlternateKey,
    r.Name as RoleName,
    COUNT(*) as PermissionCount
FROM RbacRolePermissions rp
LEFT JOIN RbacRoles r ON rp.RoleId = r.Id
WHERE rp.Status = 'ACTIVE' AND rp.Allowed = 1
GROUP BY rp.RoleId, r.Id, r.RbacRoleId, r.Name
ORDER BY rp.RoleId;
