# Copy Data from Shared to Local Database - Final Version
# Uses CSV export/import for compatibility

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copying Data to Local Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$sharedServer = "192.168.208.26,1433"
$sharedUser = "itams_user"
$sharedPass = "ITAMS@2024!"
$sharedDb = "ITAMS_Shared"

$localServer = ".\SQLEXPRESS"
$localDb = "ITAMS_Local"

$tables = @("Roles", "RbacRoles", "RbacPermissions", "RbacRolePermissions", "Projects", "Locations", "Users", "UserProjects", "RbacUserPermissions", "RbacUserScopes", "SystemSettings", "Assets")

# Step 1: Disable constraints
Write-Host "Step 1: Disabling constraints..." -ForegroundColor Yellow
sqlcmd -S $localServer -d $localDb -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'" | Out-Null
Write-Host "  Done" -ForegroundColor Green
Write-Host ""

# Step 2: Clear local data
Write-Host "Step 2: Clearing local data..." -ForegroundColor Yellow
foreach ($table in $tables) {
    sqlcmd -S $localServer -d $localDb -Q "DELETE FROM [$table]; DBCC CHECKIDENT ('[$table]', RESEED, 0)" 2>&1 | Out-Null
}
Write-Host "  Done" -ForegroundColor Green
Write-Host ""

# Step 3: Copy data table by table
Write-Host "Step 3: Copying data..." -ForegroundColor Yellow
$i = 0
foreach ($table in $tables) {
    $i++
    Write-Host "  [$i/$($tables.Count)] $table..." -ForegroundColor Gray
    
    # Check row count in shared database
    $countResult = sqlcmd -S $sharedServer -U $sharedUser -P $sharedPass -d $sharedDb -Q "SELECT COUNT(*) FROM [$table]" -h -1
    $rowCount = 0
    if ($countResult -and $countResult.Count -gt 0) {
        $countStr = ($countResult | Select-Object -First 1).ToString().Trim()
        if ($countStr -match '^\d+$') {
            $rowCount = [int]$countStr
        }
    }
    
    if ($rowCount -eq 0) {
        Write-Host "    No data to copy" -ForegroundColor Yellow
        continue
    }
    
    # Get column list
    $columnsResult = sqlcmd -S $sharedServer -U $sharedUser -P $sharedPass -d $sharedDb -Q "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$table' ORDER BY ORDINAL_POSITION" -h -1
    $columns = @()
    foreach ($col in $columnsResult) {
        $colName = $col.ToString().Trim()
        if ($colName -ne "" -and $colName -notmatch "^-+$") {
            $columns += $colName
        }
    }
    
    if ($columns.Count -eq 0) {
        Write-Host "    Could not get columns" -ForegroundColor Red
        continue
    }
    
    $columnList = ($columns | ForEach-Object { "[$_]" }) -join ","
    
    # Export data to CSV
    $csvFile = "temp_$table.csv"
    $exportQuery = "SET NOCOUNT ON; SELECT $columnList FROM [$table]"
    sqlcmd -S $sharedServer -U $sharedUser -P $sharedPass -d $sharedDb -Q $exportQuery -s "|" -W -o $csvFile 2>&1 | Out-Null
    
    if (Test-Path $csvFile) {
        # Read CSV and generate INSERT statements
        $content = Get-Content $csvFile
        $insertCount = 0
        
        # Skip header rows (first 2 lines)
        for ($lineNum = 2; $lineNum -lt $content.Count; $lineNum++) {
            $line = $content[$lineNum].Trim()
            
            # Skip separator lines and empty lines
            if ($line -eq "" -or $line -match "^-+$" -or $line -match "^\s*\(\d+ rows affected\)") {
                continue
            }
            
            # Parse values
            $values = $line -split "\|"
            if ($values.Count -ne $columns.Count) {
                continue
            }
            
            # Build INSERT statement
            $valueList = @()
            for ($j = 0; $j < $values.Count; $j++) {
                $val = $values[$j].Trim()
                if ($val -eq "NULL" -or $val -eq "") {
                    $valueList += "NULL"
                }
                elseif ($val -match "^\d+$" -or $val -match "^\d+\.\d+$") {
                    # Numeric value
                    $valueList += $val
                }
                elseif ($val -eq "True" -or $val -eq "1") {
                    $valueList += "1"
                }
                elseif ($val -eq "False" -or $val -eq "0") {
                    $valueList += "0"
                }
                else {
                    # String value - escape single quotes
                    $escapedVal = $val -replace "'", "''"
                    $valueList += "'$escapedVal'"
                }
            }
            
            $insertQuery = "INSERT INTO [$table] ($columnList) VALUES ($($valueList -join ','))"
            
            try {
                sqlcmd -S $localServer -d $localDb -Q $insertQuery 2>&1 | Out-Null
                $insertCount++
            }
            catch {
                # Skip errors
            }
        }
        
        Write-Host "    Copied $insertCount rows" -ForegroundColor Green
        
        # Cleanup
        Remove-Item $csvFile -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-Host "    Export failed" -ForegroundColor Red
    }
}

Write-Host ""

# Step 4: Re-enable constraints
Write-Host "Step 4: Re-enabling constraints..." -ForegroundColor Yellow
sqlcmd -S $localServer -d $localDb -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'" 2>&1 | Out-Null
Write-Host "  Done" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Show summary
Write-Host "Summary:" -ForegroundColor Cyan
sqlcmd -S $localServer -d $localDb -Q "SELECT 'Users' as [Table], COUNT(*) as [Rows] FROM Users UNION ALL SELECT 'Roles', COUNT(*) FROM Roles UNION ALL SELECT 'Projects', COUNT(*) FROM Projects UNION ALL SELECT 'Locations', COUNT(*) FROM Locations UNION ALL SELECT 'Assets', COUNT(*) FROM Assets UNION ALL SELECT 'RbacRoles', COUNT(*) FROM RbacRoles UNION ALL SELECT 'RbacPermissions', COUNT(*) FROM RbacPermissions"

Write-Host ""
Write-Host "Local database is ready for demo!" -ForegroundColor Green
Write-Host "Server: .\SQLEXPRESS" -ForegroundColor White
Write-Host "Database: ITAMS_Local" -ForegroundColor White
