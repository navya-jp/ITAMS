# 🗄️ ITAMS Shared Database Setup Guide

## 🎯 Goal
Set up SQL Server so all team members can access the same ITAMS database from different devices.

## 👥 Roles

### 🖥️ **Database Host (One Person)**
The person who will host the SQL Server database on their machine.

### 💻 **Team Members (Everyone Else)**
People who will connect to the shared database.

---

## 🖥️ DATABASE HOST SETUP

### Step 1: Install SQL Server Express
1. Download **SQL Server Express**: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. Run installer, choose **Basic** installation
3. **IMPORTANT**: Note the server name (usually: `COMPUTERNAME\SQLEXPRESS`)
4. Download and install **SQL Server Management Studio (SSMS)**

### Step 2: Configure SQL Server for Network Access
1. Open **SQL Server Configuration Manager**
2. Navigate to: **SQL Server Network Configuration** → **Protocols for SQLEXPRESS**
3. **Enable TCP/IP** protocol
4. Right-click **TCP/IP** → **Properties** → **IP Addresses** tab
5. Scroll to **IPAll** section at bottom
6. Set **TCP Port** to **1433**
7. **Restart SQL Server service**

### Step 3: Configure Windows Firewall
Run as Administrator:
```cmd
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
```

### Step 4: Create Database and User
1. Open **SQL Server Management Studio (SSMS)**
2. Connect to: `COMPUTERNAME\SQLEXPRESS`
3. Run this SQL script:

```sql
-- Create database
CREATE DATABASE ITAMS;
GO

-- Use the database
USE ITAMS;
GO

-- Create login for team access
CREATE LOGIN itams_user WITH PASSWORD = 'ITAMS@2024!';
GO

-- Create user in database
CREATE USER itams_user FOR LOGIN itams_user;
GO

-- Grant full permissions
ALTER ROLE db_owner ADD MEMBER itams_user;
GO
```

### Step 5: Run Setup Script
In your ITAMS project folder:
```powershell
.\setup-shared-database.ps1
```

This will:
- Get your IP address
- Update appsettings.json
- Test database connection
- Show connection details for your team

### Step 6: Share Connection Details
Share these details with your team:
- **Server IP**: (shown by setup script)
- **Database**: ITAMS
- **Username**: itams_user
- **Password**: ITAMS@2024!
- **Port**: 1433

---

## 💻 TEAM MEMBER SETUP

### Step 1: Pull Latest Code
```bash
git pull origin main
```

### Step 2: Connect to Shared Database
Run this command with the host's IP address:
```powershell
.\connect-to-shared-database.ps1 -ServerIP "192.168.1.100"
```
(Replace `192.168.1.100` with the actual host IP)

### Step 3: Start Application
The script will automatically:
- Update your appsettings.json
- Build the project
- Start the backend server

### Step 4: Start Frontend (New Terminal)
```bash
cd itams-frontend
npm install
ng serve --port 4200 --open
```

---

## 🔧 TROUBLESHOOTING

### ❌ "Cannot connect to server"
1. **Check IP address**: Make sure you're using the correct host IP
2. **Ping test**: `ping 192.168.1.100` (replace with actual IP)
3. **Port test**: `telnet 192.168.1.100 1433`
4. **Firewall**: Host should run firewall command again
5. **SQL Server**: Ensure SQL Server service is running on host

### ❌ "Login failed for user"
1. **Check credentials**: Username: `itams_user`, Password: `ITAMS@2024!`
2. **Re-run SQL script**: Host should re-create the user
3. **Mixed mode**: Ensure SQL Server uses Mixed Mode Authentication

### ❌ "Database does not exist"
1. **Create database**: Host should run the SQL script to create ITAMS database
2. **Check connection**: Verify you're connecting to the right server

### ❌ Network connectivity issues
1. **Same network**: All devices must be on the same WiFi/network
2. **VPN**: Disable VPN if causing issues
3. **Router settings**: Some routers block inter-device communication

---

## 🚀 QUICK START COMMANDS

### For Database Host:
```powershell
# One-time setup
.\setup-shared-database.ps1

# Start server
dotnet run
```

### For Team Members:
```powershell
# Connect to shared database (replace IP)
.\connect-to-shared-database.ps1 -ServerIP "192.168.1.100"

# In new terminal - start frontend
cd itams-frontend
ng serve --port 4200 --open
```

---

## 🎉 SUCCESS!
When everything works:
- All team members access the same data
- Users created by one person appear for everyone
- Real-time collaboration on the ITAMS system
- Shared database with all CRUD operations

## 📱 Access Points:
- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5066
- **Swagger Docs**: http://localhost:5066/swagger

Happy collaborating! 🎊