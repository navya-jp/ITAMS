# Complete API Testing Script
$baseUrl = "http://localhost:5066/api"

Write-Host "=== ITAMS API Testing ===" -ForegroundColor Cyan

# Test 1: Get all users
Write-Host "`n1. Testing GET /users" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/users" -Method GET
    Write-Host "✓ Users API working - Status: $($response.StatusCode)" -ForegroundColor Green
    $users = ($response.Content | ConvertFrom-Json).data
    Write-Host "Found $($users.Count) users" -ForegroundColor Green
} catch {
    Write-Host "✗ Users API failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get all roles
Write-Host "`n2. Testing GET /superadmin/roles" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/superadmin/roles" -Method GET
    Write-Host "✓ Roles API working - Status: $($response.StatusCode)" -ForegroundColor Green
    $roles = $response.Content | ConvertFrom-Json
    Write-Host "Found $($roles.Count) roles" -ForegroundColor Green
    $roles | ForEach-Object { Write-Host "  - $($_.name): $($_.description)" -ForegroundColor Gray }
} catch {
    Write-Host "✗ Roles API failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get all projects
Write-Host "`n3. Testing GET /superadmin/projects" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/superadmin/projects" -Method GET
    Write-Host "✓ Projects API working - Status: $($response.StatusCode)" -ForegroundColor Green
    $projects = $response.Content | ConvertFrom-Json
    Write-Host "Found $($projects.Count) projects" -ForegroundColor Green
} catch {
    Write-Host "✗ Projects API failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Get all locations
Write-Host "`n4. Testing GET /superadmin/locations" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/superadmin/locations" -Method GET
    Write-Host "✓ Locations API working - Status: $($response.StatusCode)" -ForegroundColor Green
    $locations = $response.Content | ConvertFrom-Json
    Write-Host "Found $($locations.Count) locations" -ForegroundColor Green
} catch {
    Write-Host "✗ Locations API failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Create a test user
Write-Host "`n5. Testing POST /users (Create User)" -ForegroundColor Yellow
$testUser = @{
    username = "test.api.$(Get-Date -Format 'HHmmss')"
    email = "test.api.$(Get-Date -Format 'HHmmss')@company.com"
    firstName = "Test"
    lastName = "API"
    roleId = 4  # IT Staff role
    password = "TestPass123!"
    mustChangePassword = $true
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/users" -Method POST -Body $testUser -ContentType "application/json"
    Write-Host "✓ Create User API working - Status: $($response.StatusCode)" -ForegroundColor Green
    $createdUser = ($response.Content | ConvertFrom-Json).data
    Write-Host "Created user: $($createdUser.username) with role: $($createdUser.roleName)" -ForegroundColor Green
    
    # Test 6: Update the created user
    Write-Host "`n6. Testing PUT /users/{id} (Update User)" -ForegroundColor Yellow
    $updateUser = @{
        firstName = "Updated"
        lastName = "User"
        email = $createdUser.email
        roleId = $createdUser.roleId
        isActive = $true
    } | ConvertTo-Json
    
    $updateResponse = Invoke-WebRequest -Uri "$baseUrl/users/$($createdUser.id)" -Method PUT -Body $updateUser -ContentType "application/json"
    Write-Host "✓ Update User API working - Status: $($updateResponse.StatusCode)" -ForegroundColor Green
    
    # Test 7: Deactivate the user
    Write-Host "`n7. Testing PATCH /users/{id}/deactivate" -ForegroundColor Yellow
    $deactivateResponse = Invoke-WebRequest -Uri "$baseUrl/users/$($createdUser.id)/deactivate" -Method PATCH -ContentType "application/json"
    Write-Host "✓ Deactivate User API working - Status: $($deactivateResponse.StatusCode)" -ForegroundColor Green
    
} catch {
    Write-Host "✗ User CRUD operations failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Error details: $responseBody" -ForegroundColor Red
        $reader.Close()
    }
}

Write-Host "`n=== API Testing Complete ===" -ForegroundColor Cyan