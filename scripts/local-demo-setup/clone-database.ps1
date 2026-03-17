# Clone Shared Database to Local
# This script copies all data from shared database to local database

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cloning Shared Database to Local" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$sharedServer = "192.168.208.26,1433"
$sharedDb = "ITAMS_Shared"
$sharedUser = "itams_user"
$sharedPass = "ITAMS@2024!"

$localServer = ".\SQLEXPRESS"
$localDb = "ITAMS_Local"

# Tables to copy in order (respecting foreign keys)
$tables = @(
    "Roles",
    "RbacRoles",
    "RbacPermissions",
    "RbacRolePermissions",
    "Projects",
    "Locations",
    "Users",
    "UserProjects",
    "RbacUserPermissions",
    "RbacUserScopes",
    "SystemSettings",
    "Assets"
)

Write-Host "Step 1: Clearing local database..." -ForegroundColor Yellow

# Disable constraints
sqlcmd -S $localServer -d $localDb -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'" | Out-Null

# Clear existing data
foreach ($table in $tables) {
    Write-Host "  Clearing $table..." -ForegroundColor Gray
    sqlcmd -S $localServer -d $localDb -Q "DELETE FROM [$table]" | Out-Null
}

Write-Host "  ✓ Local database cleared" -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Copying data from shared database..." -ForegroundColor Yellow

$totalTables = $tables.Count
$current = 0

foreach ($table in $tables) {
    $current++
    Write-Host "  [$current/$totalTables] Copying $table..." -ForegroundColor Gray
    
    # Export from shared database using bcp
    $exportFile = ".\temp_$table.dat"
    $formatFile = ".\temp_$table.fmt"
    
    # Export data
    bcp "ITAMS_Shared.dbo.$table" out $exportFile -S $sharedServer -U $sharedUser -P $sharedPass -n -q 2>&1 | Out-Null
    
    if (Test-Path $exportFile) {
        # Import to local database
        bcp "ITAMS_Local.dbo.$table" in $exportFile -S $localServer -T -n -q -b 1000 2>&1 | Out-Null
        
        # Get row count
        $count = sqlcmd -S $localServer -d $localDb -Q "SELECT COUNT(*) FROM [$table]" -h -1
        Write-Host "    ✓ Copied $($count.Trim()) rows" -ForegroundColor Green
        
        # Cleanup
        Remove-Item $exportFile -Force -ErrorAction SilentlyContinue
        Remove-Item $formatFile -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-Host "    ⚠ No data to copy" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Step 3: Re-enabling constraints..." -ForegroundColor Yellow
sqlcmd -S $localServer -d $localDb -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'" | Out-Null
Write-Host "  ✓ Constraints enabled" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Clone Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Show summary
Write-Host "Data Summary:" -ForegroundColor Yellow
$summaryQuery = @"
SELECT 'Users' as [Table], COUNT(*) as [Rows] FROM Users 
UNION ALL SELECT 'Roles', COUNT(*) FROM Roles 
UNION ALL SELECT 'Projects', COUNT(*) FROM Projects 
UNION ALL SELECT 'Locations', COUNT(*) FROM Locations 
UNION ALL SELECT 'Assets', COUNT(*) FROM Assets
"@
sqlcmd -S $localServer -d $localDb -Q $summaryQuery

Write-Host ""
Write-Host "Local database is ready for demo!" -ForegroundColor Green
Write-Host "Server: .\SQLEXPRESS" -ForegroundColor White
Write-Host "Database: ITAMS_Local" -ForegroundColor White
