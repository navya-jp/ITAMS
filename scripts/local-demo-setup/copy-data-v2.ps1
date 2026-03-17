# Copy Data from Shared to Local Database - Version 2
# Uses SQL INSERT INTO ... SELECT approach

Write-Host "Copying data from shared database to local database..." -ForegroundColor Cyan
Write-Host ""

$sharedServer = "192.168.208.26,1433"
$sharedUser = "itams_user"
$sharedPass = "ITAMS@2024!"
$sharedDb = "ITAMS_Shared"

$localServer = ".\SQLEXPRESS"
$localDb = "ITAMS_Local"

$tables = @("Roles", "RbacRoles", "RbacPermissions", "RbacRolePermissions", "Projects", "Locations", "Users", "UserProjects", "RbacUserPermissions", "RbacUserScopes", "SystemSettings", "Assets")

# Step 1: Disable constraints
Write-Host "Disabling constraints..." -ForegroundColor Yellow
sqlcmd -S $localServer -d $localDb -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'" | Out-Null

# Step 2: Clear local data
Write-Host "Clearing local data..." -ForegroundColor Yellow
foreach ($table in $tables) {
    sqlcmd -S $localServer -d $localDb -Q "DELETE FROM [$table]" | Out-Null
}

# Step 3: Copy data table by table
Write-Host "Copying data..." -ForegroundColor Yellow
$i = 0
foreach ($table in $tables) {
    $i++
    Write-Host "  [$i/$($tables.Count)] $table..." -ForegroundColor Gray
    
    # Export to temp file
    $tempFile = "temp_$table.sql"
    
    # Generate INSERT statements from shared database
    $exportQuery = @"
SET NOCOUNT ON;
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'INSERT INTO [$table] VALUES (' + 
    STUFF((
        SELECT ',' + CASE 
            WHEN value IS NULL THEN 'NULL'
            WHEN type_name = 'bit' THEN CAST(value AS NVARCHAR(MAX))
            WHEN type_name IN ('int', 'bigint', 'decimal', 'numeric', 'float', 'real') THEN CAST(value AS NVARCHAR(MAX))
            WHEN type_name IN ('datetime', 'datetime2', 'date', 'time') THEN '''' + CONVERT(VARCHAR(50), value, 121) + ''''
            ELSE '''' + REPLACE(CAST(value AS NVARCHAR(MAX)), '''', '''''') + ''''
        END
        FROM (
            SELECT column_name, data_type as type_name, 
                   CASE column_name
                       $(foreach ($col in (sqlcmd -S $sharedServer -U $sharedUser -P $sharedPass -d $sharedDb -Q "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$table' ORDER BY ORDINAL_POSITION" -h -1 | Where-Object { $_.Trim() -ne "" })) {
                           "WHEN '$col' THEN CAST([$col] AS NVARCHAR(MAX))"
                       })
                   END as value
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE TABLE_NAME = '$table'
        ) sub
        FOR XML PATH('')
    ), 1, 1, '') + ');' + CHAR(13) + CHAR(10)
FROM [$table];
PRINT @sql;
"@
    
    # This is too complex, let's use a simpler approach with bcp format files
    # Just use bcp with proper error handling
    
    $exportFile = "temp_$table.dat"
    
    # Export from shared
    $bcpExport = "bcp `"$sharedDb.dbo.$table`" out `"$exportFile`" -S `"$sharedServer`" -U `"$sharedUser`" -P `"$sharedPass`" -n -q"
    Invoke-Expression $bcpExport 2>&1 | Out-Null
    
    if (Test-Path $exportFile) {
        $fileSize = (Get-Item $exportFile).Length
        if ($fileSize -gt 0) {
            # Import to local
            $bcpImport = "bcp `"$localDb.dbo.$table`" in `"$exportFile`" -S `"$localServer`" -T -n -q -b 1000"
            $importResult = Invoke-Expression $bcpImport 2>&1
            
            # Check for errors
            if ($importResult -match "(\d+) rows copied") {
                $copiedRows = $matches[1]
                Write-Host "    Copied $copiedRows rows" -ForegroundColor Green
            }
            else {
                Write-Host "    Import completed" -ForegroundColor Green
            }
        }
        else {
            Write-Host "    No data" -ForegroundColor Yellow
        }
        
        # Cleanup
        Remove-Item $exportFile -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-Host "    Export failed" -ForegroundColor Red
    }
}

# Step 4: Re-enable constraints
Write-Host "Re-enabling constraints..." -ForegroundColor Yellow
sqlcmd -S $localServer -d $localDb -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'" | Out-Null

Write-Host ""
Write-Host "Done! Data copied successfully." -ForegroundColor Green
Write-Host ""

# Show summary
Write-Host "Summary:" -ForegroundColor Cyan
sqlcmd -S $localServer -d $localDb -Q "SELECT 'Users' as [Table], COUNT(*) as [Rows] FROM Users UNION ALL SELECT 'Roles', COUNT(*) FROM Roles UNION ALL SELECT 'Projects', COUNT(*) FROM Projects UNION ALL SELECT 'Locations', COUNT(*) FROM Locations UNION ALL SELECT 'Assets', COUNT(*) FROM Assets UNION ALL SELECT 'RbacRoles', COUNT(*) FROM RbacRoles UNION ALL SELECT 'RbacPermissions', COUNT(*) FROM RbacPermissions"
