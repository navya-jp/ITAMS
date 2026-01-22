# 🔗 Connect to Shared ITAMS Database

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerIP
)

Write-Host "=== Connecting to Shared ITAMS Database ===" -ForegroundColor Cyan
Write-Host "Server IP: $ServerIP" -ForegroundColor Yellow

# Update appsettings.json with shared database connection
$appsettingsPath = "appsettings.json"

if (Test-Path $appsettingsPath) {
    Write-Host "`nUpdating appsettings.json..." -ForegroundColor Yellow
    
    try {
        # Read current appsettings
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        
        # Create new connection string
        $connectionString = "Server=$ServerIP,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
        
        # Update connection string
        if (-not $appsettings.ConnectionStrings) {
            $appsettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
        }
        $appsettings.ConnectionStrings.SharedSqlServer = $connectionString
        
        # Save updated appsettings
        $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        
        Write-Host "✓ Updated connection string successfully" -ForegroundColor Green
        
        # Build and run
        Write-Host "`nBuilding project..." -ForegroundColor Yellow
        dotnet build
        
        Write-Host "`nStarting application..." -ForegroundColor Yellow
        Write-Host "Backend: http://localhost:5066" -ForegroundColor Green
        Write-Host "Frontend: http://localhost:4200" -ForegroundColor Green
        Write-Host "`nPress Ctrl+C to stop" -ForegroundColor Yellow
        
        dotnet run
        
    } catch {
        Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "`nTroubleshooting:" -ForegroundColor Yellow
        Write-Host "1. Check server IP: $ServerIP" -ForegroundColor White
        Write-Host "2. Ensure SQL Server is running on host" -ForegroundColor White
        Write-Host "3. Check firewall settings" -ForegroundColor White
    }
} else {
    Write-Host "✗ appsettings.json not found!" -ForegroundColor Red
    Write-Host "Make sure you're in the ITAMS project directory" -ForegroundColor Yellow
}

Write-Host "`n=== Usage ===" -ForegroundColor Cyan
Write-Host ".\connect-to-shared-database.ps1 -ServerIP '192.168.208.10'" -ForegroundColor Gray