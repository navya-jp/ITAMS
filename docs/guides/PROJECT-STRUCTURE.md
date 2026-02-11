# ITAMS Project Structure

## Core Application

### Backend (ASP.NET Core)
```
Controllers/          - API endpoints
├── AuthController.cs           - Authentication & login
├── RbacController.cs           - Role-based access control
├── UserPermissionController.cs - User permission management
├── UsersController.cs          - User CRUD operations
├── RoleController.cs           - Role management
├── ProjectManagementController.cs
└── SuperAdminController.cs

Domain/
├── Entities/         - Database models
│   ├── RBAC/        - RBAC system entities
│   ├── User.cs
│   ├── Role.cs
│   ├── Project.cs
│   └── Asset.cs
└── Interfaces/       - Service contracts

Data/
├── ITAMSDbContext.cs - Database context
└── Repositories/     - Data access layer

Services/
├── RBAC/            - RBAC business logic
│   ├── PermissionResolver.cs
│   ├── RbacAuditService.cs
│   └── RoleManagementService.cs
├── UserService.cs
├── ProjectService.cs
└── AuditService.cs

Migrations/          - Database migrations
├── 20260130_CreateRBACCoreTables_Simple.sql
├── 20260209_AssignDefaultRolePermissions_Fixed.sql
└── 20260209_AddAuditColumnsToAllTables.sql
```

### Frontend (Angular)
```
itams-frontend/src/app/
├── login/           - Login page
├── dashboard/       - Admin dashboard
├── user-dashboard/  - User dashboard
├── users/           - User management
├── roles/           - Role management UI
├── user-permissions/ - User permission assignment
├── projects/        - Project management
├── locations/       - Location management
├── assets/          - Asset management
├── guards/          - Route guards
├── services/        - API services
│   ├── api.ts      - HTTP client
│   └── auth.service.ts
└── shared/          - Shared utilities
```

## Key Features

### RBAC System
- 5 Roles: Super Admin, Admin, IT Staff, Auditor, Project Manager
- 31 Permissions across 5 modules
- Role-based and user-specific permission overrides
- Audit logging for all permission changes

### Database
- SQL Server (Shared: 192.168.208.10)
- 18 tables with full audit columns
- Foreign key relationships documented

## Running the Application

### Backend
```bash
dotnet run
# Runs on http://localhost:5066
```

### Frontend
```bash
cd itams-frontend
npm start
# Runs on http://localhost:4200
```

### Default Login
- Username: `superadmin`
- Password: `Admin@123`

## Additional Resources
- Setup guides: `docs/setup/`
- Technical guides: `docs/guides/`
- Database scripts: `scripts/database-setup/`
- Test files: `scripts/testing/`
