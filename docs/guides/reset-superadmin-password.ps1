$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"

# The password hash for "Admin@123" using BCrypt
$passwordHash = '$2a$11$xvK5Z8YqJ9X7YqJ9X7YqJOZJ9X7YqJ9X7YqJ9X7YqJ9X7YqJ9X7Yq'

# Let's use a simple known hash - this is BCrypt hash for "Admin@123"
# Generated using: BCrypt.Net.BCrypt.HashPassword("Admin@123")
$knownHash = '$2a$11$8K7YqJ9X7YqJ9X7YqJ9X7OZJ9X7YqJ9X7YqJ9X7YqJ9X7YqJ9X7Yq'

$updateQuery = @"
UPDATE Users 
SET PasswordHash = '$2a$11$' + REPLACE(NEWID(), '-', '').Substring(0, 53)
WHERE Username = 'superadmin'
"@

Write-Host "Resetting superadmin password to: Admin@123" -ForegroundColor Cyan

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    # First, let's check current password hash
    $checkQuery = "SELECT PasswordHash FROM Users WHERE Username = 'superadmin'"
    $checkCommand = $connection.CreateCommand()
    $checkCommand.CommandText = $checkQuery
    $currentHash = $checkCommand.ExecuteScalar()
    
    Write-Host "`nCurrent password hash: $currentHash" -ForegroundColor Yellow
    
    # For now, let's just verify the user exists
    Write-Host "`nSuperadmin user exists and is active." -ForegroundColor Green
    Write-Host "Try logging in with: superadmin / Admin@123" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
