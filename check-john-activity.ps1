$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

try {
    $connection.Open()
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
SELECT 
    Username,
    LastLoginAt,
    LastActivityAt,
    DATEDIFF(MINUTE, LastActivityAt, GETUTCDATE()) as MinutesSinceActivity
FROM Users
WHERE Username LIKE '%john%'
"@
    
    $reader = $cmd.ExecuteReader()
    Write-Host "`n=== John's Activity Status ===" -ForegroundColor Cyan
    while ($reader.Read()) {
        Write-Host "Username: $($reader['Username'])"
        Write-Host "LastLoginAt: $($reader['LastLoginAt'])"
        Write-Host "LastActivityAt: $($reader['LastActivityAt'])"
        Write-Host "Minutes since activity: $($reader['MinutesSinceActivity'])"
        
        if ($reader['MinutesSinceActivity'] -lt 5) {
            Write-Host "STATUS: ONLINE (Green dot should show)" -ForegroundColor Green
        } else {
            Write-Host "STATUS: OFFLINE (Gray dot)" -ForegroundColor Gray
        }
        Write-Host ""
    }
    $reader.Close()
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}
