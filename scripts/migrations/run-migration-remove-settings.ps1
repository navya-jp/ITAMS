# PowerShell script to run the migration to remove unused settings
# Run this script from the project root directory

$connectionString = "Server=192.168.208.26,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"
$migrationFile = "Migrations/20260224_RemoveUnusedSettings.sql"

Write-Host "Running migration: $migrationFile" -ForegroundColor Cyan
Write-Host "Target database: ITAMS_Shared on 192.168.208.26" -ForegroundColor Cyan
Write-Host ""

try {
    # Read the SQL file
    $sql = Get-Content $migrationFile -Raw
    
    # Execute the SQL
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $sql -Verbose
    
    Write-Host ""
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host "Removed settings:" -ForegroundColor Yellow
    Write-Host "  - AllowMultipleSessions" -ForegroundColor Yellow
    Write-Host "  - SessionWarningMinutes" -ForegroundColor Yellow
}
catch {
    Write-Host ""
    Write-Host "Error running migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
