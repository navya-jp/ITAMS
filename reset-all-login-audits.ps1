# Reset all login audits and sessions for fresh testing

$serverInstance = "localhost\SQLEXPRESS"
$database = "ITAMS_Shared"

Write-Host "Resetting all sessions and login audits..." -ForegroundColor Yellow

# SQL to clear all active sessions and mark old audits as logged out
$sql = @"
-- Clear all active sessions from Users table
UPDATE Users 
SET ActiveSessionId = NULL, 
    SessionStartedAt = NULL,
    LastActivityAt = NULL
WHERE ActiveSessionId IS NOT NULL;

-- Mark all ACTIVE login audits as LOGGED_OUT
UPDATE LoginAudit 
SET Status = 'LOGGED_OUT',
    LogoutTime = GETDATE()
WHERE Status = 'ACTIVE';

-- Show results
SELECT 'Users cleared' as Action, COUNT(*) as Count FROM Users WHERE ActiveSessionId IS NULL;
SELECT 'Audits updated' as Action, COUNT(*) as Count FROM LoginAudit WHERE Status = 'LOGGED_OUT';
"@

try {
    Invoke-Sqlcmd -ServerInstance $serverInstance -Database $database -Query $sql -ErrorAction Stop
    Write-Host "✓ All sessions cleared successfully!" -ForegroundColor Green
    Write-Host "✓ All active login audits marked as LOGGED_OUT" -ForegroundColor Green
    Write-Host ""
    Write-Host "Now you can:" -ForegroundColor Cyan
    Write-Host "1. Logout from the frontend" -ForegroundColor White
    Write-Host "2. Login again with fresh credentials" -ForegroundColor White
    Write-Host "3. Test the new session lifecycle" -ForegroundColor White
}
catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
}
