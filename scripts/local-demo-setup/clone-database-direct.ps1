# Clone Shared Database to Local - Direct SQL Method
# This script copies all data using direct SQL INSERT statements

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cloning Shared Database to Local" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$sharedConn = "Server=192.168.208.26,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"
$localConn = "Server=.\SQLEXPRESS;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=true"

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
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'" | Out-Null

# Clear existing data
foreach ($table in $tables) {
    Write-Host "  Clearing $table..." -ForegroundColor Gray
    sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "DELETE FROM [$table]" | Out-Null
}

Write-Host "  Done" -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Copying data from shared database..." -ForegroundColor Yellow

$totalTables = $tables.Count
$current = 0

foreach ($table in $tables) {
    $current++
    Write-Host "  [$current/$totalTables] Copying $table..." -ForegroundColor Gray
    
    # Get column names
    $columnsQuery = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$table' ORDER BY ORDINAL_POSITION"
    $columns = sqlcmd -S "192.168.208.26,1433" -U "itams_user" -P "ITAMS@2024!" -d "ITAMS_Shared" -Q $columnsQuery -h -1 -W
    $columnList = ($columns | Where-Object { $_.Trim() -ne "" }) -join ","
    
    # Check if table has data
    $countQuery = "SELECT COUNT(*) FROM [$table]"
    $count = sqlcmd -S "192.168.208.26,1433" -U "itams_user" -P "ITAMS@2024!" -d "ITAMS_Shared" -Q $countQuery -h -1
    $rowCount = [int]($count.Trim())
    
    if ($rowCount -gt 0) {
        # Use linked server approach or direct copy
        $copyQuery = @"
SET IDENTITY_INSERT [$table] ON;

INSERT INTO [$table]
SELECT * FROM OPENROWSET(
    'SQLNCLI',
    'Server=192.168.208.26,1433;UID=itams_user;PWD=ITAMS@2024!;',
    'SELECT * FROM ITAMS_Shared.dbo.[$table]'
);

SET IDENTITY_INSERT [$table] OFF;
"@
        
        try {
            sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q $copyQuery 2>&1 | Out-Null
            Write-Host "    Copied $rowCount rows" -ForegroundColor Green
        }
        catch {
            Write-Host "    Warning: Could not copy using OPENROWSET, trying alternative method..." -ForegroundColor Yellow
            
            # Alternative: Export to file and import
            $exportFile = "temp_$table.sql"
            $exportQuery = "SET NOCOUNT ON; SELECT * FROM [$table] FOR XML PATH('row'), ROOT('data')"
            sqlcmd -S "192.168.208.26,1433" -U "itams_user" -P "ITAMS@2024!" -d "ITAMS_Shared" -Q $exportQuery -o $exportFile -h -1
            
            if (Test-Path $exportFile) {
                Write-Host "    Exported to file, manual import needed" -ForegroundColor Yellow
                Remove-Item $exportFile -Force -ErrorAction SilentlyContinue
            }
        }
    }
    else {
        Write-Host "    No data to copy" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Step 3: Re-enabling constraints..." -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'" | Out-Null
Write-Host "  Done" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Clone Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Show summary
Write-Host "Data Summary:" -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT 'Users' as TableName, COUNT(*) as RowCount FROM Users UNION ALL SELECT 'Roles', COUNT(*) FROM Roles UNION ALL SELECT 'Projects', COUNT(*) FROM Projects UNION ALL SELECT 'Locations', COUNT(*) FROM Locations UNION ALL SELECT 'Assets', COUNT(*) FROM Assets"

Write-Host ""
Write-Host "Local database is ready!" -ForegroundColor Green
