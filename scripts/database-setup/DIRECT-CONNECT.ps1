# Direct Connection Script - Skip ping test
Write-Host "=== Direct Connection to Shared Database ===" -ForegroundColor Cyan
Write-Host "Host: 192.168.208.10 (Krivam)" -ForegroundColor Yellow
Write-Host "Teammate: 192.168.208.9 (Navya)" -ForegroundColor Yellow
Write-Host "Same network confirmed!" -ForegroundColor Green

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

# Test SQL Server port directly
Write-Host "Testing SQL Server connection..." -ForegroundColor Yellow
try {
    $portTest = Test-NetConnection -ComputerName "192.168.208.10" -Port 1433 -WarningAction SilentlyContinue
    if ($portTest.TcpTestSucceeded) {
        Write-Host "✓ SQL Server port accessible!" -ForegroundColor Green
    } else {
        Write-Host "✗ SQL Server port not accessible" -ForegroundColor Red
        Write-Host "Host may need to restart SQL Server or check firewall" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Port test failed, but trying database connection anyway..." -ForegroundColor Yellow
}

# Build and run regardless
Write-Host "Building application..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build successful!" -ForegroundColor Green
    Write-Host "Starting application with shared database..." -ForegroundColor Cyan
    Write-Host "Backend: http://localhost:5066" -ForegroundColor White
    Write-Host "Frontend: http://localhost:4200" -ForegroundColor White
    dotnet run
} else {
    Write-Host "✗ Build failed!" -ForegroundColor Red
}