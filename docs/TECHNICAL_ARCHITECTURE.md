# ITAMS - Technical Architecture Documentation

## Table of Contents
1. [Technology Stack Overview](#technology-stack-overview)
2. [Frontend Architecture](#frontend-architecture)
3. [Backend Architecture](#backend-architecture)
4. [Design Patterns & Best Practices](#design-patterns--best-practices)
5. [Database Design](#database-design)
6. [Security Implementation](#security-implementation)
7. [Project Structure](#project-structure)

---

## Technology Stack Overview

### Frontend Technologies
- **Framework**: Angular 20.3.14 (Latest standalone components)
- **Language**: TypeScript 5.x
- **Styling**: Bootstrap 5 + Custom SCSS
- **Icons**: Font Awesome 6
- **HTTP Client**: Angular HttpClient (RxJS-based)
- **Build Tool**: Vite (Fast development server)
- **State Management**: Component-based (no external state library)

### Backend Technologies
- **Framework**: ASP.NET Core 10.0 (Latest .NET)
- **Language**: C# 12
- **ORM**: Entity Framework Core 10.0
- **Database**: SQL Server (supports shared/cloud databases)
- **Authentication**: JWT (JSON Web Tokens)
- **Password Hashing**: BCrypt.NET
- **Logging**: Built-in ASP.NET Core Logging

### Development Tools
- **Version Control**: Git + GitHub
- **Package Managers**: 
  - npm (Frontend)
  - NuGet (Backend)
- **Testing**: PowerShell scripts for database validation
- **API Testing**: Built-in .http files

---

## Frontend Architecture

### 1. Angular Standalone Components Architecture

**Why Standalone Components?**
- No need for NgModules (simpler, more modular)
- Better tree-shaking (smaller bundle sizes)
- Easier lazy loading
- More intuitive component organization

**Example Structure:**
```typescript
@Component({
  selector: 'app-users',
  imports: [CommonModule, FormsModule],  // Direct imports
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users implements OnInit {
  // Component logic
}
```

### 2. Component Organization

```
src/app/
├── assets/              # Asset management component
├── audit-trail/         # Audit logging UI
├── change-password/     # Password change functionality
├── dashboard/           # Main dashboard (SuperAdmin)
├── guards/              # Route guards (auth.guard.ts)
├── locations/           # Location management
├── login/               # Authentication UI
├── navigation/          # Top navigation bar
├── projects/            # Project management
├── roles/               # Role management
├── services/            # Shared services
│   ├── api.ts          # Centralized API service
│   ├── auth.service.ts # Authentication service
│   └── validation.service.ts
├── shared/              # Shared utilities
│   ├── constants.ts    # App-wide constants
│   └── validation.service.ts
├── user-dashboard/      # Regular user dashboard
├── user-permissions/    # Permission management
├── user-projects/       # User-project assignments
└── users/               # User management
```

### 3. Service Layer Pattern

**API Service (Centralized HTTP)**
```typescript
export class Api {
  private baseUrl = 'http://localhost:5066/api';
  
  // All HTTP calls go through this service
  getUsers(): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<User[]>>(`${this.baseUrl}/users`);
  }
}
```

**Benefits:**
- Single source of truth for API endpoints
- Easy to mock for testing
- Consistent error handling
- Type-safe responses

**Auth Service (State Management)**
```typescript
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  // Manages authentication state across the app
}
```

### 4. Reactive Programming with RxJS

**Observable Pattern:**
```typescript
this.api.getUsers().subscribe({
  next: (response) => {
    // Handle success
    this.users = response.data;
  },
  error: (error) => {
    // Handle error
    this.error = error.message;
  }
});
```

**Benefits:**
- Asynchronous data handling
- Automatic unsubscription (when using async pipe)
- Composable operators (map, filter, switchMap)

### 5. Template-Driven Forms

**Two-Way Data Binding:**
```html
<input [(ngModel)]="createForm.username" name="username">
```

**Why Template-Driven?**
- Simpler for basic forms
- Less boilerplate code
- Good for rapid development
- Easy validation

### 6. Component Communication

**Parent-Child via @Input/@Output:**
```typescript
// Not heavily used in this app (mostly standalone pages)
```

**Service-Based Communication:**
```typescript
// AuthService broadcasts user state
this.authService.currentUser$.subscribe(user => {
  this.currentUser = user;
});
```

### 7. Routing & Guards

**Route Configuration:**
```typescript
export const routes: Routes = [
  { path: 'login', component: Login },
  { 
    path: 'dashboard', 
    component: Dashboard,
    canActivate: [authGuard]  // Protected route
  }
];
```

**Auth Guard (Functional Guard):**
```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  if (authService.isAuthenticated()) {
    return true;
  }
  
  router.navigate(['/login']);
  return false;
};
```

### 8. Styling Architecture

**Bootstrap 5 + Custom SCSS:**
- Bootstrap for layout (grid, utilities)
- Custom SCSS for component-specific styles
- No scrolling design (fixed heights, overflow-y: auto)
- Consistent color scheme (#f8fafc backgrounds)

**Example:**
```scss
.modal-content {
  background: #f8fafc;
  max-height: 85vh;
  overflow-y: auto;
}
```

---

## Backend Architecture

### 1. ASP.NET Core Web API Architecture

**Program.cs (Minimal Hosting Model):**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Service registration
builder.Services.AddControllers();
builder.Services.AddDbContext<ITAMSDbContext>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Middleware pipeline
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. Layered Architecture Pattern

```
ITAMS/
├── Controllers/          # API Endpoints (Presentation Layer)
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── SuperAdminController.cs
│   └── RbacController.cs
├── Services/            # Business Logic Layer
│   ├── UserService.cs
│   ├── ProjectService.cs
│   ├── LocationService.cs
│   └── RBAC/
│       ├── PermissionResolver.cs
│       └── RoleManagementService.cs
├── Domain/              # Domain Layer
│   ├── Entities/       # Domain Models
│   │   ├── User.cs
│   │   ├── Project.cs
│   │   └── RBAC/
│   └── Interfaces/     # Service Contracts
│       ├── IUserService.cs
│       └── IUserRepository.cs
├── Data/               # Data Access Layer
│   ├── ITAMSDbContext.cs
│   └── Repositories/
│       ├── UserRepository.cs
│       └── ProjectRepository.cs
├── Models/             # DTOs (Data Transfer Objects)
│   ├── UserDtos.cs
│   └── ProjectDtos.cs
└── Middleware/         # Custom Middleware
    ├── ActivityTrackingMiddleware.cs
    └── ProjectAccessControlMiddleware.cs
```

### 3. Repository Pattern

**Interface (Contract):**
```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(int id);
}
```

**Implementation:**
```csharp
public class UserRepository : IUserRepository
{
    private readonly ITAMSDbContext _context;
    
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
```

**Benefits:**
- Separation of concerns
- Easy to test (mock repositories)
- Database-agnostic business logic
- Centralized data access

### 4. Service Layer Pattern

**Service Interface:**
```csharp
public interface IUserService
{
    Task<User?> AuthenticateAsync(string username, string password);
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User> UpdateUserAsync(int id, UpdateUserRequest request);
}
```

**Service Implementation:**
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;
    
    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditService = auditService;
    }
    
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Business logic here
        // - Validation
        // - Password hashing
        // - Alternate key generation
        // - Audit logging
    }
}
```

**Benefits:**
- Business logic separated from controllers
- Reusable across multiple controllers
- Transaction management
- Validation and error handling

### 5. Dependency Injection (DI)

**Service Registration:**
```csharp
// Program.cs
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
```

**Constructor Injection:**
```csharp
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
}
```

**Benefits:**
- Loose coupling
- Easy testing (inject mocks)
- Lifecycle management (Scoped, Transient, Singleton)
- Automatic disposal

### 6. Entity Framework Core (ORM)

**DbContext:**
```csharp
public class ITAMSDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Location> Locations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API configuration
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);
    }
}
```

**Query Examples:**
```csharp
// LINQ to Entities
var users = await _context.Users
    .Include(u => u.Role)
    .Where(u => u.IsActive)
    .OrderBy(u => u.Username)
    .ToListAsync();
```

**Benefits:**
- Type-safe queries
- Automatic SQL generation
- Change tracking
- Migration support
- Lazy/Eager loading

### 7. DTO Pattern (Data Transfer Objects)

**Why DTOs?**
- Separate internal models from API contracts
- Control what data is exposed
- Validation attributes
- Version API without changing domain models

**Example:**
```csharp
public class CreateUserDto
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public int RoleId { get; set; }
    
    public int? ProjectId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string RoleName { get; set; }
    public bool IsActive { get; set; }
    // Password hash NOT included
}
```

### 8. Middleware Pattern

**Custom Middleware:**
```csharp
public class ActivityTrackingMiddleware
{
    private readonly RequestDelegate _next;
    
    public ActivityTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, ITAMSDbContext dbContext)
    {
        // Pre-processing
        var userId = GetUserIdFromToken(context);
        
        // Call next middleware
        await _next(context);
        
        // Post-processing
        if (userId.HasValue)
        {
            await UpdateLastActivity(userId.Value, dbContext);
        }
    }
}
```

**Registration:**
```csharp
app.UseMiddleware<ActivityTrackingMiddleware>();
```

### 9. API Response Pattern

**Standardized Response:**
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
```

**Controller Usage:**
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAllUsers()
{
    try
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(new ApiResponse<IEnumerable<UserDto>>
        {
            Success = true,
            Data = users,
            Message = "Users retrieved successfully"
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new ApiResponse<IEnumerable<UserDto>>
        {
            Success = false,
            Error = ex.Message
        });
    }
}
```

### 10. Authentication & Authorization

**JWT Token Generation:**
```csharp
private string GenerateJwtToken(User user, string sessionId)
{
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
            new Claim("SessionId", sessionId)
        }),
        Expires = DateTime.UtcNow.AddMinutes(30),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key), 
            SecurityAlgorithms.HmacSha256Signature)
    };
    
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

**Password Hashing (BCrypt):**
```csharp
// Hash password
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

// Verify password
bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

---

## Design Patterns & Best Practices

### 1. SOLID Principles

**S - Single Responsibility Principle:**
- Each class has one reason to change
- UserService handles user logic
- UserRepository handles data access
- AuthController handles HTTP requests

**O - Open/Closed Principle:**
- Services are open for extension (inheritance)
- Closed for modification (interfaces)

**L - Liskov Substitution Principle:**
- Any IUserService implementation can replace UserService

**I - Interface Segregation:**
- Small, focused interfaces (IUserRepository, IUserService)
- Clients don't depend on methods they don't use

**D - Dependency Inversion:**
- High-level modules depend on abstractions (interfaces)
- Not on concrete implementations

### 2. Async/Await Pattern

**Why Async?**
- Non-blocking I/O operations
- Better scalability
- Improved responsiveness

**Example:**
```csharp
public async Task<User> CreateUserAsync(CreateUserRequest request)
{
    // Async database operations
    var user = await _userRepository.CreateAsync(newUser);
    await _auditService.LogAsync("USER_CREATED", "User", user.Id.ToString());
    return user;
}
```

### 3. Exception Handling

**Service Layer:**
```csharp
if (await _userRepository.UsernameExistsAsync(username))
{
    throw new InvalidOperationException("Username already exists");
}
```

**Controller Layer:**
```csharp
try
{
    var user = await _userService.CreateUserAsync(request);
    return Ok(new ApiResponse<User> { Success = true, Data = user });
}
catch (InvalidOperationException ex)
{
    return BadRequest(new ApiResponse<User> 
    { 
        Success = false, 
        Error = ex.Message 
    });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating user");
    return StatusCode(500, new ApiResponse<User> 
    { 
        Success = false, 
        Error = "An error occurred" 
    });
}
```

### 4. Logging Pattern

```csharp
_logger.LogInformation("User {Username} logged in successfully", username);
_logger.LogWarning("Failed login attempt for user {Username}", username);
_logger.LogError(ex, "Error creating user {Username}", username);
```

### 5. Validation Pattern

**Data Annotations:**
```csharp
public class CreateUserDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

**Custom Validation:**
```csharp
public async Task<bool> ValidatePasswordPolicyAsync(string password)
{
    if (password.Length < 8) return false;
    if (!password.Any(char.IsUpper)) return false;
    if (!password.Any(char.IsLower)) return false;
    if (!password.Any(char.IsDigit)) return false;
    if (!Regex.IsMatch(password, @"[@$!%*?&]")) return false;
    return true;
}
```

---

## Database Design

### 1. Entity Relationships

**One-to-Many:**
```
Role (1) ----< (Many) User
Project (1) ----< (Many) Location
Project (1) ----< (Many) User
```

**Many-to-Many:**
```
User >----< Project (via UserProjects)
Role >----< Permission (via RolePermissions)
```

### 2. Alternate Keys Pattern

**Why Alternate Keys?**
- Human-readable identifiers
- Business-friendly references
- Migration support

**Examples:**
- Users: USR00001, USR00002
- Projects: PRJ00001, PRJ00002
- Locations: LOC00001, LOC00002
- Roles: ROLE00001, ROLE00002

**Implementation:**
```csharp
// Generate alternate key
var lastUser = await _userRepository.GetAllAsync();
var maxId = lastUser.Any() ? lastUser.Max(u => u.Id) : 0;
var userId = $"USR{(maxId + 1):D5}";
```

### 3. Audit Trail Pattern

**Audit Columns:**
```csharp
public class User
{
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
}
```

**Audit Log Table:**
```csharp
public class AuditEntry
{
    public int Id { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public DateTime Timestamp { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
```

### 4. Soft Delete Pattern

```csharp
public class User
{
    public bool IsActive { get; set; } = true;
}

// Instead of deleting
await _userRepository.DeleteAsync(id);

// Deactivate
user.IsActive = false;
await _userRepository.UpdateAsync(user);
```

---

## Security Implementation

### 1. Authentication Flow

1. User submits credentials
2. Backend validates username/password
3. Check for active session (prevent concurrent logins)
4. Generate JWT token with claims
5. Return token + user data
6. Frontend stores token in localStorage
7. Include token in Authorization header for subsequent requests

### 2. Session Management

**Single Session Enforcement:**
```csharp
// Check for active session
if (!string.IsNullOrEmpty(user.ActiveSessionId) && user.SessionStartedAt.HasValue)
{
    var lastActivity = user.LastActivityAt ?? user.SessionStartedAt.Value;
    var timeSinceActivity = DateTime.UtcNow - lastActivity;
    
    if (timeSinceActivity.TotalMinutes < 30)
    {
        return Unauthorized("Account is logged in elsewhere");
    }
}

// Create new session
var sessionId = Guid.NewGuid().ToString();
await _userService.UpdateSessionAsync(user.Id, sessionId, DateTime.UtcNow);
```

### 3. Activity Tracking

**Middleware updates LastActivityAt on every request:**
```csharp
public async Task InvokeAsync(HttpContext context, ITAMSDbContext dbContext)
{
    await _next(context);
    
    var userId = GetUserIdFromToken(context);
    if (userId.HasValue)
    {
        var user = await dbContext.Users.FindAsync(userId.Value);
        if (user != null)
        {
            user.LastActivityAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }
}
```

### 4. Role-Based Access Control (RBAC)

**Permission Check:**
```csharp
public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
{
    var user = await _userRepository.GetByIdAsync(userId);
    var permissions = await GetUserPermissionsAsync(userId);
    return permissions.Any(p => p.Code == permissionCode && p.IsGranted);
}
```

### 5. Project-Based Access Control

**Hierarchical Access:**
```
SuperAdmin (RoleId = 1) → Full access to all projects
Other Roles → Access only to assigned project
```

**Location Restrictions:**
```
User.RestrictedOffice → Most specific
User.RestrictedPlaza
User.RestrictedState
User.RestrictedRegion → Least specific
NULL → Full access within project
```

---

## Project Structure

### Frontend Structure
```
itams-frontend/
├── src/
│   ├── app/
│   │   ├── [feature-folders]/
│   │   ├── services/
│   │   ├── guards/
│   │   └── shared/
│   ├── styles.scss
│   └── main.ts
├── angular.json
├── package.json
└── tsconfig.json
```

### Backend Structure
```
ITAMS/
├── Controllers/
├── Services/
├── Domain/
│   ├── Entities/
│   └── Interfaces/
├── Data/
│   └── Repositories/
├── Models/
├── Middleware/
├── Migrations/
├── docs/
├── scripts/
├── Program.cs
└── ITAMS.csproj
```

---

## Summary

**Frontend:** Modern Angular with standalone components, reactive programming, and service-based architecture.

**Backend:** Clean architecture with ASP.NET Core, following SOLID principles, using Repository and Service patterns.

**Database:** SQL Server with EF Core, supporting alternate keys, audit trails, and soft deletes.

**Security:** JWT authentication, BCrypt password hashing, session management, and RBAC.

**Patterns:** DI, Repository, Service Layer, DTO, Middleware, Async/Await, and more.

This architecture provides a scalable, maintainable, and secure foundation for the ITAMS application.
