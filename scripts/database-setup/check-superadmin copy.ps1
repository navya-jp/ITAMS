$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"

$query = "SELECT Id, Username, Email, RoleId, IsActive FROM Users WHERE Username = 'superadmin'"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host "`nSuperadmin User Info:" -ForegroundColor Cyan
    Write-Host "=====================" -ForegroundColor Cyan
    
    if ($dataset.Tables[0].Rows.Count -eq 0) {
        Write-Host "ERROR: Superadmin user not found in database!" -ForegroundColor Red
    } else {
        $dataset.Tables[0] | Format-Table -AutoSize
    }
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
