# Supabase Setup for ITAMS Team Database

## Step 1: Create Supabase Account
1. Go to https://supabase.com
2. Sign up with GitHub/Google
3. Create new project
4. Choose region closest to your team
5. Set database password (save this!)

## Step 2: Get Connection Details
After project creation, go to Settings → Database:
- Host: `db.xxx.supabase.co`
- Database: `postgres`
- Port: `5432`
- User: `postgres`
- Password: (what you set)

## Step 3: Update ITAMS for PostgreSQL
Install PostgreSQL provider:
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Update Program.cs:
```csharp
// Replace SQL Server with PostgreSQL
builder.Services.AddDbContext<ITAMSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Update appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.xxx.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

## Step 4: Migrate Data
```bash
# Create new migration for PostgreSQL
dotnet ef migrations add InitialPostgreSQL
dotnet ef database update
```

## Benefits:
✅ Always available (24/7)
✅ Free tier (500MB database)
✅ Automatic backups
✅ Team can work independently
✅ Built-in admin panel
✅ API auto-generated