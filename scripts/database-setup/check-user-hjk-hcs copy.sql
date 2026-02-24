-- Check user hjk hcs details
SELECT 
    u.Id as UserId,
    u.Username,
    u.RoleId as UserRoleIdColumn,
    r.Id as RolesTableId,
    r.Name as RolesTableName,
    rbr.Id as RbacRolesTableId,
    rbr.Name as RbacRolesTableName,
    (SELECT COUNT(*) FROM RbacRolePermissions WHERE RoleId = rbr.Id AND Status = 'ACTIVE' AND Allowed = 1) as PermissionCount
FROM Users u
LEFT JOIN Roles r ON u.RoleId = r.Id
LEFT JOIN RbacRoles rbr ON r.Name = rbr.Name
WHERE u.Username = 'admin6';
