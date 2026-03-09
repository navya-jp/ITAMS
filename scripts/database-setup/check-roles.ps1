$connectionString = "Server=192.168.208.26,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"

$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString

try {
    $connection.Open()
    
    $query = "SELECT Id, RoleId, Name, Description FROM Roles ORDER BY Id"
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $reader = $command.ExecuteReader()
    
    Write-Host "Current Roles in Database:"
    Write-Host "=========================="
    while ($reader.Read()) {
        Write-Host "ID: $($reader['Id']) | RoleId: $($reader['RoleId']) | Name: $($reader['Name']) | Description: $($reader['Description'])"
    }
    
    $reader.Close()
}
finally {
    $connection.Close()
}
