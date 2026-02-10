# Simple Team Connection Script for ITAMS
# Run this after pulling latest changes from GitHub

Write-Host "=== ITAMS Team Database Connection ===" -ForegroundColor Cyan
Write-Host "Connecting to shared database at 192.168.208.10..." -ForegroundColor Yellow
Write-Host "WARNING: This will REPLACE your local database connection!" -ForegroundColor Red

# Update appsettings.json to use shared database ONLY
$appsettingsPath = "appsettings.json"

if (Test-Path $appsettingsPath) {
    try {
        # Create new appsettings with ONLY shared database
        $newSettings = @{
            "Logging" = @{
                "LogLevel" = @{
                    "Default" = "Information"
                    "Microsoft.AspNetCore" = "Warning"
                }
            }
            "AllowedHosts" = "*"
            "ConnectionStrings" = @{
                "DefaultConnection" = "DISABLED - USING SHARED DATABASE ONLY"
                "SharedSqlServer" = "Server=192.168.208.10,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true"
            }
        }
        
        # Save updated settings
        $newSettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        
        Write-Host "✓ Database connection updated successfully!" -ForegroundColor Green
        Write-Host "✓ Now connected to shared database at 192.168.208.10" -ForegroundColor Green
        Write-Host "✓ You should see all users created by team members" -ForegroundColor Green
        
        # Test connectivity
        Write-Host "`nTesting connection to 192.168.208.10..." -ForegroundColor Yellow
        $ping = Test-Connection -ComputerName "192.168.208.10" -Count 2 -Quiet
        if ($ping) {
            Write-Host "✓ Network connection successful" -ForegroundColor Green
        } else {
            Write-Host "✗ Cannot reach 192.168.208.10 - check network/firewall" -ForegroundColor Red
            return
        }
        
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