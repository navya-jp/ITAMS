# Test if middleware is updating LastActivityAt
$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"

Write-Host "`n=== Testing Activity Middleware ===" -ForegroundColor Cyan

# Check John's current activity
$query = @"
SELECT 
    Username,
    LastLoginAt,
    LastActivityAt,
    DATEDIFF(MINUTE, LastActivityAt, GETUTCDATE()) as MinutesSinceActivity,
    CASE 
        WHEN DATEDIFF(MINUTE, LastActivityAt, GETUTCDATE()) <= 5 THEN 'ONLINE (Green dot)'
        ELSE 'OFFLINE (Gray dot)'
    END as Status
FROM Users 
WHERE Username = 'john.doe'
"@

Invoke-Sqlcmd -ConnectionString $connectionString -Query $query | Format-Table -AutoSize

Write-Host "`nInstructions:" -ForegroundColor Yellow
Write-Host "1. Have John perform ANY action in the frontend (click a menu, load a page, etc.)"
Write-Host "2. Run this script again to see if LastActivityAt updated"
Write-Host "3. If LastActivityAt updates, the middleware is working correctly!"
