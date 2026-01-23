# Setup SQL Server for Team Sharing
# Run this script as Administrator

Write-Host "=== ITAMS SQL Server Sharing Setup ===" -ForegroundColor Cyan

# Step 1: Run the SQL script
Write-Host "1. Creating shared database and user..." -ForegroundColor Green

sqlcmd -S ".\SQLEXPRESS" -E -i "create-shared-database.sql"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Database setup completed!" -ForegroundColor Green
} else {
    Write-Host "Database setup failed!" -ForegroundColor Red
    exit 1
}

# Step 2: Update appsettings.json
Write-Host "2. Updating configuration..." -ForegroundColor Green

$content = '{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SharedSqlServer": "Server=192.168.208.10\\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}'

$content | Out-File "appsettings.json" -Encoding UTF8

Write-Host "Setup complete!" -ForegroundColor Green
Write-Host "Database: ITAMS_Shared" -ForegroundColor Yellow
Write-Host "Server: 192.168.208.10\SQLEXPRESS" -ForegroundColor Yellow