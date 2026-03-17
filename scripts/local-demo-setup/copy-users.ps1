# Copy Users from Shared to Local Database
# This script copies users with only the columns that exist in both databases

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copying Users to Local Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$sharedConn = "Server=192.168.208.26,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"
$localConn = "Server=.\SQLEXPRESS;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=true"

# Get common columns between shared and local databases
Write-Host "Step 1: Identifying common columns..." -ForegroundColor Yellow

$sharedConnection = New-Object System.Data.SqlClient.SqlConnection($sharedConn)
$localConnection = New-Object System.Data.SqlClient.SqlConnection($localConn)

try {
    $sharedConnection.Open()
    $localConnection.Open()
    
    # Get columns from shared database
    $sharedColumnsCmd = New-Object System.Data.SqlClient.SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' ORDER BY ORDINAL_POSITION", $sharedConnection)
    $sharedReader = $sharedColumnsCmd.ExecuteReader()
    $sharedColumns = @()
    while ($sharedReader.Read()) {
        $sharedColumns += $sharedReader["COLUMN_NAME"].ToString()
    }
    $sharedReader.Close()
    
    # Get columns from local database
    $localColumnsCmd = New-Object System.Data.SqlClient.SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' ORDER BY ORDINAL_POSITION", $localConnection)
    $localReader = $localColumnsCmd.ExecuteReader()
    $localColumns = @()
    while ($localReader.Read()) {
        $localColumns += $localReader["COLUMN_NAME"].ToString()
    }
    $localReader.Close()
    
    # Find common columns
    $commonColumns = $sharedColumns | Where-Object { $localColumns -contains $_ }
    
    Write-Host "  Found $($commonColumns.Count) common columns" -ForegroundColor Green
    Write-Host "  Columns: $($commonColumns -join ', ')" -ForegroundColor Gray
    Write-Host ""
    
    # Build column list for SELECT and INSERT
    $columnList = ($commonColumns | ForEach-Object { "[$_]" }) -join ","
    $paramList = ($commonColumns | ForEach-Object { "@$_" }) -join ","
    
    # Step 2: Clear existing users
    Write-Host "Step 2: Clearing existing users..." -ForegroundColor Yellow
    $clearCmd = New-Object System.Data.SqlClient.SqlCommand("DELETE FROM Users", $localConnection)
    $clearCmd.ExecuteNonQuery() | Out-Null
    Write-Host "  Done" -ForegroundColor Green
    Write-Host ""
    
    # Step 3: Copy users
    Write-Host "Step 3: Copying users from shared database..." -ForegroundColor Yellow
    
    # Get users from shared database
    $selectQuery = "SELECT $columnList FROM Users"
    $selectCmd = New-Object System.Data.SqlClient.SqlCommand($selectQuery, $sharedConnection)
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($selectCmd)
    $dataTable = New-Object System.Data.DataTable
    $rowCount = $adapter.Fill($dataTable)
    
    Write-Host "  Found $rowCount users to copy" -ForegroundColor Gray
    
    # Check if table has identity column
    $hasIdentity = $false
    $identityCheckCmd = New-Object System.Data.SqlClient.SqlCommand("SELECT COLUMNPROPERTY(OBJECT_ID('Users'), 'Id', 'IsIdentity') as IsIdentity", $localConnection)
    $identityReader = $identityCheckCmd.ExecuteReader()
    if ($identityReader.Read() -and $identityReader["IsIdentity"] -eq 1) {
        $hasIdentity = $true
    }
    $identityReader.Close()
    
    # Insert users into local database
    $insertQuery = "INSERT INTO Users ($columnList) VALUES ($paramList)"
    if ($hasIdentity) {
        $insertQuery = "SET IDENTITY_INSERT Users ON; " + $insertQuery + "; SET IDENTITY_INSERT Users OFF;"
    }
    
    $insertCmd = New-Object System.Data.SqlClient.SqlCommand($insertQuery, $localConnection)
    
    $copiedCount = 0
    $errorCount = 0
    
    foreach ($row in $dataTable.Rows) {
        $insertCmd.Parameters.Clear()
        
        foreach ($col in $commonColumns) {
            $value = $row[$col]
            if ($value -is [DBNull]) {
                $insertCmd.Parameters.AddWithValue("@$col", [DBNull]::Value) | Out-Null
            }
            else {
                $insertCmd.Parameters.AddWithValue("@$col", $value) | Out-Null
            }
        }
        
        try {
            $insertCmd.ExecuteNonQuery() | Out-Null
            $copiedCount++
            
            # Show progress for key users
            $username = $row["Username"]
            if ($username -match "superadmin|admin|manager") {
                Write-Host "    Copied: $username" -ForegroundColor Green
            }
        }
        catch {
            $errorCount++
            Write-Host "    Error copying user: $($row["Username"]) - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "  Successfully copied $copiedCount users" -ForegroundColor Green
    if ($errorCount -gt 0) {
        Write-Host "  Failed to copy $errorCount users" -ForegroundColor Yellow
    }
}
finally {
    if ($sharedConnection.State -eq 'Open') { $sharedConnection.Close() }
    if ($localConnection.State -eq 'Open') { $localConnection.Close() }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "User Copy Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Show summary
Write-Host "Summary:" -ForegroundColor Cyan
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT Id, Username, Email, RoleId FROM Users ORDER BY Id"

Write-Host ""
Write-Host "You can now login with these users!" -ForegroundColor Green
Write-Host "Note: Passwords are copied as-is (hashed)" -ForegroundColor Yellow
