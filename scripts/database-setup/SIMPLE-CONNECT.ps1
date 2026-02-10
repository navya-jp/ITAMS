# SIMPLE CONNECTION TO SHARED DATABASE
Write-Host "Connecting to shared database..." -ForegroundColor Green

# Update appsettings.json
$content = @'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SharedSqlServer": "Server=192.168.208.10,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
'@

$content | Out-File "appsettings.json" -Encoding UTF8
Write-Host "Database connection updated!" -ForegroundColor Green

# Build and run
Write-Host "Building..." -ForegroundColor Yellow
dotnet build

Write-Host "Starting application..." -ForegroundColor Yellow
dotnet run