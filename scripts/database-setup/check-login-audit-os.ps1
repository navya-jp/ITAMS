# Check Login Audit OS Detection
# This script shows the operating systems detected in recent login audits

$serverInstance = "localhost\SQLEXPRESS"
$database = "ITAMS_Shared"

Write-Host "=== Recent Login Audit Records ===" -ForegroundColor Cyan
Write-Host ""

$query = @"
SELECT TOP 10
    Id,
    Username,
    LoginTime,
    OperatingSystem,
    BrowserType,
    IpAddress,
    Status
FROM LoginAudit
ORDER BY LoginTime DESC
"@

try {
    $results = Invoke-Sqlcmd -ServerInstance $serverInstance -Database $database -Query $query -ErrorAction Stop
    
    if ($results) {
        foreach ($row in $results) {
            Write-Host "ID: $($row.Id)" -ForegroundColor Yellow
            Write-Host "  Username: $($row.Username)"
            Write-Host "  Login Time: $($row.LoginTime)"
            Write-Host "  Operating System: $($row.OperatingSystem)" -ForegroundColor $(if ($row.OperatingSystem -eq 'Linux') { 'Red' } else { 'Green' })
            Write-Host "  Browser: $($row.BrowserType)"
            Write-Host "  IP Address: $($row.IpAddress)"
            Write-Host "  Status: $($row.Status)"
            Write-Host ""
        }
        
        Write-Host "=== Summary ===" -ForegroundColor Cyan
        $linuxCount = ($results | Where-Object { $_.OperatingSystem -eq 'Linux' }).Count
        $windowsCount = ($results | Where-Object { $_.OperatingSystem -like 'Windows*' }).Count
        
        Write-Host "Linux records: $linuxCount" -ForegroundColor $(if ($linuxCount -gt 0) { 'Red' } else { 'Green' })
        Write-Host "Windows records: $windowsCount" -ForegroundColor $(if ($windowsCount -gt 0) { 'Green' } else { 'Yellow' })
        Write-Host ""
        Write-Host "NOTE: Old records will still show 'Linux' because that's what was saved before the fix." -ForegroundColor Yellow
        Write-Host "To see the fix working, LOGOUT and LOGIN AGAIN to create a new audit record." -ForegroundColor Cyan
    } else {
        Write-Host "No login audit records found." -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error querying database: $_" -ForegroundColor Red
}
