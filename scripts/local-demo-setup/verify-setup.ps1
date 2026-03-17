# Verify Local Database Setup
# Checks if everything is configured correctly for demo

param(
    [string]$ServerName = "localhost",
    [string]$DatabaseName = "ITAMS_Local"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ITAMS Demo Setup Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Check 1: SQL Server Running
Write-Host "[1/7] Checking SQL Server..." -ForegroundColor Yellow
try {
    $version = sqlcmd -S $ServerName -Q "SELECT @@VERSION" -h -1 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ SQL Server is accessible" -ForegroundColor Green
    }
    else {
        Write-Host "  ✗ Cannot connect to SQL Server" -ForegroundColor Red
        Write-Host "    Try: localhost, (localdb)\MSSQLLocalDB, or .\SQLEXPRESS" -ForegroundColor Yellow
        $allGood = $false
    }
}
catch {
    Write-Host "  ✗ SQL Server not found" -ForegroundColor Red
    $allGood = $false
}

# Check 2: Database Exists
Write-Host ""
Write-Host "[2/7] Checking database..." -ForegroundColor Yellow
try {
    $dbExists = sqlcmd -S $ServerName -Q "SELECT name FROM sys.databases WHERE name='$DatabaseName'" -h -1
    if ($dbExists -match $DatabaseName) {
        Write-Host "  ✓ Database '$DatabaseName' exists" -ForegroundColor Green
    }
    else {
        Write-Host "  ✗ Database '$DatabaseName' not found" -ForegroundColor Red
        Write-Host "    Run: sqlcmd -S $ServerName -Q `"CREATE DATABASE $DatabaseName`"" -ForegroundColor Yellow
        $allGood = $false
    }
}
catch {
    Write-Host "  ✗ Cannot check database" -ForegroundColor Red
    $allGood = $false
}

# Check 3: Tables Exist
Write-Host ""
Write-Host "[3/7] Checking tables..." -ForegroundColor Yellow
try {
    $tableCount = sqlcmd -S $ServerName -d $DatabaseName -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'" -h -1
    if ($tableCount -gt 20) {
        Write-Host "  ✓ Found $tableCount tables" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠ Only $tableCount tables found" -ForegroundColor Yellow
        Write-Host "    Run: dotnet ef database update" -ForegroundColor Yellow
        $allGood = $false
    }
}
catch {
    Write-Host "  ✗ Cannot check tables" -ForegroundColor Red
    $allGood = $false
}

# Check 4: Data Exists
Write-Host ""
Write-Host "[4/7] Checking data..." -ForegroundColor Yellow
try {
    $dataCheck = sqlcmd -S $ServerName -d $DatabaseName -Q @"
SELECT 
    (SELECT COUNT(*) FROM Users) as Users,
    (SELECT COUNT(*) FROM Roles) as Roles,
    (SELECT COUNT(*) FROM Projects) as Projects,
    (SELECT COUNT(*) FROM Locations) as Locations,
    (SELECT COUNT(*) FROM Assets) as Assets
"@ -h -1 -W
    
    Write-Host "  Data counts:" -ForegroundColor Gray
    Write-Host $dataCheck
    
    if ($dataCheck -match "0.*0.*0.*0.*0") {
        Write-Host "  ⚠ No data found" -ForegroundColor Yellow
        Write-Host "    Run: .\scripts\local-demo-setup\export-data-simple.ps1" -ForegroundColor Yellow
        Write-Host "    Then: sqlcmd -S $ServerName -d $DatabaseName -i ITAMS_Data_Export.sql" -ForegroundColor Yellow
    }
    else {
        Write-Host "  ✓ Data exists" -ForegroundColor Green
    }
}
catch {
    Write-Host "  ✗ Cannot check data" -ForegroundColor Red
}

# Check 5: Connection String
Write-Host ""
Write-Host "[5/7] Checking connection string..." -ForegroundColor Yellow
$appsettingsPath = ".\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connString = $appsettings.ConnectionStrings.DefaultConnection
    
    if ($connString -match $DatabaseName) {
        Write-Host "  ✓ Connection string points to $DatabaseName" -ForegroundColor Green
    }
    elseif ($connString -match "ITAMS_Shared") {
        Write-Host "  ⚠ Still using shared database" -ForegroundColor Yellow
        Write-Host "    Update appsettings.json to use local database" -ForegroundColor Yellow
    }
    else {
        Write-Host "  ⚠ Connection string: $connString" -ForegroundColor Yellow
    }
}
else {
    Write-Host "  ✗ appsettings.json not found" -ForegroundColor Red
    $allGood = $false
}

# Check 6: Application Builds
Write-Host ""
Write-Host "[6/7] Checking application..." -ForegroundColor Yellow
try {
    $buildResult = dotnet build --no-restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Application builds successfully" -ForegroundColor Green
    }
    else {
        Write-Host "  ✗ Build failed" -ForegroundColor Red
        $allGood = $false
    }
}
catch {
    Write-Host "  ⚠ Cannot check build" -ForegroundColor Yellow
}

# Check 7: Demo Credentials
Write-Host ""
Write-Host "[7/7] Checking demo credentials..." -ForegroundColor Yellow
try {
    $superadmin = sqlcmd -S $ServerName -d $DatabaseName -Q "SELECT Username FROM Users WHERE Username='superadmin'" -h -1
    if ($superadmin -match "superadmin") {
        Write-Host "  ✓ SuperAdmin account exists" -ForegroundColor Green
        Write-Host "    Login: superadmin / Admin@123" -ForegroundColor Gray
    }
    else {
        Write-Host "  ⚠ SuperAdmin account not found" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  ⚠ Cannot check credentials" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "✓ Setup Verified - Ready for Demo!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To start the application:" -ForegroundColor Yellow
    Write-Host "  dotnet run" -ForegroundColor White
    Write-Host ""
    Write-Host "Then open:" -ForegroundColor Yellow
    Write-Host "  http://localhost:4200" -ForegroundColor White
    Write-Host ""
    Write-Host "Login with:" -ForegroundColor Yellow
    Write-Host "  Username: superadmin" -ForegroundColor White
    Write-Host "  Password: Admin@123" -ForegroundColor White
}
else {
    Write-Host "⚠ Issues Found - Review Above" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Common fixes:" -ForegroundColor Yellow
    Write-Host "1. Create database: sqlcmd -S localhost -Q `"CREATE DATABASE ITAMS_Local`"" -ForegroundColor White
    Write-Host "2. Run migrations: dotnet ef database update" -ForegroundColor White
    Write-Host "3. Import data: sqlcmd -S localhost -d ITAMS_Local -i ITAMS_Data_Export.sql" -ForegroundColor White
    Write-Host "4. Update appsettings.json connection string" -ForegroundColor White
}
Write-Host ""
