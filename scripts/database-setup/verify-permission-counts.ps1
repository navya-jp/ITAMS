$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

try {
    $connection.Open()
    Write-Host "=== EXACT BACKEND QUERY RESULTS ===" -ForegroundColor Cyan
    
    # Test Auditor (RbacRoles ID 4)
    Write-Host "`nAuditor (RbacRoles ID 4):" -ForegroundColor Yellow
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
SELECT 
    p.Id as PermissionId,
    p.Code as PermissionCode,
    p.Module,
    p.Description,
    p.ResourceType,
    p.Action,
    p.Status
FROM RbacPermissions p
INNER JOIN RbacRolePermissions rp ON p.Id = rp.PermissionId
WHERE rp.RoleId = 4
  AND rp.Status = 'ACTIVE' 
  AND rp.Allowed = 1
  AND p.Status = 'ACTIVE'
ORDER BY p.Module, p.Code
"@
    $reader = $cmd.ExecuteReader()
    $count = 0
    while ($reader.Read()) {
        $count++
        Write-Host "  $count. $($reader['PermissionCode'])"
    }
    Write-Host "Total: $count permissions" -ForegroundColor Green
    $reader.Close()
    
    # Test Project Manager (RbacRoles ID 5)
    Write-Host "`nProject Manager (RbacRoles ID 5):" -ForegroundColor Yellow
    $cmd.CommandText = @"
SELECT 
    p.Id as PermissionId,
    p.Code as PermissionCode,
    p.Module,
    p.Description,
    p.ResourceType,
    p.Action,
    p.Status
FROM RbacPermissions p
INNER JOIN RbacRolePermissions rp ON p.Id = rp.PermissionId
WHERE rp.RoleId = 5
  AND rp.Status = 'ACTIVE' 
  AND rp.Allowed = 1
  AND p.Status = 'ACTIVE'
ORDER BY p.Module, p.Code
"@
    $reader = $cmd.ExecuteReader()
    $count = 0
    while ($reader.Read()) {
        $count++
        Write-Host "  $count. $($reader['PermissionCode'])"
    }
    Write-Host "Total: $count permissions" -ForegroundColor Green
    $reader.Close()
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}
