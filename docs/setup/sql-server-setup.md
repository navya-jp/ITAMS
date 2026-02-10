# 🗄️ SQL Server Setup for Team Access

## 📋 Prerequisites
1. **SQL Server Express** (Free) - Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. **SQL Server Management Studio (SSMS)** - Download from: https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms

## 🔧 Step 1: Install SQL Server Express
1. Download SQL Server Express
2. Run installer
3. Choose "Basic" installation
4. **IMPORTANT:** During installation, note the **Server Name** (usually: `DESKTOP-XXXXX\SQLEXPRESS`)
5. Enable **Mixed Mode Authentication**
6. Set **sa** password (remember this!)

## 🌐 Step 2: Enable Network Access
1. Open **SQL Server Configuration Manager**
2. Go to **SQL Server Network Configuration** → **Protocols for SQLEXPRESS**
3. **Enable TCP/IP**
4. Right-click **TCP/IP** → **Properties**
5. Go to **IP Addresses** tab
6. Find **IPAll** section at bottom
7. Set **TCP Port** to **1433**
8. **Restart SQL Server service**

## 🔥 Step 3: Configure Windows Firewall
```cmd
# Run as Administrator
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
```

## 🏗️ Step 4: Create Database
1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your server: `DESKTOP-XXXXX\SQLEXPRESS`
3. Right-click **Databases** → **New Database**
4. Name: **ITAMS**
5. Click **OK**

## 🔑 Step 5: Create Database User
```sql
-- Run this in SSMS
USE ITAMS;

-- Create login
CREATE LOGIN itams_user WITH PASSWORD = 'ITAMS@2024!';

-- Create user in database
CREATE USER itams_user FOR LOGIN itams_user;

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER itams_user;
```

## 📡 Step 6: Get Your Server IP Address
```cmd
# Run this to get your IP address
ipconfig
```
Look for **IPv4 Address** (usually starts with 192.168.x.x or 10.x.x.x)

## 🔧 Step 7: Update Connection String
Your connection string will be:
```
Server=YOUR_IP_ADDRESS,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;
```

Example:
```
Server=192.168.1.100,1433;Database=ITAMS;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true;
```