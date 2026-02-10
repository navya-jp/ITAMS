# Enable Network Sharing for ITAMS Database
# RUN THIS AS ADMINISTRATOR

Write-Host "=== Enabling Network Database Sharing ===" -ForegroundColor Cyan

# Step 1: Configure Windows Firewall
Write-Host "1. Configuring Windows Firewall..." -ForegroundColor Yellow
try {
    netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
    netsh advfirewall firewall add rule name="SQL Server Browser" dir=in action=allow protocol=UDP localport=1434
    Write-Host "✓ Firewall rules added successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Firewall configuration failed. Run as Administrator!" -ForegroundColor Red
}

# Step 2: Enable SQL Server Browser
Write-Host "2. Enabling SQL Server Browser..." -ForegroundColor Yellow
try {
    Set-Service -Name "SQLBrowser" -StartupType Automatic
    Start-Service -Name "SQLBrowser"
    Write-Host "✓ SQL Server Browser enabled" -ForegroundColor Green
} catch {
    Write-Host "⚠ SQL Server Browser configuration failed" -ForegroundColor Yellow
}

Write-Host "`n=== Network Sharing Enabled! ===" -ForegroundColor Green
Write-Host "Your teammates can now connect to:" -ForegroundColor Yellow
Write-Host "Server: 192.168.208.10\SQLEXPRESS" -ForegroundColor White
Write-Host "Database: ITAMS_Shared" -ForegroundColor White
Write-Host "User: itams_user" -ForegroundColor White
Write-Host "Password: ITAMS@2024!" -ForegroundColor White

Write-Host "`nNext: Tell teammates to run .\team-connect-shared.ps1" -ForegroundColor Cyan