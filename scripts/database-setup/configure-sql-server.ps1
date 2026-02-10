# 🗄️ SQL Server Configuration Script
# Run this as Administrator

Write-Host "=== Configuring SQL Server Express for Network Access ===" -ForegroundColor Cyan

# Step 1: Enable SQL Server Browser service
Write-Host "`n1. Enabling SQL Server Browser service..." -ForegroundColor Yellow
try {
    Set-Service -Name "SQLBrowser" -StartupType Automatic
    Start-Service -Name "SQLBrowser"
    Write-Host "✓ SQL Server Browser service enabled" -ForegroundColor Green
} catch {
    Write-Host "⚠ SQL Server Browser service configuration failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 2: Configure Windows Firewall
Write-Host "`n2. Configuring Windows Firewall..." -ForegroundColor Yellow
try {
    netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
    netsh advfirewall firewall add rule name="SQL Server Browser" dir=in action=allow protocol=UDP localport=1434
    netsh advfirewall firewall add rule name="SQL Server Express" dir=in action=allow program="C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Binn\sqlservr.exe"
    Write-Host "✓ Firewall rules added successfully" -ForegroundColor Green
} catch {
    Write-Host "⚠ Firewall configuration failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 3: Display connection information
Write-Host "`n=== Connection Information ===" -ForegroundColor Cyan
$ipAddress = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*"} | Select-Object -First 1).IPAddress
Write-Host "Server IP: $ipAddress" -ForegroundColor White
Write-Host "SQL Server Instance: $ipAddress\SQLEXPRESS" -ForegroundColor White
Write-Host "Database: ITAMS" -ForegroundColor White

Write-Host "`n=== Next Steps ===" -ForegroundColor Cyan
Write-Host "1. Open SQL Server Configuration Manager" -ForegroundColor White
Write-Host "2. Go to: SQL Server Network Configuration > Protocols for SQLEXPRESS" -ForegroundColor White
Write-Host "3. Enable TCP/IP protocol" -ForegroundColor White
Write-Host "4. Right-click TCP/IP > Properties > IP Addresses tab" -ForegroundColor White
Write-Host "5. Scroll to IPAll section, set TCP Port to 1433" -ForegroundColor White
Write-Host "6. Restart SQL Server (SQLEXPRESS) service" -ForegroundColor White

Write-Host "`n✨ Configuration complete!" -ForegroundColor Green