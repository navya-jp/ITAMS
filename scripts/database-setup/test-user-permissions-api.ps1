# Test the user permissions API for user ID 6 (hjk hcs - Auditor)
$response = Invoke-RestMethod -Uri "http://localhost:5066/api/rbac/users/6/permissions" -Method Get -Headers @{
    "Authorization" = "Bearer YOUR_TOKEN_HERE"
}

Write-Host "User: $($response.data.username)"
Write-Host "Role: $($response.data.roleName)"
Write-Host "Permission Count: $($response.data.rolePermissions.Count)"
Write-Host "Permissions:"
$response.data.rolePermissions | ForEach-Object { Write-Host "  - $_" }
