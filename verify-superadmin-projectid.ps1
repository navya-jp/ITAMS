$connectionString = "Server=(local)\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"

$query = @"
-- Check SuperAdmin users
SELECT 
    u.Id,
    u.Username,
    u.FirstName,
    u.LastName,
    u.ProjectId,
    r.Name as RoleName
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.Id
WHERE r.Name = 'Super Admin'
ORDER BY u.Username
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host "`n=== SUPERADMIN USERS ===" -ForegroundColor Cyan
    $dataset.Tables[0] | Format-Table -AutoSize
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
