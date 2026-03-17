# ITAMS - College Review Guide

## Quick Project Overview

**ITAMS** (IT Asset Management System) is a full-stack web application for managing IT assets across multiple projects and locations with role-based access control.

**Tech Stack:**
- Frontend: Angular 20 (TypeScript)
- Backend: ASP.NET Core 10.0 (C#)
- Database: SQL Server with Entity Framework Core
- Authentication: JWT (JSON Web Tokens)

---

## Key Architectural Concepts to Explain

### 1. JWT Authentication Flow

**What is JWT?**
JSON Web Token - a secure way to transmit information between client and server as a JSON object.

**How it works in ITAMS:**

```
1. User logs in → POST /api/auth/login
2. Backend validates credentials
3. Backend generates JWT token with claims (userId, roleId, sessionId)
4. Token sent to frontend
5. Frontend stores token in localStorage
6. Every API request includes token in Authorization header
7. Backend validates token on each request
```

**Code Location:** `Controllers/AuthController.cs` (line 50-80)

**Key Point:** Token contains user identity and expires after 30 minutes for security.

---

### 2. Entity Framework Core (ORM)

**What is EF Core?**
Object-Relational Mapper - translates C# objects to SQL database operations.

**How it works in ITAMS:**

```csharp
// Instead of writing SQL:
// SELECT * FROM Users WHERE Id = 5

// You write C# LINQ:
var user = await _context.Users
    .Include(u => u.Role)
    .FirstOrDefaultAsync(u => u.Id == 5);
```

**Benefits:**
- Type-safe queries (compile-time checking)
- Automatic SQL generation
- Change tracking (knows what changed)
- Navigation properties (automatic joins)

**Code Location:** `Data/ITAMSDbContext.cs`

**Key Concept:** DbContext is the bridge between C# objects and database tables.

---

### 3. Layered Architecture Pattern

**ITAMS follows a clean separation of concerns:**

```
┌─────────────────────────────────────┐
│  Controllers (API Endpoints)        │  ← HTTP requests/responses
├─────────────────────────────────────┤
│  Services (Business Logic)          │  ← Validation, calculations
├─────────────────────────────────────┤
│  Repositories (Data Access)         │  ← Database queries
├─────────────────────────────────────┤
│  Database (SQL Server)              │  ← Data storage
└─────────────────────────────────────┘
```

**Example Flow:**
1. User clicks "Create User" button
2. Frontend sends POST request to `UsersController`
3. Controller calls `UserService.CreateUserAsync()`
4. Service validates data, hashes password
5. Service calls `UserRepository.CreateAsync()`
6. Repository saves to database via EF Core
7. Response flows back up the chain

**Why this matters:** Each layer has one responsibility, making code maintainable and testable.

---

### 4. Repository Pattern

**What is it?**
A pattern that separates data access logic from business logic.

**Interface (Contract):**
```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
}
```

**Implementation:**
```csharp
public class UserRepository : IUserRepository
{
    private readonly ITAMSDbContext _context;
    
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
}
```

**Benefits:**
- Business logic doesn't know about database
- Easy to test (mock the repository)
- Can switch databases without changing business logic

**Code Location:** `Data/Repositories/UserRepository.cs`

---

### 5. Dependency Injection (DI)

**What is it?**
A design pattern where objects receive their dependencies from external sources rather than creating them.

**How it works:**

```csharp
// 1. Register services in Program.cs
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 2. Inject via constructor
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;  // Automatically injected
    }
}
```

**Benefits:**
- Loose coupling (depend on interfaces, not concrete classes)
- Easy testing (inject mock objects)
- Automatic lifecycle management

**Code Location:** `Program.cs` (lines 65-85)

---

### 6. Middleware Pattern

**What is Middleware?**
Components that form a pipeline to process HTTP requests.

**ITAMS Middleware Pipeline:**
```
Request → CORS → Authentication → Maintenance Mode → 
Activity Tracking → Project Access Control → Controllers
```

**Example: ProjectAccessControlMiddleware**

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // 1. Extract userId from JWT token
    var userId = GetUserIdFromToken(context);
    
    // 2. Check if user is SuperAdmin
    var isSuperAdmin = CheckSuperAdmin(userId);
    
    // 3. Get user's assigned ProjectId
    var projectId = await GetUserProjectId(userId);
    
    // 4. Store in HttpContext.Items for controllers to use
    context.Items["UserId"] = userId;
    context.Items["ProjectId"] = projectId;
    context.Items["IsSuperAdmin"] = isSuperAdmin;
    
    // 5. Call next middleware
    await _next(context);
}
```

**Why it matters:** Runs before every API request, enforcing multi-tenancy (project isolation).

**Code Location:** `Middleware/ProjectAccessControlMiddleware.cs`

---

### 7. DTO Pattern (Data Transfer Objects)

**What is it?**
Objects that carry data between processes, separate from database entities.

**Why use DTOs?**

```csharp
// Database Entity (internal)
public class Asset
{
    public int Id { get; set; }
    public AssetStatus Status { get; set; }  // Enum (1, 2, 3)
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    // ... 30+ fields
}

// DTO (API response)
public class AssetDto
{
    public int Id { get; set; }
    public string AssetId { get; set; }  // "AST00001"
    public string Status { get; set; }   // "In Use" (human-readable)
    public string? AssignedUserName { get; set; }  // Computed field
    // ... only fields frontend needs
}
```

**Benefits:**
- Security (don't expose password hashes, internal IDs)
- Flexibility (transform data, compute fields)
- API versioning (change DTO without changing database)
- Performance (send only needed data)

**Code Location:** `Models/AssetDtos.cs`

---

### 8. RBAC (Role-Based Access Control)

**What is it?**
A security model where permissions are assigned to roles, and users are assigned roles.

**ITAMS RBAC Structure:**

```
User → Role → Permissions
  ↓      ↓         ↓
John → Admin → [USERS_VIEW, USERS_CREATE, ASSETS_VIEW, ...]
```

**5 Default Roles:**
1. Super Admin (31 permissions) - Full system access
2. Admin (24 permissions) - Project management
3. IT Staff (14 permissions) - Asset management
4. Project Manager (11 permissions) - Project oversight
5. Auditor (9 permissions) - View-only + reports

**Permission Check Example:**
```csharp
// Check if user has permission
var hasPermission = await _permissionService
    .HasPermissionAsync(userId, "ASSETS_CREATE");

if (!hasPermission)
{
    return Forbidden("You don't have permission to create assets");
}
```

**Code Location:** `Services/RBAC/PermissionResolver.cs`

---

### 9. Multi-Tenancy (Project Isolation)

**What is it?**
Multiple customers (projects) share the same application but data is isolated.

**ITAMS Implementation:**

```
SuperAdmin (RoleId = 1)
  ↓
  Can access ALL projects

Other Users
  ↓
  Assigned to ONE project (ProjectId)
  ↓
  Can only see/modify data in their project
```

**How it's enforced:**
1. `ProjectAccessControlMiddleware` extracts user's ProjectId
2. Controllers filter queries by ProjectId
3. Database foreign keys ensure data integrity

**Example Query:**
```csharp
// Non-SuperAdmin users
var assets = await _context.Assets
    .Where(a => a.ProjectId == userProjectId)
    .ToListAsync();

// SuperAdmin sees all
var assets = await _context.Assets.ToListAsync();
```

---

### 10. Async/Await Pattern

**What is it?**
Non-blocking asynchronous programming for I/O operations.

**Why use it?**

```csharp
// BAD (Blocking)
public User GetUser(int id)
{
    return _context.Users.Find(id);  // Blocks thread while waiting
}

// GOOD (Non-blocking)
public async Task<User> GetUserAsync(int id)
{
    return await _context.Users.FindAsync(id);  // Thread free to handle other requests
}
```

**Benefits:**
- Better scalability (handle more concurrent requests)
- Improved responsiveness
- Efficient resource usage

**Rule:** All database operations in ITAMS use async/await.

---

## Important Files for Review

### Backend Core Files
1. **Program.cs** - Application startup, DI configuration, middleware pipeline
2. **Controllers/AuthController.cs** - JWT authentication, login logic
3. **Controllers/UsersController.cs** - User CRUD operations
4. **Services/UserService.cs** - User business logic
5. **Data/ITAMSDbContext.cs** - EF Core DbContext, entity configurations
6. **Middleware/ProjectAccessControlMiddleware.cs** - Multi-tenancy enforcement

### Frontend Core Files
7. **itams-frontend/src/app/services/api.ts** - Centralized API service
8. **itams-frontend/src/app/services/auth.service.ts** - Authentication state management
9. **itams-frontend/src/app/guards/auth.guard.ts** - Route protection
10. **itams-frontend/src/app/users/users.ts** - Example component with CRUD

### Domain Models
11. **Domain/Entities/User.cs** - User entity
12. **Domain/Entities/Asset.cs** - Asset entity with enums
13. **Models/AssetDtos.cs** - DTO examples

### Documentation
14. **README.md** - Project overview
15. **docs/TECHNICAL_ARCHITECTURE.md** - Detailed architecture
16. **docs/SESSION_ARCHITECTURE.md** - Session management

---

## Key Features to Demonstrate

### 1. Authentication & Authorization
- JWT token-based authentication
- Role-based access control (RBAC)
- Session management (single session per user)
- Password hashing with BCrypt

### 2. Multi-Project Support
- Project isolation (multi-tenancy)
- Location-based access restrictions
- SuperAdmin can access all projects

### 3. Asset Management
- CRUD operations with validation
- Bulk upload via Excel (EPPlus library)
- Search and filter capabilities
- Asset lifecycle tracking

### 4. Audit Trail
- Login/logout tracking
- Session duration calculation
- Activity tracking middleware
- Comprehensive audit logs

### 5. Modern Architecture
- Layered architecture (separation of concerns)
- Repository pattern (data access abstraction)
- Service layer (business logic)
- DTO pattern (API contracts)
- Dependency injection (loose coupling)

---

## Common Interview Questions & Answers

### Q1: Why did you use JWT instead of session-based authentication?

**Answer:** JWT is stateless and scalable. The server doesn't need to store session data, making it ideal for distributed systems. The token contains all necessary information (userId, roleId) and is cryptographically signed to prevent tampering.

### Q2: Explain the difference between Entity and DTO.

**Answer:** 
- **Entity** (e.g., Asset.cs) represents the database table structure with all fields, relationships, and internal data.
- **DTO** (e.g., AssetDto.cs) is what we send to the frontend - only necessary fields, transformed data (enum to string), and computed fields. This provides security, flexibility, and better API design.

### Q3: How does your application handle concurrent users?

**Answer:** 
- Async/await for non-blocking I/O operations
- EF Core connection pooling
- Stateless JWT authentication (no server-side session storage)
- Middleware pipeline processes requests efficiently
- Database transactions for data consistency

### Q4: What design patterns did you use?

**Answer:**
- **Repository Pattern** - Data access abstraction
- **Service Layer Pattern** - Business logic separation
- **DTO Pattern** - API contract separation
- **Dependency Injection** - Loose coupling
- **Middleware Pattern** - Request processing pipeline
- **Factory Pattern** - Object creation (EF Core)

### Q5: How do you ensure data security?

**Answer:**
- JWT authentication with token expiration
- BCrypt password hashing (one-way, salted)
- Role-based access control (RBAC)
- Project isolation (multi-tenancy)
- Input validation (frontend + backend)
- Parameterized queries (SQL injection prevention)
- HTTPS for data in transit

### Q6: Explain your database design.

**Answer:**
- Normalized schema (3NF) to reduce redundancy
- Foreign keys for referential integrity
- Alternate keys (USR00001, AST00001) for business-friendly IDs
- Audit columns (CreatedAt, UpdatedAt, CreatedBy)
- Soft delete pattern (IsActive flag)
- Indexes on frequently queried columns

### Q7: How does the frontend communicate with the backend?

**Answer:**
- RESTful API with JSON payloads
- Angular HttpClient for HTTP requests
- RxJS Observables for async data streams
- Centralized API service for all endpoints
- JWT token in Authorization header
- Standardized response format (ApiResponse<T>)

### Q8: What is the purpose of middleware in your application?

**Answer:**
Middleware forms a pipeline that processes every HTTP request:
1. **CORS** - Allow frontend origin
2. **Authentication** - Validate JWT token
3. **Maintenance Mode** - Block requests during maintenance
4. **Activity Tracking** - Update LastActivityAt
5. **Project Access Control** - Extract user context (userId, projectId)
6. **Controllers** - Handle business logic

Each middleware can short-circuit the pipeline or pass to the next.

---

## Demo Flow Suggestions

### 1. Authentication Demo
1. Show login page
2. Enter credentials
3. Inspect JWT token in browser DevTools (Application → Local Storage)
4. Decode token at jwt.io to show claims
5. Show how token is sent in API requests (Network tab)

### 2. RBAC Demo
1. Login as SuperAdmin → Show all permissions
2. Login as IT Staff → Show limited permissions
3. Try to access restricted feature → Show error
4. Show permission check in code

### 3. Multi-Tenancy Demo
1. Login as SuperAdmin → See all projects
2. Login as regular user → See only assigned project
3. Show ProjectAccessControlMiddleware code
4. Show database query filtering by ProjectId

### 4. CRUD Operations Demo
1. Create a new asset
2. Show validation (frontend + backend)
3. Show database entry
4. Show audit log entry
5. Update asset
6. Show change tracking

### 5. Architecture Demo
1. Show layered architecture diagram
2. Trace a request from frontend to database
3. Show dependency injection in action
4. Show DTO transformation
5. Show EF Core query generation

---

## Technical Highlights

### Performance Optimizations
- Async/await for non-blocking I/O
- EF Core query optimization (Include, Select)
- Pagination for large datasets
- Caching for permissions (MemoryCache)
- Connection pooling

### Security Best Practices
- JWT with expiration
- BCrypt password hashing
- RBAC with granular permissions
- Input validation
- SQL injection prevention
- CORS configuration

### Code Quality
- SOLID principles
- Clean architecture
- Separation of concerns
- Interface-based design
- Comprehensive error handling
- Logging (Serilog)

### Modern Technologies
- .NET 10.0 (latest)
- Angular 20 (standalone components)
- Entity Framework Core 10.0
- SQL Server
- TypeScript 5.x

---

## Conclusion

ITAMS demonstrates a production-ready, enterprise-grade application with:
- Modern full-stack architecture
- Secure authentication and authorization
- Scalable multi-tenancy design
- Clean code following best practices
- Comprehensive feature set

**Key Takeaway:** This project showcases not just coding skills, but understanding of software architecture, design patterns, security, and real-world application development.

---

**Good luck with your review!** 🚀
