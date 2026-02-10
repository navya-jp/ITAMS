# Shared Hosting with SQL Server Express

## Budget-Friendly Hosting Options

### Option A: Windows VPS Providers
**Cheap Windows VPS with SQL Server Express:**

1. **Contabo** (~$7/month):
   - Windows Server VPS
   - 4GB RAM, 2 CPU cores
   - Install SQL Server Express yourself

2. **Vultr** (~$10/month):
   - Windows Server instance
   - Good performance
   - Hourly billing

3. **DigitalOcean** (~$12/month):
   - Windows Droplet
   - Easy setup
   - Good documentation

### Option B: Specialized SQL Server Hosting
**Providers that support SQL Server Express:**

1. **SmarterASP.NET** (~$5/month):
   - Shared hosting with SQL Server
   - May support Express edition
   - .NET hosting optimized

2. **DiscountASP.NET** (~$8/month):
   - Windows hosting
   - SQL Server support
   - Good for .NET apps

### Setup Process:
1. **Choose provider and plan**
2. **Set up Windows server/hosting**
3. **Install SQL Server Express** (if VPS)
4. **Configure remote access**
5. **Upload your database**
6. **Share connection details with team**

### Sample Connection String:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server.provider.com\\SQLEXPRESS;Database=ITAMSSharedDB;User Id=your_user;Password=your_password;TrustServerCertificate=true;Encrypt=false;"
  }
}
```

### Benefits:
✅ Uses SQL Server Express
✅ Always available
✅ Managed infrastructure
✅ Professional hosting
✅ Backup services usually included

### Costs:
- VPS: $7-15/month
- Shared hosting: $5-10/month