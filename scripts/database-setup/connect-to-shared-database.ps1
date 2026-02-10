# Connect to Shared ITAMS Database
param(
    [Parameter(Mandatory=$true)]
    [string]$ServerIP
)

Write-Host "=== Connecting to Shared ITAMS Database ===" -ForegroundColor Cyan
Write-Host "Server IP: $ServerIP" -ForegroundColor Yellow

$appsettingsPath = "appsettings.json"

if (Test-Path $appsettingsPath) {
    Write-Host "Updating appsettings.json..." -ForegroundColor Yellow
    
    try {
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        $connectionString = "Server=$ServerIP,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
        
        if (-not $appsettings.ConnectionStrings) {
            $appsettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
        }
        $appsettings.ConnectionStrings.SharedSqlServer = $connectionString
        
        $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        
        Write-Host "Connection string updated successfully" -ForegroundColor Green
        
        Write-Host "Building project..." -ForegroundColor Yellow
        dotnet build
        
        Write-Host "Starting application..." -ForegroundColor Yellow
        Write-Host "Backend: http://localhost:5066" -ForegroundColor Green
        Write-Host "Frontend: http://localhost:4200" -ForegroundColor Green
        
        dotnet run
        
    } catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "appsettings.json not found!" -ForegroundColor Red
}