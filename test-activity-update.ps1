# Quick test to see if activity is updating
$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

Write-Host "`n=== Activity Update Test ===" -ForegroundColor Cyan
Write-Host "Checking superadmin activity..." -ForegroundColor Yellow

$connection.Open()
$cmd = $connection.CreateCommand()
$cmd.CommandText = @"
SELECT 
    Username,
    LastActivityAt,
    DATEDIFF(SECOND, LastActivityAt, GETUTCDATE()) as SecondsSinceActivity,
    CASE 
        WHEN DATEDIFF(SECOND, LastActivityAt, GETUTCDATE()) <= 300 THEN 'ONLINE (Green)'
        ELSE 'OFFLINE (Gray)'
    END as Status
FROM Users 
WHERE Username = 'superadmin'
"@

$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "`nUsername: $($reader['Username'])"
    Write-Host "LastActivityAt: $($reader['LastActivityAt'])"
    Write-Host "Seconds since activity: $($reader['SecondsSinceActivity'])"
    Write-Host "Status: $($reader['Status'])" -ForegroundColor $(if ($reader['SecondsSinceActivity'] -le 300) { 'Green' } else { 'Gray' })
}
$reader.Close()
$connection.Close()

Write-Host "`nNow go to your frontend and click ANYTHING (refresh users page, click a menu, etc.)"
Write-Host "Then run this script again to see if LastActivityAt updated!" -ForegroundColor Yellow
