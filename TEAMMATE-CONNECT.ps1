# Simple Connection Script for Teammates
# Connect to Krivam's shared database

Write-Host "=== Connecting to Krivam's Shared Database ===" -ForegroundColor Cyan
Write-Host "Server: 192.168.208.10,1433" -ForegroundColor Yellow

# Update appsettings.json with correct connection string
$content = '{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SharedSqlServer": "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}'

Write-Host "Updating connection..." -ForegroundColor Green
$content | Out-File "appsettings.json" -Encoding UTF8

# Test network connection
Write-Host "Testing connection to 192.168.208.10..." -ForegroundColor Yellow
$ping = Test-Connection -ComputerName "192.168.208.10" -Count 2 -Quiet

if ($ping) {
    Write-Host "Network connection successful!" -ForegroundColor Green
} else {
    Write-Host "Cannot reach 192.168.208.10 - check network" -ForegroundColor Red
    exit 1
}

# Build and run
Write-Host "Building..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Starting application..." -ForegroundColor Green
    Write-Host "You should now see Krivam's shared data!" -ForegroundColor Cyan
    dotnet run
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}