$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

try {
    $connection.Open()
    Write-Host "Connected to database successfully" -ForegroundColor Green
    
    # Check users and their roles
    Write-Host "`n=== Users and Their Roles ===" -ForegroundColor Cyan
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
SELECT 
    u.Username,
    u.RoleId as UserRoleId,
    r.Name as RoleName
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.Id
WHERE u.Username IN ('hjk.hcx', 'haha.hehe', 'krayyy.mehtaa')
ORDER BY u.Username
"@
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "$($reader['Username']): RoleId=$($reader['UserRoleId']), RoleName=$($reader['RoleName'])"
    }
    $reader.Close()
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}
