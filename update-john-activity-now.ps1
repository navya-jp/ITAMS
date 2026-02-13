$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

try {
    $connection.Open()
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
UPDATE Users
SET LastActivityAt = GETUTCDATE()
WHERE Username = 'john.doe'
"@
    
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Updated john.doe LastActivityAt to current time" -ForegroundColor Green
    
    # Check the result
    $cmd.CommandText = @"
SELECT 
    Username,
    LastActivityAt,
    DATEDIFF(SECOND, LastActivityAt, GETUTCDATE()) as SecondsSinceActivity
FROM Users
WHERE Username = 'john.doe'
"@
    
    $reader = $cmd.ExecuteReader()
    if ($reader.Read()) {
        Write-Host "Username: $($reader['Username'])"
        Write-Host "LastActivityAt: $($reader['LastActivityAt'])"
        Write-Host "Seconds since activity: $($reader['SecondsSinceActivity'])"
        Write-Host "John should now show GREEN DOT!" -ForegroundColor Green
    }
    $reader.Close()
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}
