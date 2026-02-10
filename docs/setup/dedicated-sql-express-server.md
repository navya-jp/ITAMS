# Dedicated SQL Server Express Setup

## Option 1: Use One Team Member's Computer as Server

### Requirements:
- One computer that can stay ON during work hours
- Stable internet connection
- Windows 10/11 or Windows Server

### Setup Steps:

#### 1. Install SQL Server Express on Server Computer
```powershell
# Download SQL Server Express 2022
# https://www.microsoft.com/en-us/sql-server/sql-server-downloads
# Choose "Express" edition with "Advanced Services"
```

#### 2. Configure for Remote Access
```powershell
# Run as Administrator
.\setup-sql-express-server.ps1
```

#### 3. Set Up Windows Service
```powershell
# Ensure SQL Server starts automatically
Set-Service -Name "MSSQL$SQLEXPRESS" -StartupType Automatic
Set-Service -Name "SQLBrowser" -StartupType Automatic

# Start services
Start-Service -Name "MSSQL$SQLEXPRESS"
Start-Service -Name "SQLBrowser"
```

#### 4. Configure Static IP (Recommended)
- Set the server computer to use a static IP address
- This prevents connection issues when IP changes

#### 5. Team Connection Details
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.1.100\\SQLEXPRESS;Database=ITAMSSharedDB;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false;"
  }
}
```

### Benefits:
✅ Uses SQL Server Express
✅ Always available when server computer is ON
✅ Better than depending on your personal computer
✅ Can be any team member's computer

### Drawbacks:
❌ Server computer must stay ON
❌ Depends on one person's computer/internet
❌ No automatic backups