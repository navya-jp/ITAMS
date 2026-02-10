# Setup SQL Server for Team Sharing
# Run this script as Administrator

Write-Host "=== ITAMS SQL Server Sharing Setup ===" -ForegroundColor Cyan
Write-Host "This will configure your SQL Server for team access" -ForegroundColor Yellow

# Step 1: Run the SQL script to create database and user
Write-Host "`n1. Creating shared database and user..." -ForegroundColor Green

try {
    # Use sqlcmd to run the SQL script
    $sqlScript = "create-shared-database.sql"
    
    if (Test-Path $sqlScript) {
        Write-Host "Running SQL setup script..." -ForegroundColor Yellow
        sqlcmd -S ".\SQLEXPRESS" -E -i $sqlScript
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Database and user created successfully!" -ForegroundColor Green
        } else {
            Write-Host "✗ SQL script execution failed" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "✗ SQL script file not found: $sqlScript" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Error running SQL script: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Restart SQL Server to apply Mixed Mode authentication
Write-Host "`n2. Restarting SQL Server to apply changes..." -ForegroundColor Green

try {
    Write-Host "Stopping SQL Server..." -ForegroundColor Yellow
    Stop-Service -Name "MSSQL`$SQLEXPRESS" -Force
    
    Write-Host "Starting SQL Server..." -ForegroundColor Yellow
    Start-Service -Name "MSSQL`$SQLEXPRESS"
    
    Write-Host "✓ SQL Server restarted successfully!" -ForegroundColor Green
} catch {
    Write-Host "⚠ Could not restart SQL Server automatically. Please restart manually:" -ForegroundColor Yellow
    Write-Host "  1. Open Services (services.msc)" -ForegroundColor Gray
    Write-Host "  2. Find 'SQL Server (SQLEXPRESS)'" -ForegroundColor Gray
    Write-Host "  3. Right-click → Restart" -ForegroundColor Gray
}

# Step 3: Test the connection
Write-Host "`n3. Testing database connection..." -ForegroundColor Green

try {
    $testConnection = "Server=.\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;"
    
    # Simple connection test using .NET
    Add-Type -AssemblyName System.Data
    $connection = New-Object System.Data.SqlClient.SqlConnection($testConnection)
    $connection.Open()
    $connection.Close()
    
    Write-Host "✓ Database connection test successful!" -ForegroundColor Green
} catch {
    Write-Host "✗ Database connection test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check SQL Server configuration" -ForegroundColor Yellow
}

# Step 4: Update appsettings.json
Write-Host "`n4. Updating application configuration..." -ForegroundColor Green

$newConnectionString = "Server=192.168.208.10\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"

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
    "SharedSqlServer": "$newConnectionString"
  }
}
"@

$appsettingsContent | Set-Content "appsettings.json" -Encoding UTF8
Write-Host "✓ appsettings.json updated with shared database connection" -ForegroundColor Green

Write-Host "`n=== Setup Complete! ===" -ForegroundColor Cyan
Write-Host "Your SQL Server is now configured for team sharing:" -ForegroundColor Green
Write-Host "• Database: ITAMS_Shared" -ForegroundColor White
Write-Host "• Server: 192.168.208.10\SQLEXPRESS" -ForegroundColor White
Write-Host "• User: itams_user" -ForegroundColor White
Write-Host "• Password: ITAMS@2024!" -ForegroundColor White

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Test your application: dotnet run" -ForegroundColor Gray
Write-Host "2. Share connection details with teammates" -ForegroundColor Gray
Write-Host "3. Teammates should run: .\team-connect-shared.ps1" -ForegroundColor Gray