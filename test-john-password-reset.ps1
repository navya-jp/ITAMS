# Test john.doe password reset request
Write-Host "Testing john.doe password reset request..." -ForegroundColor Cyan
Write-Host ""

# Step 1: Send password reset request
Write-Host "1. Sending password reset request for john.doe..." -ForegroundColor Yellow
$body = @{
    username = "john.doe"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5066/api/auth/forgot-password" `
        -Method POST `
        -ContentType "application/json" `
        -Body $body
    
    Write-Host "   SUCCESS: $($response.message)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
    Write-Host ""
}

# Step 2: Check database
Write-Host "2. Checking database..." -ForegroundColor Yellow
$query = "SELECT Id, Username, PasswordResetRequested, PasswordResetRequestedAt FROM Users WHERE Username = 'john.doe'"
sqlcmd -S localhost\SQLEXPRESS -d ITAMS_Shared -Q $query -W
Write-Host ""

Write-Host "Done! Check the User Management page for john.doe" -ForegroundColor Cyan
