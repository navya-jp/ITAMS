# Railway Database Setup for ITAMS

## Step 1: Create Railway Account
1. Go to https://railway.app
2. Sign up with GitHub
3. Get $5 monthly credits (free)

## Step 2: Create Database
1. New Project → Add PostgreSQL
2. Database will be created automatically
3. Note the connection details

## Step 3: Get Connection String
Go to your PostgreSQL service → Connect:
```
postgresql://postgres:password@containers-us-west-xxx.railway.app:6543/railway
```

## Step 4: Update ITAMS for PostgreSQL
Install PostgreSQL provider:
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Update Program.cs:
```csharp
builder.Services.AddDbContext<ITAMSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Update appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://postgres:password@containers-us-west-xxx.railway.app:6543/railway"
  }
}
```

## Step 5: Deploy (Optional)
Railway can also host your .NET app:
1. Connect GitHub repo
2. Auto-deploys on push
3. Get public URL for your app

## Benefits:
✅ $5/month free credits
✅ Very simple setup
✅ Can host app + database
✅ Auto-deployments
✅ Great for teams