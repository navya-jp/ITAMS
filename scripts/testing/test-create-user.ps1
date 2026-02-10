$body = @{
    username = "test.user2"
    email = "test.user2@company.com"
    firstName = "Test"
    lastName = "User2"
    roleId = 2
    password = "Test@123456"
    mustChangePassword = $true
} | ConvertTo-Json

$headers = @{
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5066/api/users" -Method POST -Body $body -Headers $headers
    Write-Host "Success!" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error:" -ForegroundColor Red
    Write-Host "Status Code:" $_.Exception.Response.StatusCode
    Write-Host "Exception Message:" $_.Exception.Message
    if ($_.ErrorDetails) {
        Write-Host "Error Details:" $_.ErrorDetails.Message
    }
    
    # Try to get the response content
    try {
        $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
        Write-Host "Validation Errors:" -ForegroundColor Yellow
        $errorResponse | ConvertTo-Json -Depth 3
    } catch {
        Write-Host "Could not parse error response"
    }
}