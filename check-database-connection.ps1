# Check Database Connection Script
# This script verifies which database you're connected to

Write-Host "=== ITAMS Database Connection Check ===" -ForegroundColor Cyan

# Check appsettings.json
$appsettingsPath = "appsettings.json"

if (Test-Path $appsettingsPath) {
    try {
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        
        Write-Host "`nConnection Strings in appsettings.json:" -ForegroundColor Yellow
        
        if ($appsettings.ConnectionStrings.DefaultConnection) {
            Write-Host "DefaultConnection: $($appsettings.ConnectionStrings.DefaultConnection)" -ForegroundColor Gray
        }
        
        if ($appsettings.ConnectionStrings.SharedSqlServer) {
            Write-Host "SharedSqlServer: $($appsettings.ConnectionStrings.SharedSqlServer)" -ForegroundColor Green
            Write-Host "✓ Shared database connection is configured!" -ForegroundColor Green
        } else {
            Write-Host "✗ SharedSqlServer connection string is missing!" -ForegroundColor Red
            Write-Host "Run team-connect-simple.ps1 to configure shared database connection." -ForegroundColor Yellow
        }
        
        # Determine which connection will be used
        if ($appsettings.ConnectionStrings.SharedSqlServer) {
            Write-Host "`n✓ Application will use: SHARED DATABASE (192.168.208.10)" -ForegroundColor Green
            Write-Host "  You will see all users created by team members." -ForegroundColor Green
        } else {
            Write-Host "`n⚠ Application will use: LOCAL DATABASE" -ForegroundColor Yellow
            Write-Host "  You will only see users you created locally." -ForegroundColor Yellow
        }
        
    } catch {
        Write-Host "Error reading appsettings.json: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "appsettings.json not found!" -ForegroundColor Red
}

Write-Host "`nTo connect to shared database, run: .\team-connect-simple.ps1" -ForegroundColor Cyan