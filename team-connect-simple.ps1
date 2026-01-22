# Simple Team Connection Script for ITAMS
# Run this after pulling latest changes from GitHub

Write-Host "=== ITAMS Team Database Connection ===" -ForegroundColor Cyan
Write-Host "Connecting to shared database at 192.168.208.10..." -ForegroundColor Yellow

# Update appsettings.json to use shared database
$appsettingsPath = "appsettings.json"

if (Test-Path $appsettingsPath) {
    try {
        # Read current settings
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        
        # Add/Update SharedSqlServer connection string
        if (-not $appsettings.ConnectionStrings) {
            $appsettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
        }
        
        $appsettings.ConnectionStrings.SharedSqlServer = "Server=192.168.208.10,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
        
        # Save updated settings
        $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        
        Write-Host "✓ Database connection updated successfully!" -ForegroundColor Green
        Write-Host "✓ Now connected to shared database at 192.168.208.10" -ForegroundColor Green
        
        # Build and run
        Write-Host "`nBuilding project..." -ForegroundColor Yellow
        dotnet build
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Build successful!" -ForegroundColor Green
            Write-Host "`nStarting application..." -ForegroundColor Yellow
            Write-Host "Backend will be available at: http://localhost:5066" -ForegroundColor Cyan
            Write-Host "Frontend will be available at: http://localhost:4200" -ForegroundColor Cyan
            Write-Host "`nPress Ctrl+C to stop the application" -ForegroundColor Gray
            
            # Start the backend
            dotnet run
        } else {
            Write-Host "✗ Build failed! Please check the errors above." -ForegroundColor Red
        }
        
    } catch {
        Write-Host "✗ Error updating configuration: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "✗ appsettings.json not found! Make sure you're in the ITAMS project directory." -ForegroundColor Red
}