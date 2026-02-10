# Connect to Shared ITAMS Database
# Run this after pulling latest changes from GitHub

Write-Host "=== Connecting to Shared ITAMS Database ===" -ForegroundColor Cyan
Write-Host "Server: 192.168.208.10\SQLEXPRESS" -ForegroundColor Yellow
Write-Host "Database: ITAMS_Shared" -ForegroundColor Yellow

# Update appsettings.json with shared database connection
$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"

$appsettingsContent = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SharedSqlServer": "$connectionString"
  }
}
"@

Write-Host "`nUpdating connection configuration..." -ForegroundColor Green
$appsettingsContent | Set-Content "appsettings.json" -Encoding UTF8

# Test network connectivity
Write-Host "Testing network connection to 192.168.208.10..." -ForegroundColor Yellow
$ping = Test-Connection -ComputerName "192.168.208.10" -Count 2 -Quiet

if ($ping) {
    Write-Host "✓ Network connection successful" -ForegroundColor Green
} else {
    Write-Host "✗ Cannot reach 192.168.208.10" -ForegroundColor Red
    Write-Host "Please check:" -ForegroundColor Yellow
    Write-Host "• Network connectivity" -ForegroundColor Gray
    Write-Host "• Firewall settings" -ForegroundColor Gray
    Write-Host "• Host machine is running" -ForegroundColor Gray
    exit 1
}

# Build and run the application
Write-Host "`nBuilding application..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build successful!" -ForegroundColor Green
    Write-Host "`nStarting application..." -ForegroundColor Green
    Write-Host "Backend: http://localhost:5066" -ForegroundColor Cyan
    Write-Host "Frontend: http://localhost:4200" -ForegroundColor Cyan
    Write-Host "`nYou should now see all shared data!" -ForegroundColor Green
    
    dotnet run
} else {
    Write-Host "✗ Build failed. Please check errors above." -ForegroundColor Red
}