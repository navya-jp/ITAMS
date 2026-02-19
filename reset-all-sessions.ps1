# Reset all sessions and prepare for fresh login
$connectionString = "Server=localhost\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True;"

Write-Host "=== Resetting All Sessions ===" -ForegroundColor Cyan
Write-Host ""

$query = @"
-- Clear all active sessions
UPDATE Users 
SET ActiveSessionId = NULL, 
    SessionStartedAt = NULL, 
    LastActivityAt = NULL
WHERE ActiveSessionId IS NOT NULL;

-- Update all active login audits to FORCED_LOGOUT
UPDATE LoginAudit
SET LogoutTime = GETUTCDATE(),
    Status = 'FORCED_LOGOUT'
WHERE Status = 'ACTIVE';

-- Show results
SELECT 
    'Sessions Cleared' AS Action,
    COUNT(*) AS Count
FROM Users 
WHERE ActiveSessionId IS NULL;

SELECT 
    'Active Logins Updated' AS Action,
    COUNT(*) AS Count
FROM LoginAudit 
WHERE Status = 'FORCED_LOGOUT' 
  AND LogoutTime >= DATEADD(MINUTE, -1, GETUTCDATE());
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    Write-Host "✓ All sessions cleared!" -ForegroundColor Green
    Write-Host ""
    
    foreach ($table in $dataset.Tables) {
        $table | Format-Table -AutoSize
    }
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "=== Next Steps ===" -ForegroundColor Yellow
    Write-Host "1. In your browser, press F12 to open Developer Tools" -ForegroundColor Gray
    Write-Host "2. Go to Application tab → Local Storage" -ForegroundColor Gray
    Write-Host "3. Click 'Clear All' or delete 'auth_token' and 'current_user'" -ForegroundColor Gray
    Write-Host "4. Close the browser tab completely" -ForegroundColor Gray
    Write-Host "5. Open a new tab and go to http://localhost:4200" -ForegroundColor Gray
    Write-Host "6. Login with: superadmin / Admin@123" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
}
