# Switch to Local Database
# This script updates the connection string to use the local database

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Switching to Local Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$appSettingsPath = "appsettings.json"
$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

# Backup current connection string if not already backed up
if (-not ($appSettings.ConnectionStrings.PSObject.Properties.Name -contains "SharedSqlServer_Backup")) {
    $appSettings.ConnectionStrings | Add-Member -NotePropertyName "SharedSqlServer_Backup" -NotePropertyValue $appSettings.ConnectionStrings.SharedSqlServer -Force
}

# Set to local database
$appSettings.ConnectionStrings.SharedSqlServer = "Server=.\SQLEXPRESS;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# Save
$appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath

Write-Host "Connection string updated to use local database" -ForegroundColor Green
Write-Host ""
Write-Host "Server: .\SQLEXPRESS" -ForegroundColor White
Write-Host "Database: ITAMS_Local" -ForegroundColor White
Write-Host ""

# Show summary
Write-Host "Local Database Summary:" -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT 'Users' as [Table], COUNT(*) as [Rows] FROM Users UNION ALL SELECT 'Roles', COUNT(*) FROM Roles UNION ALL SELECT 'Projects', COUNT(*) FROM Projects UNION ALL SELECT 'Locations', COUNT(*) FROM Locations UNION ALL SELECT 'Assets', COUNT(*) FROM Assets"

Write-Host ""
Write-Host "Restart your application to use the local database." -ForegroundColor Yellow
Write-Host ""
Write-Host "Demo Credentials:" -ForegroundColor Cyan
Write-Host "  Username: superadmin" -ForegroundColor White
Write-Host "  Password: (same as shared database)" -ForegroundColor White
