$connectionString = "Server=(local)\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"

$query = @"
-- Find the project from the asset
SELECT TOP 1 
    p.Id as ProjectDbId,
    p.ProjectId,
    p.Name as ProjectName,
    p.Code as ProjectCode
FROM Assets a
INNER JOIN Projects p ON a.ProjectId = p.Id
WHERE a.AssetTag = 'psj/a:001'

-- Get all users in this project
SELECT 
    u.Id,
    u.Username,
    u.FirstName,
    u.LastName,
    u.Email,
    u.ProjectId,
    p.ProjectId as ProjectCode,
    p.Name as ProjectName,
    r.Name as RoleName
FROM Users u
INNER JOIN Projects p ON u.ProjectId = p.Id
LEFT JOIN Roles r ON u.RoleId = r.Id
WHERE u.ProjectId = (
    SELECT TOP 1 a.ProjectId 
    FROM Assets a 
    WHERE a.AssetTag = 'psj/a:001'
)
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
    
    Write-Host "`n=== PROJECT INFO ===" -ForegroundColor Cyan
    $dataset.Tables[0] | Format-Table -AutoSize
    
    Write-Host "`n=== USERS IN THIS PROJECT ===" -ForegroundColor Cyan
    $dataset.Tables[1] | Format-Table -AutoSize
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
