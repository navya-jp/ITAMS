# Network Troubleshooting Script for Teammates
Write-Host "=== ITAMS Network Troubleshooting ===" -ForegroundColor Cyan

# Step 1: Check teammate's IP
Write-Host "`n1. Checking your network configuration..." -ForegroundColor Yellow
$ipConfig = ipconfig | Select-String "IPv4 Address"
Write-Host "Your IP addresses:" -ForegroundColor White
$ipConfig

# Step 2: Check if on same network
Write-Host "`n2. Checking if you're on the same network as host..." -ForegroundColor Yellow
$myIP = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -like "192.168.*"}).IPAddress
if ($myIP -like "192.168.208.*") {
    Write-Host "✓ You're on the same network (192.168.208.x)" -ForegroundColor Green
} else {
    Write-Host "✗ You're NOT on the same network!" -ForegroundColor Red
    Write-Host "Host is on: 192.168.208.x" -ForegroundColor Yellow
    Write-Host "You are on: $myIP" -ForegroundColor Yellow
    Write-Host "Solution: Connect to the same WiFi/network as the host" -ForegroundColor Cyan
    exit 1
}

# Step 3: Test ping to host
Write-Host "`n3. Testing ping to host (192.168.208.10)..." -ForegroundColor Yellow
$ping = Test-Connection -ComputerName "192.168.208.10" -Count 2 -Quiet
if ($ping) {
    Write-Host "✓ Can ping host successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Cannot ping host" -ForegroundColor Red
    Write-Host "Possible issues:" -ForegroundColor Yellow
    Write-Host "- Host machine firewall blocking ping" -ForegroundColor Gray
    Write-Host "- Different network/WiFi" -ForegroundColor Gray
    Write-Host "- Host machine is off" -ForegroundColor Gray
}

# Step 4: Test SQL Server port
Write-Host "`n4. Testing SQL Server port (1433)..." -ForegroundColor Yellow
try {
    $portTest = Test-NetConnection -ComputerName "192.168.208.10" -Port 1433 -WarningAction SilentlyContinue
    if ($portTest.TcpTestSucceeded) {
        Write-Host "✓ SQL Server port 1433 is accessible!" -ForegroundColor Green
        Write-Host "✓ Ready to connect to shared database!" -ForegroundColor Green
    } else {
        Write-Host "✗ SQL Server port 1433 is not accessible" -ForegroundColor Red
        Write-Host "Host needs to configure SQL Server for network access" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Cannot test SQL Server port" -ForegroundColor Red
}

Write-Host "`n=== Troubleshooting Complete ===" -ForegroundColor Cyan