-- Check how Users.RoleId relates to RbacRoles
SELECT TOP 5
    u.Id as UserId,
    u.Username,
    u.RoleId as UserRoleId,
    r.Id as RoleTableId,
    r.RoleId as RoleAlternateKey,
    r.Name as RoleName,
    rbr.Id as RbacRoleId,
    rbr.RbacRoleId as RbacRoleAlternateKey,
    rbr.Name as RbacRoleName
FROM Users u
LEFT JOIN Roles r ON u.RoleId = r.Id
LEFT JOIN RbacRoles rbr ON r.RoleId = rbr.RbacRoleId
ORDER BY u.Id;
