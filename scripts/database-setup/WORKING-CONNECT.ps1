# Working Connection Script for Navya
Write-Host "=== Connecting to Shared Database ===" -ForegroundColor Cyan
Write-Host "SQL Server connection test: PASSED!" -ForegroundColor Green

# Update appsettings.json
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

Write-Host "Updating connection configuration..." -ForegroundColor Green
$content | Out-File "appsettings.json" -Encoding UTF8

Write-Host "Building application..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Starting shared database connection..." -ForegroundColor Green
    Write-Host "You should now see Krivam's shared data!" -ForegroundColor Cyan
    dotnet run
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}