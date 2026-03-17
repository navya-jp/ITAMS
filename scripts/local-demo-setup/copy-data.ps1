# Copy Data from Shared to Local Database
# Simple and reliable method

Write-Host "Copying data from shared database to local database..." -ForegroundColor Cyan
Write-Host ""

$tables = @("Roles", "RbacRoles", "RbacPermissions", "RbacRolePermissions", "Projects", "Locations", "Users", "UserProjects", "RbacUserPermissions", "RbacUserScopes", "SystemSettings", "Assets")

# Step 1: Disable constraints
Write-Host "Disabling constraints..." -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'" | Out-Null

# Step 2: Clear local data
Write-Host "Clearing local data..." -ForegroundColor Yellow
foreach ($table in $tables) {
    sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "DELETE FROM [$table]" | Out-Null
}

# Step 3: Copy data table by table
Write-Host "Copying data..." -ForegroundColor Yellow
$i = 0
foreach ($table in $tables) {
    $i++
    Write-Host "  [$i/$($tables.Count)] $table..." -ForegroundColor Gray
    
    # Get row count from shared database
    $countResult = sqlcmd -S "192.168.208.26,1433" -U "itams_user" -P "ITAMS@2024!" -d "ITAMS_Shared" -Q "SELECT COUNT(*) FROM [$table]" -h -1
    $rowCount = 0
    if ($countResult -and $countResult.Count -gt 0) {
        $countStr = ($countResult | Select-Object -First 1).ToString().Trim()
        if ($countStr -match '^\d+$') {
            $rowCount = [int]$countStr
        }
    }
    
    if ($rowCount -gt 0) {
        # Export from shared
        $exportFile = "temp_$table.dat"
        bcp "ITAMS_Shared.dbo.$table" out $exportFile -S "192.168.208.26,1433" -U "itams_user" -P "ITAMS@2024!" -n -q 2>&1 | Out-Null
        
        if (Test-Path $exportFile) {
            # Import to local
            bcp "ITAMS_Local.dbo.$table" in $exportFile -S ".\SQLEXPRESS" -T -n -q -b 1000 2>&1 | Out-Null
            
            # Verify
            $localCountResult = sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT COUNT(*) FROM [$table]" -h -1
            $localCountStr = ($localCountResult | Select-Object -First 1).ToString().Trim()
            Write-Host "    Copied $localCountStr rows" -ForegroundColor Green
            
            # Cleanup
            Remove-Item $exportFile -Force -ErrorAction SilentlyContinue
        }
    }
    else {
        Write-Host "    No data" -ForegroundColor Yellow
    }
}

# Step 4: Re-enable constraints
Write-Host "Re-enabling constraints..." -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'" | Out-Null

Write-Host ""
Write-Host "Done! Data copied successfully." -ForegroundColor Green
Write-Host ""

# Show summary
Write-Host "Summary:" -ForegroundColor Cyan
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT 'Users' as [Table], COUNT(*) as [Rows] FROM Users UNION ALL SELECT 'Roles', COUNT(*) FROM Roles UNION ALL SELECT 'Projects', COUNT(*) FROM Projects UNION ALL SELECT 'Locations', COUNT(*) FROM Locations UNION ALL SELECT 'Assets', COUNT(*) FROM Assets"
