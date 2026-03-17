# Copy Roles and Locations from Shared to Local Database

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copying Roles and Locations" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$sharedConn = "Server=192.168.208.26,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"
$localConn = "Server=.\SQLEXPRESS;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=true"

$sharedConnection = New-Object System.Data.SqlClient.SqlConnection($sharedConn)
$localConnection = New-Object System.Data.SqlClient.SqlConnection($localConn)

function Copy-Table {
    param(
        [string]$TableName,
        [System.Data.SqlClient.SqlConnection]$SharedConn,
        [System.Data.SqlClient.SqlConnection]$LocalConn
    )
    
    Write-Host "Copying $TableName..." -ForegroundColor Yellow
    
    # Get common columns
    $sharedColumnsCmd = New-Object System.Data.SqlClient.SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$TableName' ORDER BY ORDINAL_POSITION", $SharedConn)
    $sharedReader = $sharedColumnsCmd.ExecuteReader()
    $sharedColumns = @()
    while ($sharedReader.Read()) {
        $sharedColumns += $sharedReader["COLUMN_NAME"].ToString()
    }
    $sharedReader.Close()
    
    $localColumnsCmd = New-Object System.Data.SqlClient.SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$TableName' ORDER BY ORDINAL_POSITION", $LocalConn)
    $localReader = $localColumnsCmd.ExecuteReader()
    $localColumns = @()
    while ($localReader.Read()) {
        $localColumns += $localReader["COLUMN_NAME"].ToString()
    }
    $localReader.Close()
    
    $commonColumns = $sharedColumns | Where-Object { $localColumns -contains $_ }
    
    Write-Host "  Found $($commonColumns.Count) common columns" -ForegroundColor Gray
    
    # Build column list
    $columnList = ($commonColumns | ForEach-Object { "[$_]" }) -join ","
    $paramList = ($commonColumns | ForEach-Object { "@$_" }) -join ","
    
    # Clear existing data
    $clearCmd = New-Object System.Data.SqlClient.SqlCommand("DELETE FROM $TableName", $LocalConn)
    $clearCmd.ExecuteNonQuery() | Out-Null
    
    # Get data from shared database
    $selectQuery = "SELECT $columnList FROM $TableName"
    $selectCmd = New-Object System.Data.SqlClient.SqlCommand($selectQuery, $SharedConn)
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($selectCmd)
    $dataTable = New-Object System.Data.DataTable
    $rowCount = $adapter.Fill($dataTable)
    
    if ($rowCount -eq 0) {
        Write-Host "  No data to copy" -ForegroundColor Yellow
        return 0
    }
    
    # Check if table has identity column
    $hasIdentity = $false
    $identityCheckCmd = New-Object System.Data.SqlClient.SqlCommand("SELECT COLUMNPROPERTY(OBJECT_ID('$TableName'), 'Id', 'IsIdentity') as IsIdentity", $LocalConn)
    $identityReader = $identityCheckCmd.ExecuteReader()
    if ($identityReader.Read() -and $identityReader["IsIdentity"] -eq 1) {
        $hasIdentity = $true
    }
    $identityReader.Close()
    
    # Insert data
    $insertQuery = "INSERT INTO $TableName ($columnList) VALUES ($paramList)"
    if ($hasIdentity) {
        $insertQuery = "SET IDENTITY_INSERT $TableName ON; " + $insertQuery + "; SET IDENTITY_INSERT $TableName OFF;"
    }
    
    $insertCmd = New-Object System.Data.SqlClient.SqlCommand($insertQuery, $LocalConn)
    
    $copiedCount = 0
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
        }
        catch {
            Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host "  Copied $copiedCount rows" -ForegroundColor Green
    return $copiedCount
}

try {
    $sharedConnection.Open()
    $localConnection.Open()
    
    # Copy Roles
    Write-Host "Step 1: Copying Roles..." -ForegroundColor Cyan
    $rolesCount = Copy-Table -TableName "Roles" -SharedConn $sharedConnection -LocalConn $localConnection
    Write-Host ""
    
    # Copy Locations
    Write-Host "Step 2: Copying Locations..." -ForegroundColor Cyan
    $locationsCount = Copy-Table -TableName "Locations" -SharedConn $sharedConnection -LocalConn $localConnection
    Write-Host ""
    
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Copy Complete!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Summary:" -ForegroundColor Yellow
    Write-Host "  Roles: $rolesCount" -ForegroundColor White
    Write-Host "  Locations: $locationsCount" -ForegroundColor White
}
finally {
    if ($sharedConnection.State -eq 'Open') { $sharedConnection.Close() }
    if ($localConnection.State -eq 'Open') { $localConnection.Close() }
}

Write-Host ""
Write-Host "Verifying data..." -ForegroundColor Cyan
Write-Host ""
Write-Host "Roles:" -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT Id, RoleName FROM Roles"

Write-Host ""
Write-Host "Locations:" -ForegroundColor Yellow
sqlcmd -S ".\SQLEXPRESS" -d "ITAMS_Local" -Q "SELECT TOP 10 Id, LocationName, ProjectId FROM Locations"
