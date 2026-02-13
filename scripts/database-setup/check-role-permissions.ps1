$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

try {
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green
    
    # Check Roles table
    Write-Host "`n=== Roles Table ===" -ForegroundColor Cyan
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT Id, Name FROM Roles ORDER BY Id"
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "ID: $($reader['Id']), Name: $($reader['Name'])"
    }
    $reader.Close()
    
    # Check RbacRoles table
    Write-Host "`n=== RbacRoles Table ===" -ForegroundColor Cyan
    $cmd.CommandText = "SELECT Id, Name FROM RbacRoles ORDER BY Id"
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "ID: $($reader['Id']), Name: $($reader['Name'])"
    }
    $reader.Close()
    
    # Check permission counts per role
    Write-Host "`n=== Permission Counts by Role ===" -ForegroundColor Cyan
    $cmd.CommandText = @"
SELECT 
    r.Name as RoleName,
    r.Id as RoleId,
    COUNT(DISTINCT rp.PermissionId) as PermissionCount
FROM RbacRoles r
LEFT JOIN RbacRolePermissions rp ON r.Id = rp.RoleId AND rp.Status = 'ACTIVE' AND rp.Allowed = 1
LEFT JOIN RbacPermissions p ON rp.PermissionId = p.Id AND p.Status = 'ACTIVE'
GROUP BY r.Id, r.Name
ORDER BY r.Name
"@
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "$($reader['RoleName']) (ID: $($reader['RoleId'])): $($reader['PermissionCount']) permissions"
    }
    $reader.Close()
    
    # Check Auditor permissions
    Write-Host "`n=== Auditor Permissions ===" -ForegroundColor Yellow
    $cmd.CommandText = @"
SELECT p.Code
FROM RbacRoles r
INNER JOIN RbacRolePermissions rp ON r.Id = rp.RoleId
INNER JOIN RbacPermissions p ON rp.PermissionId = p.Id
WHERE r.Name = 'Auditor' AND rp.Status = 'ACTIVE' AND rp.Allowed = 1 AND p.Status = 'ACTIVE'
ORDER BY p.Code
"@
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "  - $($reader['Code'])"
    }
    $reader.Close()
    
    # Check Project Manager permissions
    Write-Host "`n=== Project Manager Permissions ===" -ForegroundColor Yellow
    $cmd.CommandText = @"
SELECT p.Code
FROM RbacRoles r
INNER JOIN RbacRolePermissions rp ON r.Id = rp.RoleId
INNER JOIN RbacPermissions p ON rp.PermissionId = p.Id
WHERE r.Name = 'Project Manager' AND rp.Status = 'ACTIVE' AND rp.Allowed = 1 AND p.Status = 'ACTIVE'
ORDER BY p.Code
"@
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "  - $($reader['Code'])"
    }
    $reader.Close()
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}
