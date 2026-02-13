$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

try {
    $connection.Open()
    Write-Host "Connected to database" -ForegroundColor Green
    
    # Add LastActivityAt column
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
ALTER TABLE Users
ADD LastActivityAt DATETIME2 NULL
"@
    
    try {
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "Added LastActivityAt column" -ForegroundColor Green
    } catch {
        if ($_.Exception.Message -like "*already*") {
            Write-Host "LastActivityAt column already exists" -ForegroundColor Yellow
        } else {
            throw
        }
    }
    
    # Initialize with LastLoginAt values
    $cmd.CommandText = @"
UPDATE Users
SET LastActivityAt = LastLoginAt
WHERE LastLoginAt IS NOT NULL
"@
    $updated = $cmd.ExecuteNonQuery()
    Write-Host "Initialized $updated users with LastActivityAt values" -ForegroundColor Green
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}
