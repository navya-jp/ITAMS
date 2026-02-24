# Check login audit records with negative duration
$server = "192.168.208.26,1433"
$database = "ITAMS_Shared"
$username = "itams_user"
$password = "ITAMS@2024!"

$query = @"
SELECT TOP 10
    Id,
    Username,
    LoginTime,
    LogoutTime,
    DATEDIFF(MINUTE, LoginTime, LogoutTime) AS DurationMinutes,
    Status
FROM LoginAudit
WHERE LogoutTime IS NOT NULL
ORDER BY Id DESC;
"@

sqlcmd -S $server -d $database -U $username -P $password -Q $query
