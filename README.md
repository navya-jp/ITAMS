# ITAMS — IT Asset Management System

A full-stack web application for managing IT assets across projects and locations.
Built for **Elsamex India Pvt. Ltd.**

---

## Prerequisites — Install These First

On a clean PC, install the following before anything else:

| # | Software | Version | Download |
|---|----------|---------|----------|
| 1 | .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| 2 | Node.js | 18 LTS+ | https://nodejs.org |
| 3 | Angular CLI | 20+ | Run: `npm install -g @angular/cli` |
| 4 | SQL Server | 2019+ | https://www.microsoft.com/en-us/sql-server/sql-server-downloads |
| 5 | SSMS | Latest | https://aka.ms/ssmsfullsetup |
| 6 | Git | Latest | https://git-scm.com |

> SQL Server Express (free edition) is sufficient. SSMS is the GUI tool to manage the database.

After installing Node.js, open a terminal and run:
```bash
npm install -g @angular/cli
```

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
├── Middleware/           # Custom middleware
├── Utilities/            # Helpers (DateTimeHelper, etc.)
├── wwwroot/              # Static files (logo, etc.)
├── Migrations/           # schema.sql + migration history
├── itams-frontend/       # Angular frontend
├── docs/                 # Architecture and reference docs
├── appsettings.json      # Backend configuration
└── ITAMS.csproj          # .NET project file
```

---

## Step 1 — Database Setup

### Prerequisites
- Microsoft SQL Server (2019 or later) or SQL Server Express

### Instructions

**1. Create a new database** in SQL Server Management Studio (SSMS):
```sql
CREATE DATABASE ITAMS_DB;
GO
```

**2. Open `Migrations/schema.sql`** in SSMS:
- Go to File → Open → File
- Select `Migrations/schema.sql`

**3. Make sure the correct database is selected** in the dropdown at the top of SSMS (should show `ITAMS_DB`, not `master`).

**4. Click Execute** — this creates all 56 tables with correct columns, constraints, and foreign keys.

> If you get errors on re-run, the script uses `IF OBJECT_ID ... IS NULL` guards so it is safe to run multiple times.

---

## Step 2 — Backend Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Instructions

**1. Clone the repository:**
```bash
git clone https://github.com/navya-jp/ITAMS.git
cd ITAMS
```

**2. Update the connection string** in `appsettings.json`:
```json
"ConnectionStrings": {
  "SharedSqlServer": "Server=<your-server>;Database=ITAMS_DB;User Id=<user>;Password=<password>;TrustServerCertificate=true"
}
```
Replace `<your-server>`, `<user>`, and `<password>` with your SQL Server details.

For Windows Authentication (no username/password):
```json
"SharedSqlServer": "Server=<your-server>;Database=ITAMS_DB;Trusted_Connection=True;TrustServerCertificate=true"
```

**3. Run the backend:**
```bash
dotnet run
```

The API will start on `http://localhost:5066`.

**4. To publish for production:**
```bash
dotnet publish -c Release -o ./publish
```
Deploy the `publish/` folder to IIS or any .NET-compatible host.

---

## Step 3 — Frontend Setup

### Prerequisites
- [Node.js 18+](https://nodejs.org/)
- Angular CLI: `npm install -g @angular/cli`

### Instructions

**1. Navigate to the frontend folder:**
```bash
cd itams-frontend
```

**2. Install dependencies:**
```bash
npm install
```

**3. Run in development mode** (proxies API calls to backend automatically):
```bash
ng serve
```
App will be available at `http://localhost:4200`.

> The backend must be running at `http://localhost:5066` for the frontend to work.

**4. Build for production:**
```bash
ng build --configuration production
```
Output will be in `itams-frontend/dist/itams-frontend/browser/`.
Deploy this folder to any static web server (Nginx, IIS, Apache).

**5. For production deployment**, configure your web server to:
- Serve the `dist/` folder as static files
- Proxy all `/api/*` requests to the backend server URL
- Redirect all routes to `index.html` (for Angular routing)

---

## Step 4 — Running Both Together (Development)

You need **two terminals** running simultaneously:

**Terminal 1 — Backend:**
```bash
cd ITAMS
dotnet run
```

**Terminal 2 — Frontend:**
```bash
cd ITAMS/itams-frontend
ng serve
```

Then open `http://localhost:4200` in your browser.

---

## Configuration Reference

### appsettings.json

| Key | Description |
|-----|-------------|
| `ConnectionStrings.SharedSqlServer` | SQL Server connection string |
| `Email.SmtpHost` | SMTP server for alert emails |
| `Email.SmtpUser` | SMTP username |
| `Email.SmtpPassword` | SMTP password |

### Frontend API URL

The frontend proxies API calls via `itams-frontend/proxy.conf.json`. In development this points to `http://localhost:5066`. For production, update your web server proxy config to point to your deployed backend URL.

---

## Default Login

After setting up the database, a superadmin user must be seeded manually or via the existing data in `Migrations/20260325_InsertHOUsers.sql`.

| Role | Username | Password |
|------|----------|----------|
| Super Admin | `superadmin` | `Admin@123` |

---

## Documentation

| File | Description |
|------|-------------|
| `docs/APPLICATION_FLOW.md` | End-to-end application flow |
| `docs/DATABASE_STRUCTURE.md` | Database tables and relationships |
| `docs/TECHNICAL_ARCHITECTURE.md` | System architecture overview |
| `docs/SESSION_ARCHITECTURE.md` | Session and auth design |
| `docs/guides/ACCESS_CONTROL_IMPLEMENTATION.md` | RBAC and access control |
| `Migrations/README.md` | Database migration history |
| `docs/specs/` | Feature requirements and design specs |
