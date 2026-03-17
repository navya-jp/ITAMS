# Clone Shared Database to Local - Simple Version
# This script copies all data from shared database to local database using sqlcmd

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
$disableConstraints = "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'"
sqlcmd -S $localServer -d $localDb -Q $disableConstraints | Out-Null

# Clear existing data
foreach ($table in $tables) {
    Write-Host "  Clearing $table..." -ForegroundColor Gray
    $deleteQuery = "DELETE FROM [$table]"
    sqlcmd -S $localServer -d $localDb -Q $deleteQuery | Out-Null
}

Write-Host "  Done clearing local database" -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Copying data from shared database..." -ForegroundColor Yellow

$totalTables = $tables.Count
$current = 0

foreach ($table in $tables) {
    $current++
    Write-Host "  [$current/$totalTables] Copying $table..." -ForegroundColor Gray
    
    # Get data from shared database
    $exportQuery = "SET NOCOUNT ON; SELECT * FROM [$table]"
    $tempFile = "temp_$table.csv"
    
    # Export to CSV
    sqlcmd -S $sharedServer -U $sharedUser -P $sharedPass -d $sharedDb -Q $exportQuery -s "," -W -o $tempFile
    
    if (Test-Path $tempFile) {
        $fileSize = (Get-Item $tempFile).Length
        if ($fileSize -gt 100) {
            # Use BULK INSERT to import data
            $bulkInsertQuery = "BULK INSERT [$table] FROM '$((Get-Location).Path)\$tempFile' WITH (FIELDTERMINATOR = ',', ROWTERMINATOR = '\n', FIRSTROW = 2)"
            
            try {
                sqlcmd -S $localServer -d $localDb -Q $bulkInsertQuery 2>&1 | Out-Null
                
                # Get row count
                $countQuery = "SELECT COUNT(*) FROM [$table]"
                $count = sqlcmd -S $localServer -d $localDb -Q $countQuery -h -1
                Write-Host "    Copied $($count.Trim()) rows" -ForegroundColor Green
            }
            catch {
                Write-Host "    Warning: Could not import $table" -ForegroundColor Yellow
            }
        }
        else {
            Write-Host "    No data to copy" -ForegroundColor Yellow
        }
        
        # Cleanup
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-Host "    No data to copy" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Step 3: Re-enabling constraints..." -ForegroundColor Yellow
$enableConstraints = "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'"
sqlcmd -S $localServer -d $localDb -Q $enableConstraints | Out-Null
Write-Host "  Done re-enabling constraints" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Clone Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Show summary
Write-Host "Data Summary:" -ForegroundColor Yellow
$summaryQuery = "SELECT 'Users' as TableName, COUNT(*) as RowCount FROM Users UNION ALL SELECT 'Roles', COUNT(*) FROM Roles UNION ALL SELECT 'Projects', COUNT(*) FROM Projects UNION ALL SELECT 'Locations', COUNT(*) FROM Locations UNION ALL SELECT 'Assets', COUNT(*) FROM Assets"
sqlcmd -S $localServer -d $localDb -Q $summaryQuery

Write-Host ""
Write-Host "Local database is ready for demo!" -ForegroundColor Green
Write-Host "Server: .\SQLEXPRESS" -ForegroundColor White
Write-Host "Database: ITAMS_Local" -ForegroundColor White
