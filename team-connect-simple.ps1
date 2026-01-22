# Simple connection script for team members
param([string]$IP = "192.168.208.10")

Write-Host "Connecting to ITAMS Database at $IP..." -ForegroundColor Green

# Update appsettings.json
$json = Get-Content "appsettings.json" | ConvertFrom-Json
$json.ConnectionStrings.SharedSqlServer = "Server=$IP,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
$json | ConvertTo-Json -Depth 10 | Set-Content "appsettings.json"

Write-Host "Updated connection string. Starting application..." -ForegroundColor Yellow

# Build and run
dotnet build
dotnet run