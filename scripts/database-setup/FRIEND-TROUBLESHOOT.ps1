# 🔧 ITAMS Troubleshooting Script for Friends
# Run this if you can't see users or forms look different

Write-Host "=== ITAMS Troubleshooting Guide ===" -ForegroundColor Cyan
Write-Host "Let's fix the issues step by step..." -ForegroundColor Yellow

# Step 1: Check network connection to shared database
Write-Host "`n1. Testing connection to shared database..." -ForegroundColor Green
$ping = Test-Connection -ComputerName "192.168.208.10" -Count 2 -Quiet

if ($ping) {
    Write-Host "✅ Network connection to database server is working!" -ForegroundColor Green
} else {
    Write-Host "❌ Cannot reach database server (192.168.208.10)" -ForegroundColor Red
    Write-Host "   This means you're not on the same network as the database." -ForegroundColor Yellow
    Write-Host "   Solutions:" -ForegroundColor Yellow
    Write-Host "   - Connect to the same WiFi/network as the database server" -ForegroundColor White
    Write-Host "   - Use VPN if connecting remotely" -ForegroundColor White
    Write-Host "   - Ask for database server IP address" -ForegroundColor White
    exit 1
}

# Step 2: Check if correct connection string is set
Write-Host "`n2. Checking database connection string..." -ForegroundColor Green
$appsettings = Get-Content "appsettings.json" | ConvertFrom-Json
$connectionString = $appsettings.ConnectionStrings.SharedSqlServer

if ($connectionString -like "*192.168.208.10*") {
    Write-Host "✅ Connection string is correct for shared database" -ForegroundColor Green
} else {
    Write-Host "❌ Connection string is not set for shared database" -ForegroundColor Red
    Write-Host "   Fixing connection string..." -ForegroundColor Yellow
    
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
    $content | Out-File "appsettings.json" -Encoding UTF8
    Write-Host "✅ Connection string updated!" -ForegroundColor Green
}

# Step 3: Test database connection
Write-Host "`n3. Testing database connection..." -ForegroundColor Green
try {
    $result = sqlcmd -S "192.168.208.10,1433" -U "itams_user" -P "ITAMS@2024!" -Q "SELECT COUNT(*) FROM Users" -h -1
    if ($result -match '\d+') {
        Write-Host "✅ Database connection successful! Found $result users in database" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Database connection failed" -ForegroundColor Red
    Write-Host "   Make sure SQL Server tools are installed" -ForegroundColor Yellow
}

# Step 4: Check .NET version
Write-Host "`n4. Checking .NET version..." -ForegroundColor Green
$dotnetVersion = dotnet --version
Write-Host "   .NET Version: $dotnetVersion" -ForegroundColor White

if ($dotnetVersion -like "10.*") {
    Write-Host "✅ .NET 10 is installed" -ForegroundColor Green
} else {
    Write-Host "❌ .NET 10 is required. Please install .NET 10 SDK" -ForegroundColor Red
}

# Step 5: Check Node.js version
Write-Host "`n5. Checking Node.js version..." -ForegroundColor Green
$nodeVersion = node --version
Write-Host "   Node.js Version: $nodeVersion" -ForegroundColor White

if ($nodeVersion -like "v1*" -or $nodeVersion -like "v2*") {
    Write-Host "✅ Node.js version is compatible" -ForegroundColor Green
} else {
    Write-Host "❌ Node.js 18+ is required" -ForegroundColor Red
}

# Step 6: Build and run backend
Write-Host "`n6. Building backend..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Backend build successful!" -ForegroundColor Green
    
    Write-Host "`n7. Starting backend on port 5068..." -ForegroundColor Green
    Write-Host "   Backend will run on: http://localhost:5068" -ForegroundColor Cyan
    Write-Host "   Press Ctrl+C to stop and continue to frontend setup" -ForegroundColor Yellow
    
    Start-Process powershell -ArgumentList "-Command", "dotnet run --urls 'http://localhost:5068'"
    Start-Sleep 3
    
    # Test backend API
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5068/api/users" -Method Get
        Write-Host "✅ Backend API is responding!" -ForegroundColor Green
        Write-Host "   Found $($response.data.Count) users" -ForegroundColor Cyan
    } catch {
        Write-Host "❌ Backend API is not responding yet. Wait a moment and try again." -ForegroundColor Red
    }
    
} else {
    Write-Host "❌ Backend build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n8. Frontend Setup Instructions:" -ForegroundColor Green
Write-Host "   Open a NEW terminal/command prompt and run:" -ForegroundColor Yellow
Write-Host "   cd itams-frontend" -ForegroundColor White
Write-Host "   npm install" -ForegroundColor White
Write-Host "   ng serve --port 4202" -ForegroundColor White
Write-Host "   Then go to: http://localhost:4202" -ForegroundColor Cyan

Write-Host "`n🎉 Setup should be complete!" -ForegroundColor Green
Write-Host "   Backend: http://localhost:5068" -ForegroundColor Cyan
Write-Host "   Frontend: http://localhost:4202" -ForegroundColor Cyan
Write-Host "   You should now see all users and the updated forms!" -ForegroundColor Green