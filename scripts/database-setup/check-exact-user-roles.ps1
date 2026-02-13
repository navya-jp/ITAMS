$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

try {
    $connection.Open()
    Write-Host "=== Checking User Role IDs ===" -ForegroundColor Cyan
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
SELECT 
    u.Username,
    u.RoleId as UserTableRoleId,
    r.Name as RolesTableName,
    rbac.Id as RbacRolesId,
    rbac.Name as RbacRolesName
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.Id
LEFT JOIN RbacRoles rbac ON r.Name = rbac.Name
WHERE u.Username IN ('hjk.hcx', 'haha.hehe', 'john.doe')
ORDER BY u.Username
"@
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "`nUser: $($reader['Username'])"
        Write-Host "  Users.RoleId: $($reader['UserTableRoleId'])"
        Write-Host "  Roles.Name: $($reader['RolesTableName'])"
        Write-Host "  RbacRoles.Id: $($reader['RbacRolesId'])"
        Write-Host "  RbacRoles.Name: $($reader['RbacRolesName'])"
    }
    $reader.Close()
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}
