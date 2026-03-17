# Setup Local Database for Demo
# This script creates a local database and imports data

param(
    [string]$ServerName = "localhost",
    [string]$DatabaseName = "ITAMS_Local",
    [string]$DataFolder = ".\scripts\local-demo-setup\exported-data"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ITAMS Local Database Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if SQL Server is running
Write-Host "[1/5] Checking SQL Server..." -ForegroundColor Yellow
try {
    $service = Get-Service MSSQLSERVER -ErrorAction SilentlyContinue
    if ($service -and $service.Status -eq "Running") {
        Write-Host "  ✓ SQL Server is running" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠ SQL Server is not running. Attempting to start..." -ForegroundColor Yellow
        Start-Service MSSQLSERVER
        Write-Host "  ✓ SQL Server started" -ForegroundColor Green
    }
}
catch {
    Write-Host "  ⚠ Could not check SQL Server service. Continuing anyway..." -ForegroundColor Yellow
}

# Step 2: Drop existing database if it exists
Write-Host ""
Write-Host "[2/5] Preparing database..." -ForegroundColor Yellow
$dropDbScript = @"
IF EXISTS (SELECT name FROM sys.databases WHERE name = '$DatabaseName')
BEGIN
    ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$DatabaseName];
    PRINT 'Dropped existing database';
END
CREATE DATABASE [$DatabaseName];
PRINT 'Created new database';
"@

try {
    sqlcmd -S $ServerName -Q $dropDbScript -b
    Write-Host "  ✓ Database prepared" -ForegroundColor Green
}
catch {
    Write-Host "  ✗ Error preparing database: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Run Entity Framework migrations
Write-Host ""
Write-Host "[3/5] Running database migrations..." -ForegroundColor Yellow
$env:ASPNETCORE_ENVIRONMENT = "Development"
$connectionString = "Server=$ServerName;Database=$DatabaseName;Trusted_Connection=True;TrustServerCertificate=True"

# Update appsettings temporarily
$appsettingsPath = ".\appsettings.json"
$appsettingsBackup = ".\appsettings.json.backup"

if (Test-Path $appsettingsPath) {
    Copy-Item $appsettingsPath $appsettingsBackup -Force
    
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $appsettings.ConnectionStrings.DefaultConnection = $connectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    
    Write-Host "  Running migrations..." -ForegroundColor Gray
    dotnet ef database update --no-build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Migrations completed" -ForegroundColor Green
    }
    else {
        Write-Host "  ✗ Migration failed" -ForegroundColor Red
        # Restore backup
        Move-Item $appsettingsBackup $appsettingsPath -Force
        exit 1
    }
    
    # Restore backup
    Move-Item $appsettingsBackup $appsettingsPath -Force
}

# Step 4: Import data
Write-Host ""
Write-Host "[4/5] Importing data..." -ForegroundColor Yellow

if (Test-Path $DataFolder) {
    $sqlFiles = Get-ChildItem -Path $DataFolder -Filter "*.sql" | Sort-Object Name
    
    if ($sqlFiles.Count -eq 0) {
        Write-Host "  ⚠ No data files found. Run export-shared-data.ps1 first." -ForegroundColor Yellow
    }
    else {
        foreach ($file in $sqlFiles) {
            Write-Host "  Importing $($file.Name)..." -ForegroundColor Gray
            try {
                sqlcmd -S $ServerName -d $DatabaseName -i $file.FullName -b
                Write-Host "    ✓ Imported" -ForegroundColor Green
            }
            catch {
                Write-Host "    ✗ Error: $_" -ForegroundColor Red
            }
        }
    }
}
else {
    Write-Host "  ⚠ Data folder not found: $DataFolder" -ForegroundColor Yellow
    Write-Host "  Run export-shared-data.ps1 first to export data." -ForegroundColor Yellow
}

# Step 5: Verify setup
Write-Host ""
Write-Host "[5/5] Verifying setup..." -ForegroundColor Yellow

$verifyScript = @"
USE [$DatabaseName];
SELECT 
    'Users' as TableName, COUNT(*) as RowCount FROM Users
UNION ALL
SELECT 'Roles', COUNT(*) FROM Roles
UNION ALL
SELECT 'Projects', COUNT(*) FROM Projects
UNION ALL
SELECT 'Locations', COUNT(*) FROM Locations
UNION ALL
SELECT 'Assets', COUNT(*) FROM Assets;
"@

try {
    $result = sqlcmd -S $ServerName -Q $verifyScript -h -1
    Write-Host ""
    Write-Host "  Data Summary:" -ForegroundColor Cyan
    Write-Host $result
    Write-Host ""
    Write-Host "  ✓ Setup verified" -ForegroundColor Green
}
catch {
    Write-Host "  ⚠ Could not verify setup" -ForegroundColor Yellow
}

# Final instructions
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Local Database Details:" -ForegroundColor Yellow
Write-Host "  Server: $ServerName" -ForegroundColor White
Write-Host "  Database: $DatabaseName" -ForegroundColor White
Write-Host ""
Write-Host "Connection String:" -ForegroundColor Yellow
Write-Host "  Server=$ServerName;Database=$DatabaseName;Trusted_Connection=True;TrustServerCertificate=True" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Update appsettings.json with the connection string above" -ForegroundColor White
Write-Host "2. Run: dotnet run" -ForegroundColor White
Write-Host "3. Login with: superadmin / Admin@123" -ForegroundColor White
Write-Host ""
