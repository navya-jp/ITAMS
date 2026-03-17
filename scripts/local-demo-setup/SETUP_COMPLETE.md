# Local Demo Setup - Complete! ✅

Your local database has been successfully set up with all the data from the shared database.

## What Was Done

1. ✅ Created local database `ITAMS_Local` on SQL Server Express
2. ✅ Ran all necessary migrations to match the shared database schema
3. ✅ Copied all essential data:
   - **24 Users** (including superadmin)
   - **5 Roles** (Super Admin, Admin, IT Staff, Project Manager, Auditor)
   - **6 Projects**
   - **7 Locations**
   - **799 Assets**
   - **7 System Settings**
4. ✅ Updated connection string to use local database

## Current Configuration

**Database:**
- Server: `.\SQLEXPRESS`
- Database: `ITAMS_Local`
- Connection: Windows Authentication (Trusted Connection)

**Application:**
- Backend: http://localhost:5066
- Frontend: http://localhost:4200

## Demo Credentials

**Super Admin:**
- Username: `superadmin`
- Password: (same as shared database)

**Other Users:**
- All 24 users from shared database are available
- Passwords are preserved (hashed)

## How to Use

### Start the Application

1. **Backend:**
   ```powershell
   dotnet run
   ```

2. **Frontend:**
   ```powershell
   cd itams-frontend
   npm start
   ```

3. **Access:**
   - Open browser: http://localhost:4200
   - Login with superadmin credentials

### Switch Between Databases

**Switch to Local Database (for demo):**
```powershell
.\scripts\local-demo-setup\switch-to-local.ps1
```

**Switch to Shared Database (for team work):**
```powershell
.\scripts\local-demo-setup\switch-to-shared.ps1
```

**Note:** Restart the application after switching databases.

## What Works Offline

✅ User login and authentication
✅ Asset management (view, create, edit, delete)
✅ Bulk upload
✅ User management
✅ Role management
✅ Project management
✅ Location management
✅ Dashboard and reports
✅ Audit trail
✅ System settings

## Demo Checklist

Before your presentation:

- [ ] Verify SQL Server Express is running
- [ ] Switch to local database: `.\scripts\local-demo-setup\switch-to-local.ps1`
- [ ] Start backend: `dotnet run`
- [ ] Start frontend: `cd itams-frontend && npm start`
- [ ] Test login with superadmin
- [ ] Verify assets are visible (799 assets)
- [ ] Test key features:
  - [ ] View assets list
  - [ ] Create new asset
  - [ ] Edit existing asset
  - [ ] Bulk upload
  - [ ] User management
  - [ ] Reports/dashboard

## Troubleshooting

### Application won't start
- Check if SQL Server Express is running: `Get-Service MSSQL*`
- Verify connection string in `appsettings.json`

### Can't login
- Verify users were copied: `sqlcmd -S .\SQLEXPRESS -d ITAMS_Local -Q "SELECT COUNT(*) FROM Users"`
- Check if superadmin exists: `sqlcmd -S .\SQLEXPRESS -d ITAMS_Local -Q "SELECT * FROM Users WHERE Username = 'superadmin'"`

### No data visible
- Verify data was copied: `.\scripts\local-demo-setup\switch-to-local.ps1` (shows summary)
- Re-run data copy if needed: `.\scripts\local-demo-setup\copy-data-dotnet.ps1`

## Backup and Restore

### Create Backup
```powershell
sqlcmd -S .\SQLEXPRESS -Q "BACKUP DATABASE ITAMS_Local TO DISK='C:\Temp\ITAMS_Local_Backup.bak'"
```

### Restore Backup
```powershell
sqlcmd -S .\SQLEXPRESS -Q "RESTORE DATABASE ITAMS_Local FROM DISK='C:\Temp\ITAMS_Local_Backup.bak' WITH REPLACE"
```

## Re-sync Data from Shared Database

If you need to update local database with latest data from shared database:

```powershell
# Make sure shared database is accessible
.\scripts\local-demo-setup\copy-users.ps1
.\scripts\local-demo-setup\copy-roles-locations.ps1
.\scripts\local-demo-setup\copy-data-dotnet.ps1
```

## Files Created

All setup scripts are in `scripts/local-demo-setup/`:

- `setup-complete-local-demo.ps1` - Complete setup (run once)
- `switch-to-local.ps1` - Switch to local database
- `switch-to-shared.ps1` - Switch to shared database
- `copy-users.ps1` - Copy users from shared to local
- `copy-roles-locations.ps1` - Copy roles and locations
- `copy-data-dotnet.ps1` - Copy all other data
- `run-all-migrations.ps1` - Run database migrations
- `README.md` - Detailed documentation
- `QUICK_START.md` - Quick reference guide

## Important Notes

- Local database is completely independent from shared database
- Changes to local database don't affect shared database
- You can reset local database anytime by re-running the setup scripts
- Keep the exported data files safe for future demos
- The local database is perfect for offline demos and presentations

## Success! 🎉

Your application is now ready for offline demo. You can present your project without depending on the shared database or internet connection.

**Good luck with your college project review!** 🚀
