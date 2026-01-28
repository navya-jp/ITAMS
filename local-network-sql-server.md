# Local Network SQL Server Express Setup

## If Your Team Works from Same Location/Office

### Requirements:
- All team members on same local network (WiFi/LAN)
- One computer designated as database server
- Router with port forwarding capability (for external access)

### Setup Steps:

#### 1. Choose Server Computer
- Computer with best uptime (desktop preferred over laptop)
- Reliable power supply (consider UPS)
- Good network connection

#### 2. Install & Configure SQL Server Express
```powershell
# Install SQL Server Express with Advanced Services
# Run our setup script
.\setup-sql-express-server.ps1
```

#### 3. Configure Router (Optional - for external access)
If team needs access from outside office:
1. **Port Forwarding**:
   - Forward external port 1433 to server computer's IP:1433
   - Forward external port 1434 to server computer's IP:1434

2. **Dynamic DNS** (if IP changes):
   - Use services like No-IP, DynDNS
   - Get a hostname like `yourteam.ddns.net`

#### 4. Team Connection Strings

**Local Network Access:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.1.100\\SQLEXPRESS;Database=ITAMSSharedDB;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false;"
  }
}
```

**External Access (if configured):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=yourteam.ddns.net\\SQLEXPRESS;Database=ITAMSSharedDB;User Id=itams_user;Password=StrongPassword123;TrustServerCertificate=true;Encrypt=false;"
  }
}
```

### Benefits:
✅ Uses SQL Server Express
✅ No monthly cloud costs
✅ Fast local network speeds
✅ Full control over server

### Drawbacks:
❌ Server computer must stay ON
❌ Depends on local network/power
❌ Manual backup management
❌ Security configuration needed for external access