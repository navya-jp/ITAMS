# Switch to Shared Database
# This script updates the connection string to use the shared database

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Switching to Shared Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$appSettingsPath = "appsettings.json"
$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

# Restore shared database connection string
if ($appSettings.ConnectionStrings.PSObject.Properties.Name -contains "SharedSqlServer_Backup") {
    $appSettings.ConnectionStrings.SharedSqlServer = $appSettings.ConnectionStrings.SharedSqlServer_Backup
    $appSettings.ConnectionStrings.PSObject.Properties.Remove("SharedSqlServer_Backup")
}
else {
    # Default shared database connection string
    $appSettings.ConnectionStrings.SharedSqlServer = "Server=192.168.208.26,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
}

# Save
$appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath

Write-Host "Connection string updated to use shared database" -ForegroundColor Green
Write-Host ""
Write-Host "Server: 192.168.208.26,1433" -ForegroundColor White
Write-Host "Database: ITAMS_Shared" -ForegroundColor White
Write-Host ""
Write-Host "Restart your application to use the shared database." -ForegroundColor Yellow
