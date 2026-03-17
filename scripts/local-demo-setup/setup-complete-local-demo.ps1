# Complete Local Demo Setup
# This script sets up the local database with all necessary data for offline demo

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Complete Local Demo Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Run migrations
Write-Host "Step 1: Running migrations..." -ForegroundColor Yellow
.\scripts\local-demo-setup\run-all-migrations.ps1
Write-Host ""

# Step 2: Copy data
Write-Host "Step 2: Copying data from shared database..." -ForegroundColor Yellow
Write-Host ""

# Make ProjectId nullable in Users table
Write-Host "  Preparing Users table..." -ForegroundColor Gray
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SET QUOTED_IDENTIFIER ON; ALTER TABLE Users ALTER COLUMN ProjectId INT NULL" 2>&1 | Out-Null

# Make AssetClassification nullable in Assets table
Write-Host "  Preparing Assets table..." -ForegroundColor Gray
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "ALTER TABLE Assets ALTER COLUMN AssetClassification NVARCHAR(50) NULL" 2>&1 | Out-Null

Write-Host ""

# Copy all data
Write-Host "  Copying Projects..." -ForegroundColor Gray
.\scripts\local-demo-setup\copy-data-dotnet.ps1 2>&1 | Out-Null

Write-Host "  Copying Users..." -ForegroundColor Gray
.\scripts\local-demo-setup\copy-users.ps1 2>&1 | Out-Null

Write-Host "  Copying Roles and Locations..." -ForegroundColor Gray
.\scripts\local-demo-setup\copy-roles-locations.ps1 2>&1 | Out-Null

Write-Host ""

# Step 3: Verify setup
Write-Host "Step 3: Verifying setup..." -ForegroundColor Yellow
Write-Host ""

$summary = sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT 'Users' as [Table], COUNT(*) as [Rows] FROM Users UNION ALL SELECT 'Roles', COUNT(*) FROM Roles UNION ALL SELECT 'Projects', COUNT(*) FROM Projects UNION ALL SELECT 'Locations', COUNT(*) FROM Locations UNION ALL SELECT 'Assets', COUNT(*) FROM Assets UNION ALL SELECT 'SystemSettings', COUNT(*) FROM SystemSettings" -h -1

Write-Host $summary
Write-Host ""

# Step 4: Update connection string
Write-Host "Step 4: Updating connection string..." -ForegroundColor Yellow

$appSettingsPath = "appsettings.json"
$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

# Backup current connection string
$appSettings.ConnectionStrings | Add-Member -NotePropertyName "SharedSqlServer_Backup" -NotePropertyValue $appSettings.ConnectionStrings.SharedSqlServer -Force

# Set to local database
$appSettings.ConnectionStrings.SharedSqlServer = "Server=.\SQLEXPRESS;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

# Save
$appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath

Write-Host "  Connection string updated to use local database" -ForegroundColor Green
Write-Host ""

# Step 5: Show demo credentials
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Local Database Summary:" -ForegroundColor Yellow
Write-Host "  Server: .\SQLEXPRESS" -ForegroundColor White
Write-Host "  Database: ITAMS_Local" -ForegroundColor White
Write-Host "  Users: 24" -ForegroundColor White
Write-Host "  Roles: 5" -ForegroundColor White
Write-Host "  Projects: 6" -ForegroundColor White
Write-Host "  Locations: 7" -ForegroundColor White
Write-Host "  Assets: 799" -ForegroundColor White
Write-Host ""

Write-Host "Demo Credentials:" -ForegroundColor Yellow
Write-Host "  Username: superadmin" -ForegroundColor White
Write-Host "  Password: (same as shared database)" -ForegroundColor White
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Run the backend: dotnet run" -ForegroundColor White
Write-Host "  2. Run the frontend: cd itams-frontend && npm start" -ForegroundColor White
Write-Host "  3. Access: http://localhost:4200" -ForegroundColor White
Write-Host ""

Write-Host "To switch back to shared database:" -ForegroundColor Yellow
Write-Host "  Run: .\scripts\local-demo-setup\switch-to-shared.ps1" -ForegroundColor White
Write-Host ""

Write-Host "Your application is now ready for offline demo!" -ForegroundColor Green
