# Clear superadmin session to allow login
$connectionString = "Server=localhost\SQLEXPRESS;Database=ITAMS_Shared;Trusted_Connection=True;TrustServerCertificate=True;"

$query = @"
UPDATE Users 
SET ActiveSessionId = NULL, 
    SessionStartedAt = NULL, 
    LastActivityAt = NULL
WHERE Username = 'superadmin';

-- Also update any active login audits
UPDATE LoginAudit
SET LogoutTime = GETUTCDATE(),
    Status = 'FORCED_LOGOUT'
WHERE UserId = (SELECT Id FROM Users WHERE Username = 'superadmin')
  AND Status = 'ACTIVE';

SELECT 
    Username,
    ActiveSessionId,
    SessionStartedAt,
    LastActivityAt
FROM Users 
WHERE Username = 'superadmin';
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host "Session cleared for superadmin!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Current status:" -ForegroundColor Cyan
    $dataset.Tables[0] | Format-Table -AutoSize
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
