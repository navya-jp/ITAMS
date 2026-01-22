# FORCE SHARED DATABASE CONNECTION
# This script ensures connection to 192.168.208.10 shared database

Write-Host "=== FORCING SHARED DATABASE CONNECTION ===" -ForegroundColor Red
Write-Host "Target: 192.168.208.10 (Krivam's database)" -ForegroundColor Yellow

# Step 1: Update appsettings.json
Write-Host "`n1. Updating appsettings.json..." -ForegroundColor Cyan

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
    "DefaultConnection": "DISABLED - DO NOT USE LOCAL DB",
    "SharedSqlServer": "Server=192.168.208.10,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
"@

$appsettingsContent | Set-Content "appsettings.json" -Encoding UTF8
Write-Host "✓ appsettings.json updated with SHARED database only" -ForegroundColor Green

# Step 2: Verify connection string
Write-Host "`n2. Verifying connection string..." -ForegroundColor Cyan
$config = Get-Content "appsettings.json" | ConvertFrom-Json
Write-Host "SharedSqlServer: $($config.ConnectionStrings.SharedSqlServer)" -ForegroundColor Yellow

# Step 3: Test network connectivity
Write-Host "`n3. Testing network connectivity to 192.168.208.10..." -ForegroundColor Cyan
$ping = Test-Connection -ComputerName "192.168.208.10" -Count 2 -Quiet
if ($ping) {
    Write-Host "✓ Network connection to 192.168.208.10 is working" -ForegroundColor Green
} else {
    Write-Host "✗ Cannot reach 192.168.208.10 - check network/firewall" -ForegroundColor Red
    exit 1
}

# Step 4: Build and run
Write-Host "`n4. Building project..." -ForegroundColor Cyan
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build successful" -ForegroundColor Green
    Write-Host "`n5. Starting application with SHARED database..." -ForegroundColor Cyan
    Write-Host "You should now see Krivam's 4 users!" -ForegroundColor Green
    Write-Host "Backend: http://localhost:5066" -ForegroundColor Yellow
    Write-Host "Frontend: http://localhost:4200" -ForegroundColor Yellow
    
    dotnet run
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
}