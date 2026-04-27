# ITAMS — IT Asset Management System

A full-stack web application for managing IT assets across projects and locations.
Built for **Elsamex India Pvt. Ltd.**

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Angular 20, Bootstrap 5, Chart.js |
| Backend | ASP.NET Core (.NET 10), C#, Entity Framework Core |
| Database | Microsoft SQL Server |
| Auth | JWT Bearer Tokens |
| Reports | iText7 (PDF), EPPlus (Excel) |

---

## Project Structure

```
ITAMS/
├── Controllers/          # API controllers (REST endpoints)
├── Services/             # Business logic layer
├── Domain/
│   ├── Entities/         # EF Core entity models
│   └── Interfaces/       # Service/repository interfaces
├── Data/                 # DbContext and repositories
├── Models/               # DTOs (request/response models)
├── Middleware/           # Custom middleware (auth, activity tracking)
├── Utilities/            # Helpers (DateTimeHelper, etc.)
├── wwwroot/              # Static files (logo, etc.)
├── Migrations/           # SQL migration scripts + schema.sql
├── itams-frontend/       # Angular frontend (separate app)
├── docs/                 # Architecture and reference documentation
├── appsettings.json      # Backend configuration
└── ITAMS.csproj          # .NET project file
```

---

## 1. Database Setup

Run the single schema file on your SQL Server instance:

```sql
sqlcmd -S <your-server> -U <username> -P <password> -i Migrations/schema.sql
```

Or open `Migrations/schema.sql` in SQL Server Management Studio and execute it.

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "SharedSqlServer": "Server=<your-server>;Database=ITAMS_Shared;User Id=<user>;Password=<password>;TrustServerCertificate=true"
}
```

---

## 2. Backend Setup

**Prerequisites:** .NET 10 SDK

```bash
# Restore packages
dotnet restore

# Run in development
dotnet run

# Publish for production
dotnet publish -c Release -o ./publish
```

The API runs on `http://localhost:5000` by default (configurable in `Properties/launchSettings.json`).

**Key configuration in `appsettings.json`:**
- `ConnectionStrings.SharedSqlServer` — SQL Server connection string
- `Email.*` — SMTP settings for alert emails
- JWT settings for authentication

---

## 3. Frontend Setup

**Prerequisites:** Node.js 18+, Angular CLI 20

```bash
cd itams-frontend

# Install dependencies
npm install

# Run development server (proxies API calls to backend)
ng serve
# App available at http://localhost:4200

# Build for production
ng build --configuration production
# Output in itams-frontend/dist/itams-frontend/
```

The dev server proxies `/api/*` requests to the backend via `proxy.conf.json`.

For production, deploy the `dist/` output to any static web server (Nginx, IIS, Apache) and configure it to proxy `/api/*` to the backend server.

---

## 4. Integration Notes

- The frontend communicates with the backend exclusively via REST API at `/api/*`
- Authentication uses JWT — the frontend stores the token and sends it as `Authorization: Bearer <token>` on every request
- CORS is configured in the backend `Program.cs` — update allowed origins for your deployment URL
- All timestamps use IST (UTC+5:30)

---

## 5. Default Credentials

| Role | Username | Password |
|------|----------|----------|
| Super Admin | `superadmin` | *(set during DB setup)* |

---

## Documentation

Full technical documentation is available in the `docs/` folder:

- `docs/APPLICATION_FLOW.md` — end-to-end application flow
- `docs/DATABASE_STRUCTURE.md` — database tables and relationships
- `docs/TECHNICAL_ARCHITECTURE.md` — system architecture overview
- `docs/SESSION_ARCHITECTURE.md` — session and auth design
- `docs/guides/ACCESS_CONTROL_IMPLEMENTATION.md` — RBAC and access control
- `Migrations/README.md` — database migration history
