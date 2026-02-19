# Verify shared database connection and status
$connectionString = "Server=localhost\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;"

Write-Host "=== ITAMS Shared Database Connection Verification ===" -ForegroundColor Cyan
Write-Host ""

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "✓ Database connection successful!" -ForegroundColor Green
    Write-Host "  Server: $($connection.DataSource)" -ForegroundColor Gray
    Write-Host "  Database: $($connection.Database)" -ForegroundColor Gray
    Write-Host ""
    
    # Check users count
    $query = "SELECT COUNT(*) AS UserCount FROM Users"
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $userCount = $command.ExecuteScalar()
    Write-Host "✓ Total Users: $userCount" -ForegroundColor Green
    
    # Check active sessions
    $query = "SELECT COUNT(*) AS ActiveSessions FROM Users WHERE ActiveSessionId IS NOT NULL"
    $command.CommandText = $query
    $activeSessions = $command.ExecuteScalar()
    Write-Host "✓ Active Sessions: $activeSessions" -ForegroundColor Green
    
    # Check projects
    $query = "SELECT COUNT(*) AS ProjectCount FROM Projects"
    $command.CommandText = $query
    $projectCount = $command.ExecuteScalar()
    Write-Host "✓ Total Projects: $projectCount" -ForegroundColor Green
    
    # Check locations
    $query = "SELECT COUNT(*) AS LocationCount FROM Locations"
    $command.CommandText = $query
    $locationCount = $command.ExecuteScalar()
    Write-Host "✓ Total Locations: $locationCount" -ForegroundColor Green
    
    # Check if Site column exists
    $query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Locations' AND COLUMN_NAME = 'Site'"
    $command.CommandText = $query
    $hasSite = $command.ExecuteScalar()
    if ($hasSite -gt 0) {
        Write-Host "✓ Location table updated (Plaza → Site)" -ForegroundColor Green
    }
    
    # Check LoginAudit status types
    $query = "SELECT DISTINCT Status FROM LoginAudit"
    $command.CommandText = $query
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host ""
    Write-Host "Login Audit Status Types:" -ForegroundColor Cyan
    foreach ($row in $dataset.Tables[0].Rows) {
        Write-Host "  - $($row.Status)" -ForegroundColor Gray
    }
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "=== Ready to connect! ===" -ForegroundColor Green
    Write-Host "Backend: http://localhost:5066" -ForegroundColor Yellow
    Write-Host "Frontend: http://localhost:4200" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Login credentials:" -ForegroundColor Cyan
    Write-Host "  Username: superadmin" -ForegroundColor Gray
    Write-Host "  Password: Admin@123" -ForegroundColor Gray
}
catch {
    Write-Host "✗ Connection failed: $_" -ForegroundColor Red
}
