# Restart SQL Server - Run as Administrator
Write-Host "Restarting SQL Server..." -ForegroundColor Yellow

# Method 1: Try services
try {
    Stop-Service -Name "MSSQL`$SQLEXPRESS" -Force -ErrorAction Stop
    Start-Service -Name "MSSQL`$SQLEXPRESS" -ErrorAction Stop
    Write-Host "SQL Server restarted successfully!" -ForegroundColor Green
} catch {
    Write-Host "Could not restart via PowerShell. Please restart manually:" -ForegroundColor Yellow
    Write-Host "1. Press Windows + R" -ForegroundColor Gray
    Write-Host "2. Type: services.msc" -ForegroundColor Gray
    Write-Host "3. Find 'SQL Server (SQLEXPRESS)'" -ForegroundColor Gray
    Write-Host "4. Right-click -> Restart" -ForegroundColor Gray
}