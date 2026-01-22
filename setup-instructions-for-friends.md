# 🚀 ITAMS Project Setup Instructions

## 📋 Prerequisites
Make sure you have these installed:
- **Git** (https://git-scm.com/)
- **.NET 10 SDK** (https://dotnet.microsoft.com/download)
- **Node.js 18+** (https://nodejs.org/)
- **Angular CLI**: `npm install -g @angular/cli`
- **SQL Server LocalDB** (comes with Visual Studio or SQL Server Express)

## 🔽 Step 1: Clone the Repository
```bash
git clone https://github.com/navya-jp/ITAMS.git
cd ITAMS
```

## 🔧 Step 2: Setup Backend (.NET)
```bash
# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run database migrations (creates database automatically)
dotnet ef database update

# Start the backend server
dotnet run
```
**Backend will run on:** http://localhost:5066

## 🎨 Step 3: Setup Frontend (Angular)
Open a **new terminal/command prompt** and run:
```bash
# Navigate to frontend folder
cd itams-frontend

# Install npm packages
npm install

# Start the Angular development server
ng serve --port 4200 --open
```
**Frontend will run on:** http://localhost:4200

## 🌐 Step 4: Access the Application
- **Main App:** http://localhost:4200
- **API Documentation:** http://localhost:5066/swagger
- **Backend API:** http://localhost:5066/api

## 🧪 Step 5: Test Everything Works
```bash
# Test APIs (run from main ITAMS folder)
./test-all-apis.ps1

# Or test individual endpoints
curl http://localhost:5066/api/users
curl http://localhost:5066/api/superadmin/roles
```

## 📱 Step 6: Use the Application
1. Go to http://localhost:4200
2. Click "User Management" in sidebar
3. Click "Add User" to create users
4. Test all CRUD operations!

## 🔍 Troubleshooting

### If Backend Fails:
```bash
# Check if .NET 10 is installed
dotnet --version

# If database issues, reset database
dotnet ef database drop
dotnet ef database update
```

### If Frontend Fails:
```bash
# Check Node.js version
node --version

# Clear npm cache and reinstall
npm cache clean --force
rm -rf node_modules
npm install
```

### If Ports are Busy:
```bash
# Kill processes on ports
# Windows:
netstat -ano | findstr :5066
taskkill /PID <PID_NUMBER> /F

# Or use different ports:
dotnet run --urls="http://localhost:5067"
ng serve --port 4201
```

## 📊 Project Structure
```
ITAMS/
├── Controllers/          # API Controllers
├── Services/            # Business Logic
├── Data/               # Database & Repositories
├── Models/             # DTOs & Data Models
├── Domain/             # Entities & Interfaces
├── itams-frontend/     # Angular Frontend
├── test-*.ps1         # Testing Scripts
└── *.sql              # Database Scripts
```

## 🎯 Quick Start (One-liner)
```bash
git clone https://github.com/navya-jp/ITAMS.git && cd ITAMS && dotnet run &
cd itams-frontend && npm install && ng serve --open
```

## 🆘 Need Help?
- Check the `test-frontend-checklist.md` for testing guide
- Run `./test-all-apis.ps1` to verify APIs
- Check browser console (F12) for frontend errors
- Check terminal output for backend errors

Happy coding! 🎉