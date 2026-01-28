# Azure SQL Database Setup for ITAMS

## Step 1: Create Azure Account
1. Go to https://azure.microsoft.com/free
2. Sign up for free account (requires credit card but won't charge)
3. Get $200 free credits + 12 months free services

## Step 2: Create SQL Database
1. Go to Azure Portal
2. Create Resource → Databases → SQL Database
3. Settings:
   - **Database name**: `itams-db`
   - **Server**: Create new server
   - **Pricing tier**: Basic (5 DTU, 2GB) - ~$5/month
   - **Location**: Choose closest region

## Step 3: Configure Firewall
1. Go to your SQL Server resource
2. Security → Networking
3. Add rule: "Allow Azure services" = Yes
4. Add your team's IP addresses

## Step 4: Get Connection String
1. Go to your database
2. Settings → Connection strings
3. Copy ADO.NET connection string

Example:
```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=itams-db;Persist Security Info=False;User ID=your-admin;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Step 5: Update ITAMS
Update appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=itams-db;Persist Security Info=False;User ID=your-admin;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

## Step 6: Migrate Data
```bash
# Update database with existing migrations
dotnet ef database update
```

## Benefits:
✅ Same SQL Server (no code changes)
✅ Professional grade
✅ Automatic backups
✅ Always available
✅ Scales with your needs
✅ Microsoft support