# Quick Start: Local Demo Setup

## 5-Minute Setup

### Step 1: Export Data (One-Time, While Shared DB is Available)

```powershell
cd C:\Users\Navya\Downloads\ITAMS-1
.\scripts\local-demo-setup\export-data-simple.ps1
```

This creates `ITAMS_Data_Export.sql` with all your data.

### Step 2: Create Local Database

```powershell
# Create database
sqlcmd -S localhost -Q "CREATE DATABASE ITAMS_Local"

# Run migrations
$env:ASPNETCORE_ENVIRONMENT="Local"
dotnet ef database update

# Import data
sqlcmd -S localhost -d ITAMS_Local -i .\scripts\local-demo-setup\ITAMS_Data_Export.sql
```

### Step 3: Run Application

```powershell
# Set environment to use local database
$env:ASPNETCORE_ENVIRONMENT="Local"
dotnet run
```

Or edit `appsettings.json` directly:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ITAMS_Local;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Step 4: Access Application

- URL: http://localhost:5066
- Frontend: http://localhost:4200
- Login: `superadmin` / `Admin@123`

## Alternative: SQLite (Portable)

For maximum portability, use SQLite:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ITAMS_Demo.db"
  }
}
```

Then run:
```powershell
dotnet ef database update
# Import data (you'll need to convert SQL Server syntax to SQLite)
```

## Switching Between Databases

### Method 1: Environment Variables

```powershell
# Local database
$env:ASPNETCORE_ENVIRONMENT="Local"
dotnet run

# Shared database
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

### Method 2: Configuration Files

Create `appsettings.Local.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ITAMS_Local;Trusted_Connection=True"
  }
}
```

Keep `appsettings.Development.json` for shared database.

### Method 3: Command Line

```powershell
dotnet run --ConnectionStrings:DefaultConnection="Server=localhost;Database=ITAMS_Local;Trusted_Connection=True"
```

## Troubleshooting

### SQL Server Not Found

Try these server names:
- `localhost`
- `(localdb)\MSSQLLocalDB`
- `.\SQLEXPRESS`
- `localhost\SQLEXPRESS`

### Login Failed

Use Windows Authentication:
```
Trusted_Connection=True
```

Or create SQL login:
```sql
CREATE LOGIN itams_demo WITH PASSWORD = 'Demo@123!';
USE ITAMS_Local;
CREATE USER itams_demo FOR LOGIN itams_demo;
ALTER ROLE db_owner ADD MEMBER itams_demo;
```

Then use:
```
Server=localhost;Database=ITAMS_Local;User Id=itams_demo;Password=Demo@123!
```

### Database Doesn't Exist

```powershell
sqlcmd -S localhost -Q "CREATE DATABASE ITAMS_Local"
dotnet ef database update
```

### Migration Errors

```powershell
# Reset database
dotnet ef database drop --force
dotnet ef database update
```

## Demo Checklist

Before your presentation:

- [ ] Local SQL Server is running
- [ ] Database has data (check with SSMS)
- [ ] Application starts without errors
- [ ] Can login with demo credentials
- [ ] Test key features:
  - [ ] View assets
  - [ ] Create new asset
  - [ ] Bulk upload
  - [ ] User management
  - [ ] Reports/dashboard

## Backup Your Local Database

```powershell
# Backup
sqlcmd -S localhost -Q "BACKUP DATABASE ITAMS_Local TO DISK='C:\Temp\ITAMS_Local.bak'"

# Restore
sqlcmd -S localhost -Q "RESTORE DATABASE ITAMS_Local FROM DISK='C:\Temp\ITAMS_Local.bak' WITH REPLACE"
```

## Reset and Start Fresh

```powershell
# Drop database
sqlcmd -S localhost -Q "DROP DATABASE ITAMS_Local"

# Recreate
sqlcmd -S localhost -Q "CREATE DATABASE ITAMS_Local"

# Run migrations
dotnet ef database update

# Import data
sqlcmd -S localhost -d ITAMS_Local -i .\scripts\local-demo-setup\ITAMS_Data_Export.sql
```

## Files You Need

Keep these files safe for future demos:

1. `ITAMS_Data_Export.sql` - Your exported data
2. `appsettings.Local.json` - Local configuration
3. `ITAMS_Local.bak` - Database backup (optional)

## Demo Day Workflow

1. **Morning of Demo:**
   ```powershell
   # Verify SQL Server is running
   Get-Service MSSQLSERVER
   
   # Test connection
   sqlcmd -S localhost -Q "SELECT @@VERSION"
   
   # Start application
   $env:ASPNETCORE_ENVIRONMENT="Local"
   dotnet run
   ```

2. **During Demo:**
   - Application runs completely offline
   - No dependency on teammate's machine
   - All features work normally

3. **After Demo:**
   - Switch back to shared database if needed
   - Your local database remains for future demos

## Support

If something goes wrong:

1. Check SQL Server is running: `Get-Service MSSQL*`
2. Verify database exists: `sqlcmd -S localhost -Q "SELECT name FROM sys.databases"`
3. Check connection string in `appsettings.json`
4. Review application logs in `logs/` folder
5. Try the troubleshooting steps above

## Success Indicators

You're ready for demo when:

✓ Application starts without errors
✓ Login page loads
✓ Can login with demo credentials
✓ Dashboard shows data
✓ Can navigate all pages
✓ No "connection failed" errors
✓ Works without internet connection
