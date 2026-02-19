# Test Password Reset Request Workflow
Write-Host "Testing Password Reset Request Workflow..." -ForegroundColor Cyan
Write-Host ""

# Test 1: Send password reset request for navya.pradeepkumar
Write-Host "1. Sending password reset request for navya.pradeepkumar..." -ForegroundColor Yellow
$body = @{
    username = "navya.pradeepkumar"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5066/api/auth/forgot-password" `
        -Method POST `
        -ContentType "application/json" `
        -Body $body
    
    Write-Host "   Response: $($response.message)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Test 2: Check database to verify the flag was set
Write-Host "2. Checking database for password reset flag..." -ForegroundColor Yellow
$query = "SELECT Id, Username, PasswordResetRequested, PasswordResetRequestedAt FROM Users WHERE Username = 'navya.pradeepkumar'"
sqlcmd -S localhost\SQLEXPRESS -d ITAMS_Shared -Q $query -W
Write-Host ""

# Test 3: Get all users to see if the flag appears in the API response
Write-Host "3. Checking API response for users..." -ForegroundColor Yellow
try {
    $users = Invoke-RestMethod -Uri "http://localhost:5066/api/users" -Method GET
    $navya = $users.data | Where-Object { $_.username -eq "navya.pradeepkumar" }
    
    if ($navya) {
        Write-Host "   User found in API response:" -ForegroundColor Green
        Write-Host "   - Username: $($navya.username)"
        Write-Host "   - PasswordResetRequested: $($navya.passwordResetRequested)"
        Write-Host "   - PasswordResetRequestedAt: $($navya.passwordResetRequestedAt)"
    } else {
        Write-Host "   User not found in API response" -ForegroundColor Red
    }
} catch {
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test complete! Now check the frontend:" -ForegroundColor Cyan
Write-Host "1. Login as superadmin/Admin@123" -ForegroundColor White
Write-Host "2. Go to User Management" -ForegroundColor White
Write-Host "3. Look for navya.pradeepkumar - should have orange 'Password Reset Requested' badge" -ForegroundColor White
Write-Host "4. Click the key icon to reset the password" -ForegroundColor White
