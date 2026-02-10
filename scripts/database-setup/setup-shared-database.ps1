# 🗄️ ITAMS Shared Database Setup Script

Write-Host "=== ITAMS Shared Database Setup ===" -ForegroundColor Cyan

# Step 1: Get server IP
Write-Host "`n1. Getting your server IP address..." -ForegroundColor Yellow
$ipAddress = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*"} | Select-Object -First 1).IPAddress
Write-Host "Your server IP: $ipAddress" -ForegroundColor Green

# Step 2: Update appsettings.json
Write-Host "`n2. Updating appsettings.json..." -ForegroundColor Yellow
$appsettingsPath = "appsettings.json"
$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json

# Update the SharedSqlServer connection string
$newConnectionString = "Server=$ipAddress,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
$appsettings.ConnectionStrings.SharedSqlServer = $newConnectionString

# Save updated appsettings.json
$appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
Write-Host "✓ Updated connection string with IP: $ipAddress" -ForegroundColor Green

# Step 3: Test database connection
Write-Host "`n3. Testing database connection..." -ForegroundColor Yellow
try {
    dotnet ef database update
    Write-Host "✓ Database connection successful!" -ForegroundColor Green
} catch {
    Write-Host "✗ Database connection failed. Please check SQL Server setup." -ForegroundColor Red
    Write-Host "Make sure SQL Server is running and accessible on port 1433" -ForegroundColor Yellow
}

# Step 4: Display connection info for team
Write-Host "`n=== Share This Info With Your Team ===" -ForegroundColor Cyan
Write-Host "Server IP: $ipAddress" -ForegroundColor White
Write-Host "Database: ITAMS" -ForegroundColor White
Write-Host "Username: itams_user" -ForegroundColor White
Write-Host "Password: ITAMS@2024!" -ForegroundColor White
Write-Host "Port: 1433" -ForegroundColor White

Write-Host "`nYour team should update their appsettings.json with:" -ForegroundColor Yellow
Write-Host "`"SharedSqlServer`": `"$newConnectionString`"" -ForegroundColor Gray

Write-Host "`n=== Next Steps ===" -ForegroundColor Cyan
Write-Host "1. Make sure SQL Server Express is installed and running" -ForegroundColor White
Write-Host "2. Ensure port 1433 is open in Windows Firewall" -ForegroundColor White
Write-Host "3. Share the connection details above with your team" -ForegroundColor White
Write-Host "4. Run 'dotnet run' to start the server" -ForegroundColor White

Write-Host "`n✨ Setup complete! Your ITAMS system is now using shared SQL Server!" -ForegroundColor Green