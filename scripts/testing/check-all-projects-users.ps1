$connectionString = "Server=(local)\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"

$query = @"
-- All Projects
SELECT 
    Id,
    ProjectId,
    Name,
    Code,
    IsActive
FROM Projects
ORDER BY Id

-- All Users with their projects
SELECT 
    u.Id,
    u.Username,
    u.FirstName,
    u.LastName,
    u.Email,
    u.ProjectId,
    p.ProjectId as ProjectCode,
    p.Name as ProjectName,
    r.Name as RoleName,
    u.IsActive
FROM Users u
LEFT JOIN Projects p ON u.ProjectId = p.Id
LEFT JOIN Roles r ON u.RoleId = r.Id
ORDER BY u.Username

-- All Assets
SELECT 
    Id,
    AssetId,
    AssetTag,
    ProjectId,
    LocationId,
    AssetType,
    Make,
    Model,
    Status
FROM Assets
ORDER BY Id
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host "`n=== ALL PROJECTS ===" -ForegroundColor Cyan
    $dataset.Tables[0] | Format-Table -AutoSize
    
    Write-Host "`n=== ALL USERS ===" -ForegroundColor Cyan
    $dataset.Tables[1] | Format-Table -AutoSize
    
    Write-Host "`n=== ALL ASSETS ===" -ForegroundColor Cyan
    $dataset.Tables[2] | Format-Table -AutoSize
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
