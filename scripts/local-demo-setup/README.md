# Local Database Setup for Demo

This guide will help you set up a local SQL Server database with replicated data from the shared database for your college project demo.

## Prerequisites

- SQL Server Express (or any SQL Server edition) installed locally
- SQL Server Management Studio (SSMS) or Azure Data Studio
- Access to the shared database (one-time, to export data)

## Quick Start

1. **Install SQL Server Express** (if not already installed)
   - Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Choose "Express" edition (free)
   - During installation, enable "SQL Server and Windows Authentication mode"

2. **Run the setup script**
   ```powershell
   .\scripts\local-demo-setup\setup-local-database.ps1
   ```

3. **Switch to local database**
   - Edit `appsettings.json`
   - Change connection string to use local database

4. **Run the application**
   ```powershell
   dotnet run
   ```

## Detailed Steps

### Step 1: Export Data from Shared Database

Run this script while connected to the shared database:

```powershell
.\scripts\local-demo-setup\export-shared-data.ps1
```

This will create SQL files in `scripts/local-demo-setup/exported-data/` containing all your data.

### Step 2: Create Local Database

Run this script to create and configure your local database:

```powershell
.\scripts\local-demo-setup\setup-local-database.ps1
```

This script will:
- Create a new database named `ITAMS_Local`
- Run all Entity Framework migrations
- Import the exported data
- Verify the setup

### Step 3: Configure Connection String

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**For easy switching**, you can use environment-specific files:

- `appsettings.Development.json` - Local database
- `appsettings.Production.json` - Shared database

### Step 4: Verify Setup

Run the verification script:

```powershell
.\scripts\local-demo-setup\verify-local-setup.ps1
```

This will check:
- Database exists
- All tables are created
- Data is imported correctly
- Foreign key relationships are intact

## Connection String Options

### Local Database (Demo)
```json
"Server=localhost;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=True"
```

### Shared Database (Team)
```json
"Server=192.168.208.26,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"
```

### SQLite (Portable Demo)
```json
"Data Source=ITAMS_Demo.db"
```

## Demo Data Included

The exported data will include:

### Master Data
- **Roles**: SuperAdmin, Admin, Manager, User
- **Permissions**: All RBAC permissions
- **Projects**: Sample projects with configurations
- **Locations**: Sample locations hierarchy
- **Vendors**: Common vendors
- **Asset Statuses**: In Use, Spare, Repair, Decommissioned
- **Asset Categories**: Hardware, Software, Digital
- **Asset Types**: Laptop, Desktop, Server, etc.

### Sample Data
- **Users**: 5-10 demo users with different roles
- **Assets**: 20-50 sample assets with realistic data
- **Audit Logs**: Recent activity logs
- **System Settings**: Configured timeouts and settings

## Troubleshooting

### Issue: Cannot connect to local SQL Server

**Solution:**
1. Check if SQL Server is running:
   ```powershell
   Get-Service MSSQL*
   ```

2. Start SQL Server if stopped:
   ```powershell
   Start-Service MSSQLSERVER
   ```

3. Verify connection string server name:
   - Try: `localhost`
   - Or: `(localdb)\MSSQLLocalDB`
   - Or: `.\SQLEXPRESS`

### Issue: Login failed for user

**Solution:**
1. Use Windows Authentication (Trusted_Connection=True)
2. Or create SQL Server login:
   ```sql
   CREATE LOGIN itams_local WITH PASSWORD = 'YourPassword123!';
   CREATE USER itams_local FOR LOGIN itams_local;
   ALTER ROLE db_owner ADD MEMBER itams_local;
   ```

### Issue: Database already exists

**Solution:**
```powershell
.\scripts\local-demo-setup\reset-local-database.ps1
```

### Issue: Migration errors

**Solution:**
```powershell
# Drop and recreate database
dotnet ef database drop --force
dotnet ef database update
```

## Demo Checklist

Before your demo, verify:

- [ ] Local database is running
- [ ] Application starts without errors
- [ ] Can login with demo credentials
- [ ] Can view assets list
- [ ] Can create/edit assets
- [ ] Can upload bulk assets
- [ ] Can view audit trail
- [ ] Can manage users and roles
- [ ] All features work offline

## Demo Credentials

After setup, you can login with:

**SuperAdmin:**
- Username: `superadmin`
- Password: `Admin@123`

**Regular User:**
- Username: `demouser`
- Password: `Demo@123`

## Backup and Restore

### Create Backup
```powershell
.\scripts\local-demo-setup\backup-local-database.ps1
```

### Restore Backup
```powershell
.\scripts\local-demo-setup\restore-local-database.ps1
```

## Switching Back to Shared Database

1. Edit `appsettings.json`
2. Change connection string back to shared database
3. Restart application

No data migration needed - your local database remains untouched.

## Files Created

```
scripts/local-demo-setup/
├── README.md (this file)
├── setup-local-database.ps1
├── export-shared-data.ps1
├── import-data.ps1
├── verify-local-setup.ps1
├── reset-local-database.ps1
├── backup-local-database.ps1
├── restore-local-database.ps1
├── create-demo-users.sql
├── exported-data/
│   ├── 01-roles.sql
│   ├── 02-permissions.sql
│   ├── 03-users.sql
│   ├── 04-projects.sql
│   ├── 05-locations.sql
│   ├── 06-assets.sql
│   └── ... (other tables)
└── backups/
    └── ITAMS_Local_backup.bak
```

## Support

If you encounter issues:
1. Check the troubleshooting section above
2. Review the logs in `logs/` folder
3. Verify SQL Server is running
4. Check connection string syntax

## Notes

- The local database is completely independent
- Changes to local database don't affect shared database
- You can reset and re-import data anytime
- Keep the exported data files for future demos
