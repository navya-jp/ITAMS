# Copy Data using .NET SqlClient - Most Reliable Method

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copying Data to Local Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$sharedConn = "Server=192.168.208.26,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"
$localConn = "Server=.\SQLEXPRESS;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=true"

$tables = @("Roles", "RbacRoles", "RbacPermissions", "RbacRolePermissions", "Projects", "Locations", "Users", "UserProjects", "RbacUserPermissions", "RbacUserScope", "SystemSettings", "Assets")

# Step 1: Disable constraints
Write-Host "Step 1: Disabling constraints..." -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'" | Out-Null
Write-Host "  Done" -ForegroundColor Green
Write-Host ""

# Step 2: Clear local data
Write-Host "Step 2: Clearing local data..." -ForegroundColor Yellow
foreach ($table in $tables) {
    sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "DELETE FROM [$table]" 2>&1 | Out-Null
}
Write-Host "  Done" -ForegroundColor Green
Write-Host ""

# Step 3: Copy data
Write-Host "Step 3: Copying data..." -ForegroundColor Yellow

$sharedConnection = New-Object System.Data.SqlClient.SqlConnection($sharedConn)
$localConnection = New-Object System.Data.SqlClient.SqlConnection($localConn)

try {
    $sharedConnection.Open()
    $localConnection.Open()
    
    $i = 0
    foreach ($table in $tables) {
        $i++
        Write-Host "  [$i/$($tables.Count)] $table..." -ForegroundColor Gray
        
        # Get data from shared database
        $selectCmd = New-Object System.Data.SqlClient.SqlCommand("SELECT * FROM [$table]", $sharedConnection)
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($selectCmd)
        $dataTable = New-Object System.Data.DataTable
        $rowCount = $adapter.Fill($dataTable)
        
        if ($rowCount -eq 0) {
            Write-Host "    No data to copy" -ForegroundColor Yellow
            continue
        }
        
        # Get column names
        $columns = @()
        $parameters = @()
        foreach ($col in $dataTable.Columns) {
            $columns += "[$($col.ColumnName)]"
            $parameters += "@$($col.ColumnName)"
        }
        
        $columnList = $columns -join ","
        $paramList = $parameters -join ","
        
        # Check if table has identity column
        $hasIdentity = $false
        $identityCheckCmd = New-Object System.Data.SqlClient.SqlCommand("SELECT COLUMNPROPERTY(OBJECT_ID('[$table]'), COLUMN_NAME, 'IsIdentity') as IsIdentity FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$table' AND COLUMNPROPERTY(OBJECT_ID('[$table]'), COLUMN_NAME, 'IsIdentity') = 1", $localConnection)
        $identityReader = $identityCheckCmd.ExecuteReader()
        if ($identityReader.Read()) {
            $hasIdentity = $true
        }
        $identityReader.Close()
        
        # Insert data into local database
        $insertQuery = "INSERT INTO [$table] ($columnList) VALUES ($paramList)"
        if ($hasIdentity) {
            $insertQuery = "SET IDENTITY_INSERT [$table] ON; " + $insertQuery + "; SET IDENTITY_INSERT [$table] OFF;"
        }
        $insertCmd = New-Object System.Data.SqlClient.SqlCommand($insertQuery, $localConnection)
        
        $copiedCount = 0
        foreach ($row in $dataTable.Rows) {
            $insertCmd.Parameters.Clear()
            
            foreach ($col in $dataTable.Columns) {
                $value = $row[$col.ColumnName]
                if ($value -is [DBNull]) {
                    $insertCmd.Parameters.AddWithValue("@$($col.ColumnName)", [DBNull]::Value) | Out-Null
                }
                else {
                    $insertCmd.Parameters.AddWithValue("@$($col.ColumnName)", $value) | Out-Null
                }
            }
            
            try {
                $insertCmd.ExecuteNonQuery() | Out-Null
                $copiedCount++
            }
            catch {
                if ($copiedCount -eq 0) {
                    Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
        
        Write-Host "    Copied $copiedCount rows" -ForegroundColor Green
    }
}
finally {
    if ($sharedConnection.State -eq 'Open') { $sharedConnection.Close() }
    if ($localConnection.State -eq 'Open') { $localConnection.Close() }
}

Write-Host ""

# Step 4: Re-enable constraints
Write-Host "Step 4: Re-enabling constraints..." -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'" 2>&1 | Out-Null
Write-Host "  Done" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Show summary
Write-Host "Summary:" -ForegroundColor Cyan
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT 'Users' as [Table], COUNT(*) as [Rows] FROM Users UNION ALL SELECT 'Roles', COUNT(*) FROM Roles UNION ALL SELECT 'Projects', COUNT(*) FROM Projects UNION ALL SELECT 'Locations', COUNT(*) FROM Locations UNION ALL SELECT 'Assets', COUNT(*) FROM Assets UNION ALL SELECT 'RbacRoles', COUNT(*) FROM RbacRoles UNION ALL SELECT 'RbacPermissions', COUNT(*) FROM RbacPermissions"

Write-Host ""
Write-Host "Local database is ready for demo!" -ForegroundColor Green
