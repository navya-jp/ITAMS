# 🔗 Connect to Shared ITAMS Database

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerIP,
    
    [string]$Username = "itams_user",
    [string]$Password = "ITAMS@2024!",
    [string]$Database = "ITAMS"
)

Write-Host "=== Connecting to Shared ITAMS Database ===" -ForegroundColor Cyan

# Update appsettings.json with shared database connection
Write-Host "`nUpdating database connection..." -ForegroundColor Yellow

$appsettingsPath = "appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    
    # Create connection string
    $connectionString = "Server=$ServerIP,1433;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=true;MultipleActiveResultSets=true"
    
    # Update the connection string
    if (-not $appsettings.ConnectionStrings) {
        $appsettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
    }
    $appsettings.ConnectionStrings.SharedSqlServer = $connectionString
    
    # Save updated appsettings.json
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    
    Write-Host "✓ Updated appsettings.json with shared database connection" -ForegroundColor Green
    Write-Host "Server: $ServerIP" -ForegroundColor Gray
    Write-Host "Database: $Database" -ForegroundColor Gray
    
    # Test connection
    Write-Host "`nTesting database connection..." -ForegroundColor Yellow
    try {
        dotnet build
        Write-Host "✓ Project built successfully" -ForegroundColor Green
        
        Write-Host "`nStarting application..." -ForegroundColor Yellow
        Write-Host "Backend will be available at: http://localhost:5066" -ForegroundColor Green
        Write-Host "Frontend will be available at: http://localhost:4200" -ForegroundColor Green
        Write-Host "`nPress Ctrl+C to stop the server" -ForegroundColor Yellow
        
        dotnet run
    } catch {
        Write-Host "✗ Connection failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "`nTroubleshooting:" -ForegroundColor Yellow
        Write-Host "1. Make sure the server IP is correct: $ServerIP" -ForegroundColor White
        Write-Host "2. Ensure SQL Server is running on the host machine" -ForegroundColor White
        Write-Host "3. Check if port 1433 is open in firewall" -ForegroundColor White
        Write-Host "4. Verify database credentials are correct" -ForegroundColor White
    }
} else {
    Write-Host "✗ appsettings.json not found. Make sure you're in the ITAMS project directory." -ForegroundColor Red
}

# Usage instructions
Write-Host "`n=== Usage ===" -ForegroundColor Cyan
Write-Host ".\connect-to-shared-database.ps1 -ServerIP '192.168.1.100'" -ForegroundColor Gray