# ITAMS - IT Asset Management System

A comprehensive web-based IT Asset Management System with role-based access control, multi-project support, and bulk upload capabilities.

## 🚀 Features

- **Asset Management**: Complete CRUD operations with search, filter, and bulk upload
- **Role-Based Access Control (RBAC)**: Granular permissions system with 50+ permissions
- **Multi-Project Support**: Manage assets across multiple projects and locations
- **Bulk Upload**: Excel-based bulk import with validation and error reporting
- **User Management**: User creation, role assignment, session management
- **Audit Trail**: Comprehensive login and activity audit logs
- **Location-Based Access**: Restrict user access by region, state, plaza, or office
- **Session Management**: Automatic session cleanup and security features

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 10.0 (C#)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Excel Processing**: EPPlus library

### Frontend
- **Framework**: Angular 20 (Standalone Components)
- **Language**: TypeScript
- **Styling**: Bootstrap 5 + Custom SCSS
- **Icons**: Font Awesome

## 📋 Prerequisites

- .NET 10.0 SDK
- Node.js 18+ and npm
- SQL Server 2019+
- Git

## 🔧 Installation

### Backend Setup

```bash
# Clone the repository
git clone https://github.com/navya-jp/ITAMS.git
cd ITAMS

# Restore NuGet packages
dotnet restore

# Update connection string in appsettings.json
# ConnectionStrings:DefaultConnection

# Run the application
dotnet run
```

### Frontend Setup

```bash
# Navigate to frontend directory
cd itams-frontend

# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build
```

### Database Setup

1. Create a new SQL Server database named `ITAMS`
2. Update the connection string in `appsettings.json`
3. Run migration scripts from `Migrations/` folder in order
4. Execute seed data scripts from `scripts/database-setup/`

## 📁 Project Structure

```
ITAMS/
├── Controllers/          # API endpoints
├── Domain/
│   ├── Entities/        # Database models
│   └── Interfaces/      # Service interfaces
├── Data/
│   ├── ITAMSDbContext.cs
│   └── Repositories/    # Data access layer
├── Services/            # Business logic
├── Middleware/          # Custom middleware
├── Models/              # DTOs
├── Utilities/           # Helper classes
├── Migrations/          # SQL migration scripts
├── docs/                # Documentation
├── scripts/             # Utility scripts
└── itams-frontend/      # Angular frontend
    └── src/app/
        ├── services/    # API & Auth services
        ├── assets/      # Asset management
        ├── users/       # User management
        ├── audit-trail/ # Audit logs
        └── settings/    # System settings
```

## 🔑 Default Credentials

**Super Admin**
- Username: `superadmin`
- Password: `Admin@123`

> ⚠️ **Important**: Change the default password immediately after first login!

## 📖 Documentation

- [Project Presentation Guide](scripts/docs/PROJECT_PRESENTATION_GUIDE.md) - Complete project overview
- [Technical Architecture](docs/TECHNICAL_ARCHITECTURE.md) - System architecture details
- [Session Architecture](docs/SESSION_ARCHITECTURE.md) - Session management implementation
- [Bulk Upload Guide](docs/BULK_UPLOAD_GUIDE.md) - Bulk upload feature documentation
- [Access Control Implementation](docs/guides/ACCESS_CONTROL_IMPLEMENTATION.md) - RBAC details

## 🔒 Security Features

- JWT-based authentication with token expiration
- BCrypt password hashing with salt
- Role-based access control (RBAC) with granular permissions
- Location-based access restrictions
- Session management with automatic cleanup
- Activity tracking and audit trails
- Input validation on frontend and backend
- SQL injection prevention (parameterized queries)

## 🎯 Key Modules

### 1. Asset Management
- Create, read, update, delete assets
- Search and filter capabilities
- Bulk upload via Excel files
- Tab-based forms for better UX
- Asset lifecycle tracking

### 2. User Management
- User CRUD operations
- Role assignment
- Project and location restrictions
- Session management
- Password reset functionality

### 3. RBAC System
- 50+ granular permissions
- Role creation and management
- Permission assignment to roles
- User-specific permission overrides

### 4. Audit Trail
- Login/logout tracking
- Session duration calculation
- IP address and browser detection
- Time-range filtering
- Username search

### 5. System Settings
- Configurable system parameters
- Category-based organization
- Bulk update capability
- Session timeout configuration

## 🚀 Deployment

### Production Build

```bash
# Backend
dotnet publish -c Release -o ./publish

# Frontend
cd itams-frontend
npm run build
```

### Environment Configuration

Update `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your-Production-Connection-String"
  },
  "Jwt": {
    "Key": "Your-Secure-Secret-Key",
    "Issuer": "ITAMS",
    "Audience": "ITAMS-Users"
  }
}
```

## 🧪 Testing

```bash
# Run backend tests
dotnet test

# Run frontend tests
cd itams-frontend
npm test
```

## 📊 API Documentation

API endpoints are documented in the [Technical Architecture](docs/TECHNICAL_ARCHITECTURE.md) document.

Base URL: `http://localhost:5000/api`

### Main Endpoints
- `/api/auth/*` - Authentication
- `/api/users/*` - User management
- `/api/assets/*` - Asset management
- `/api/rbac/*` - RBAC operations
- `/api/settings/*` - System settings

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

## 👥 Authors

- **Navya JP** - [GitHub](https://github.com/navya-jp)

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Angular team for the powerful frontend framework
- EPPlus for Excel processing capabilities
- Bootstrap team for the responsive UI framework

## 📞 Support

For support, email navya@example.com or open an issue in the GitHub repository.

---

**Built with ❤️ using ASP.NET Core and Angular**
