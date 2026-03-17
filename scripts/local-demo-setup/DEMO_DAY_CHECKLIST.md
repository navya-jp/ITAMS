# Demo Day Checklist 📋

## Before Demo (30 minutes before)

### 1. Verify SQL Server is Running
```powershell
Get-Service MSSQL*
# If not running:
Start-Service MSSQLSERVER
```

### 2. Switch to Local Database
```powershell
cd C:\Users\Navya\Downloads\ITAMS-1
.\scripts\local-demo-setup\switch-to-local.ps1
```

### 3. Verify Data
```powershell
sqlcmd -S .\SQLEXPRESS -d ITAMS_Local -Q "SELECT COUNT(*) as Users FROM Users; SELECT COUNT(*) as Assets FROM Assets"
```
Expected: 24 users, 799 assets

### 4. Start Backend
```powershell
dotnet run
```
Wait for: "Now listening on: http://localhost:5066"

### 5. Start Frontend (New Terminal)
```powershell
cd itams-frontend
npm start
```
Wait for: "Compiled successfully"

### 6. Test Login
- Open: http://localhost:4200
- Username: `superadmin`
- Password: (your password)
- Verify: Dashboard loads with data

## During Demo

### Login Credentials
- **Super Admin:** superadmin
- **Admin:** admin2, admin6
- **Regular User:** navya.pradeepkumar

### Key Features to Demonstrate
1. ✅ Login and authentication
2. ✅ Dashboard with statistics
3. ✅ Asset list (799 assets)
4. ✅ Create new asset
5. ✅ Edit existing asset
6. ✅ Bulk upload
7. ✅ User management
8. ✅ Role-based access control
9. ✅ Audit trail
10. ✅ Reports

### Quick Stats to Mention
- 24 users across 5 roles
- 799 assets managed
- 6 projects
- 7 locations
- Completely offline capable

## After Demo

### Switch Back to Shared Database
```powershell
.\scripts\local-demo-setup\switch-to-shared.ps1
```

### Stop Services
- Press Ctrl+C in backend terminal
- Press Ctrl+C in frontend terminal

## Troubleshooting

### "Cannot connect to database"
```powershell
# Check SQL Server
Get-Service MSSQL*
Start-Service MSSQLSERVER

# Verify connection string
.\scripts\local-demo-setup\switch-to-local.ps1
```

### "Login failed"
- Verify superadmin exists:
```powershell
sqlcmd -S .\SQLEXPRESS -d ITAMS_Local -Q "SELECT * FROM Users WHERE Username = 'superadmin'"
```

### "No assets showing"
- Verify data:
```powershell
sqlcmd -S .\SQLEXPRESS -d ITAMS_Local -Q "SELECT COUNT(*) FROM Assets"
```
- Should show 799

### "Frontend won't start"
```powershell
cd itams-frontend
npm install
npm start
```

### "Backend won't start"
```powershell
dotnet restore
dotnet build
dotnet run
```

## Emergency Backup Plan

If something goes wrong, re-sync from shared database:
```powershell
# Make sure shared database is accessible
.\scripts\local-demo-setup\copy-users.ps1
.\scripts\local-demo-setup\copy-roles-locations.ps1
.\scripts\local-demo-setup\copy-data-dotnet.ps1
```

## Quick Commands Reference

| Task | Command |
|------|---------|
| Switch to Local | `.\scripts\local-demo-setup\switch-to-local.ps1` |
| Switch to Shared | `.\scripts\local-demo-setup\switch-to-shared.ps1` |
| Start Backend | `dotnet run` |
| Start Frontend | `cd itams-frontend && npm start` |
| Check SQL Server | `Get-Service MSSQL*` |
| Verify Data | `.\scripts\local-demo-setup\switch-to-local.ps1` |

## Success Indicators

✅ SQL Server service is running
✅ Backend shows "Now listening on: http://localhost:5066"
✅ Frontend shows "Compiled successfully"
✅ Can login with superadmin
✅ Dashboard shows data
✅ Assets list shows 799 items

## Contact Info (for your reference)

- Local Database: `.\SQLEXPRESS` / `ITAMS_Local`
- Shared Database: `192.168.208.26,1433` / `ITAMS_Shared`
- Backend Port: 5066
- Frontend Port: 4200

---

**Remember:** Your local database is completely independent. You can demo confidently without internet or shared database access!

**Good luck! 🚀**
