$body = @{
    username = "test_user3"
    email = "test.user3@company.com"
    firstName = "Test"
    lastName = "User3"
    roleId = 2
    password = "TestPass123!"
    mustChangePassword = $true
} | ConvertTo-Json

Write-Host "Request Body:" -ForegroundColor Cyan
Write-Host $body

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5066/api/users" -Method POST -Body $body -ContentType "application/json"
    Write-Host "Success!" -ForegroundColor Green
    Write-Host "Status Code:" $response.StatusCode
    Write-Host "Response:" $response.Content
} catch {
    Write-Host "Error:" -ForegroundColor Red
    Write-Host "Status Code:" $_.Exception.Response.StatusCode
    Write-Host "Status Description:" $_.Exception.Response.StatusDescription
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body:" -ForegroundColor Yellow
        Write-Host $responseBody
        $reader.Close()
    }
}