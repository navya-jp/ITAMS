# ITAMS - Complete Application Flow Documentation

## Table of Contents
1. [Login Flow](#1-login-flow)
2. [Create User Flow](#2-create-user-flow)
3. [Load Users List Flow](#3-load-users-list-flow)
4. [Create Location Flow](#4-create-location-flow)
5. [Session Management Flow](#5-session-management-flow)
6. [Activity Tracking Flow](#6-activity-tracking-flow)

---

## 1. Login Flow

### Step-by-Step Journey

#### **FRONTEND: User Clicks "Login" Button**

**File:** `itams-frontend/src/app/login/login.html`
```html
<button (click)="onLogin()" class="btn btn-primary">
  Login
</button>
```

**File:** `itams-frontend/src/app/login/login.ts`
```typescript
onLogin() {
  // 1. Validate form
  if (!this.username || !this.password) {
    this.error = 'Please enter username and password';
    return;
  }
  
  // 2. Call AuthService
  this.loading = true;
  this.authService.login(this.username, this.password).subscribe({
    next: (response) => {
      // Success - navigate to dashboard
      this.router.navigate(['/dashboard']);
    },
    error: (error) => {
      // Show error message
      this.error = error.error?.message || 'Login failed';
      this.loading = false;
    }
  });
}
```

#### **FRONTEND: AuthService Processes Login**

**File:** `itams-frontend/src/app/services/auth.service.ts`
```typescript
login(username: string, password: string): Observable<LoginResponse> {
  // 3. Make HTTP POST request to backend
  return this.http.post<LoginResponse>(
    `${this.baseUrl}/auth/login`,
    { username, password }
  ).pipe(
    tap(response => {
      if (response.success && response.token) {
        // 4. Store token in localStorage
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        
        // 5. Update current user state
        this.currentUserSubject.next(response.user);
      }
    })
  );
}
```

#### **NETWORK: HTTP Request Sent**

```
POST http://localhost:5066/api/auth/login
Content-Type: application/json

{
  "username": "superadmin",
  "password": "Admin@123"
}
```

#### **BACKEND: Request Hits AuthController**

**File:** `Controllers/AuthController.cs`
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // 6. Log the login attempt
    _logger.LogInformation("Login attempt for user: {Username}", request.Username);
    
    // 7. Get user from database
    var users = await _userService.GetAllUsersAsync();
    var user = users.FirstOrDefault(u => 
        u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) && 
        u.IsActive);
    
    if (user == null)
    {
        return Unauthorized(new LoginResponse
        {
            Success = false,
            Message = "Invalid username or password"
        });
    }
```

    // 8. Check for active session (prevent concurrent logins)
    if (!string.IsNullOrEmpty(user.ActiveSessionId) && user.SessionStartedAt.HasValue)
    {
        var lastActivity = user.LastActivityAt ?? user.SessionStartedAt.Value;
        var timeSinceActivity = DateTime.UtcNow - lastActivity;
        
        if (timeSinceActivity.TotalMinutes < 30)
        {
            _logger.LogWarning("BLOCKING LOGIN: User {Username} has active session", 
                request.Username);
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "Account is logged in from another location"
            });
        }
    }
    
    // 9. Authenticate user (verify password)
    var authenticatedUser = await _userService.AuthenticateAsync(
        request.Username, 
        request.Password
    );
    
    if (authenticatedUser == null)
    {
        return Unauthorized(new LoginResponse
        {
            Success = false,
            Message = "Invalid username or password"
        });
    }
```

#### **BACKEND: UserService Authenticates**

**File:** `Services/UserService.cs`
```csharp
public async Task<User?> AuthenticateAsync(string username, string password)
{
    // 10. Get user from repository
    var user = await _userRepository.GetByUsernameAsync(username);
    
    if (user == null || !user.IsActive)
    {
        return null;
    }
    
    // 11. Check if user is locked
    if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
    {
        return null;
    }
    
    // 12. Verify password using BCrypt
    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    
    if (!isPasswordValid)
    {
        // 13. Increment failed login attempts
        user.FailedLoginAttempts++;
        
        // Lock user after 5 failed attempts
        if (user.FailedLoginAttempts >= 5)
        {
            user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
        
        await _userRepository.UpdateAsync(user);
        return null;
    }
    
    // 14. Reset failed attempts and update last login
    user.FailedLoginAttempts = 0;
    user.LockedUntil = null;
    user.LastLoginAt = DateTime.UtcNow;
    
    await _userRepository.UpdateAsync(user);
    
    // 15. Log successful authentication
    await _auditService.LogAsync("LOGIN_SUCCESS", "User", user.Id.ToString(), 
        user.Id, user.Username);
    
    return user;
}
```

#### **DATABASE: SQL Queries Executed**

```sql
-- Query 1: Get user by username
SELECT u.*, r.*
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.Id
WHERE u.Username = 'superadmin' AND u.IsActive = 1

-- Query 2: Update user login info
UPDATE Users
SET FailedLoginAttempts = 0,
    LockedUntil = NULL,
    LastLoginAt = '2026-02-16 11:30:00'
WHERE Id = 1

-- Query 3: Insert audit log
INSERT INTO AuditEntries (Action, EntityType, EntityId, UserId, Username, Timestamp)
VALUES ('LOGIN_SUCCESS', 'User', '1', 1, 'superadmin', '2026-02-16 11:30:00')
```

#### **BACKEND: Generate JWT Token**

**File:** `Controllers/AuthController.cs`
```csharp
    // 16. Generate session ID
    var sessionId = Guid.NewGuid().ToString();
    await _userService.UpdateSessionAsync(authenticatedUser.Id, sessionId, DateTime.UtcNow);
    
    // 17. Create login audit record
    var loginAudit = new LoginAudit
    {
        UserId = authenticatedUser.Id,
        Username = authenticatedUser.Username,
        LoginTime = DateTime.UtcNow,
        IpAddress = GetClientIpAddress(),
        BrowserType = ExtractBrowserType(userAgent),
        OperatingSystem = ExtractOperatingSystem(userAgent),
        SessionId = sessionId,
        Status = "ACTIVE"
    };
    _context.LoginAudits.Add(loginAudit);
    await _context.SaveChangesAsync();
    
    // 18. Generate JWT token
    var token = GenerateJwtToken(authenticatedUser, sessionId);
    
    // 19. Return response
    return Ok(new LoginResponse
    {
        Success = true,
        Token = token,
        User = new AuthUserDto
        {
            Id = authenticatedUser.Id,
            Username = authenticatedUser.Username,
            Email = authenticatedUser.Email,
            RoleId = authenticatedUser.RoleId,
            RoleName = authenticatedUser.Role?.Name ?? "User",
            SessionId = sessionId
        }
    });
}

private string GenerateJwtToken(User user, string sessionId)
{
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
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

#### **NETWORK: HTTP Response Sent**

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "superadmin",
    "email": "admin@itams.com",
    "roleId": 1,
    "roleName": "Super Admin",
    "sessionId": "c97f89d1-57bc-47d1-819b-f48200894f6f"
  }
}
```

#### **FRONTEND: Process Response**

**File:** `itams-frontend/src/app/services/auth.service.ts`
```typescript
// 20. Store token and user data
localStorage.setItem('token', response.token);
localStorage.setItem('user', JSON.stringify(response.user));

// 21. Update observable state
this.currentUserSubject.next(response.user);

// 22. Navigate to dashboard
this.router.navigate(['/dashboard']);
```

#### **FRONTEND: Dashboard Loads**

**File:** `itams-frontend/src/app/dashboard/dashboard.ts`
```typescript
ngOnInit() {
  // 23. Subscribe to current user
  this.authService.currentUser$.subscribe(user => {
    if (user) {
      this.currentUser = user;
      this.loadDashboardData();
    }
  });
}
```

### Login Flow Summary

```
User clicks Login
    ↓
Frontend validates input
    ↓
AuthService makes HTTP POST to /api/auth/login
    ↓
Backend AuthController receives request
    ↓
Check for active session (session management)
    ↓
UserService authenticates (verify password with BCrypt)
    ↓
UserRepository queries database
    ↓
Update user login info in database
    ↓
Generate JWT token with claims
    ↓
Create login audit record
    ↓
Return token + user data
    ↓
Frontend stores token in localStorage
    ↓
Update AuthService state (BehaviorSubject)
    ↓
Navigate to dashboard
    ↓
Dashboard loads with user context
```

---

## 2. Create User Flow

### Step-by-Step Journey

#### **FRONTEND: User Clicks "Create User" Button**

**File:** `itams-frontend/src/app/users/users.html`
```html
<button (click)="onCreateUserClick()" class="btn btn-primary">
  Create User
</button>
```

**File:** `itams-frontend/src/app/users/users.ts`
```typescript
onCreateUserClick() {
  // 1. Validate form
  if (!this.createForm.firstName || !this.createForm.lastName || 
      !this.createForm.username || !this.createForm.email || 
      !this.createForm.password || this.createForm.roleId === 0) {
    this.error = 'Please fill in all required fields';
    return;
  }
  
  // 2. Convert form values to numbers
  this.createForm.roleId = Number(this.createForm.roleId);
  if (this.createForm.projectId) {
    this.createForm.projectId = Number(this.createForm.projectId);
  }
  
  console.log('Creating user with data:', JSON.stringify(this.createForm));
  
  // 3. Call createUser method
  this.createUser();
}

createUser() {
  this.loading = true;
  this.error = '';
  
  // 4. Make HTTP request via API service
  this.api.createUser(this.createForm).subscribe({
    next: (response: ApiResponse<User>) => {
      if (response.success && response.data) {
        // 5. Add new user to local array
        this.users.push(response.data);
        this.success = response.message || 'User created successfully';
        this.loading = false;
        this.closeModals();
      }
    },
    error: (error) => {
      console.error('Create user error:', error);
      this.error = error.error?.message || 'Failed to create user';
      this.loading = false;
    }
  });
}
```

#### **FRONTEND: API Service Makes Request**

**File:** `itams-frontend/src/app/services/api.ts`
```typescript
createUser(user: CreateUser): Observable<ApiResponse<User>> {
  // 6. Add Authorization header with JWT token
  const token = localStorage.getItem('token');
  const headers = new HttpHeaders({
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  });
  
  // 7. Make HTTP POST request
  return this.http.post<ApiResponse<User>>(
    `${this.baseUrl}/users`,
    user,
    { headers }
  );
}
```

#### **NETWORK: HTTP Request Sent**

```
POST http://localhost:5066/api/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "username": "john.doe",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roleId": 3,
  "password": "SecurePass@123",
  "mustChangePassword": true,
  "projectId": 1,
  "restrictedRegion": "",
  "restrictedState": "",
  "restrictedPlaza": "",
  "restrictedOffice": ""
}
```

#### **BACKEND: Middleware Pipeline**

```csharp
// 8. CORS Middleware - Allow cross-origin requests
app.UseCors();

// 9. Authentication Middleware - Validate JWT token
app.UseAuthentication();

// 10. Authorization Middleware - Check user permissions
app.UseAuthorization();

// 11. Activity Tracking Middleware - Update LastActivityAt
app.UseMiddleware<ActivityTrackingMiddleware>();
```

**File:** `Middleware/ActivityTrackingMiddleware.cs`
```csharp
public async Task InvokeAsync(HttpContext context, ITAMSDbContext dbContext)
{
    // 12. Extract user ID from JWT token
    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
    
    // 13. Call next middleware (controller)
    await _next(context);
    
    // 14. Update user activity after request completes
    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastActivityAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }
}
```

#### **BACKEND: Request Hits UsersController**

**File:** `Controllers/UsersController.cs`
```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
    [FromBody] CreateUserDto createUserDto)
{
    try
    {
        // 15. Log the request
        _logger.LogInformation("Creating user: {Username}", createUserDto.Username);
        
        // 16. Map DTO to service request
        var request = new CreateUserRequest
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            RoleId = createUserDto.RoleId,
            Password = createUserDto.Password,
            MustChangePassword = createUserDto.MustChangePassword,
            ProjectId = createUserDto.ProjectId,
            RestrictedRegion = createUserDto.RestrictedRegion,
            RestrictedState = createUserDto.RestrictedState,
            RestrictedPlaza = createUserDto.RestrictedPlaza,
            RestrictedOffice = createUserDto.RestrictedOffice
        };
        
        // 17. Call UserService
        var user = await _userService.CreateUserAsync(request);
```

        // 18. Map entity to DTO for response
        var userDto = new UserDto
        {
            Id = user.Id,
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? "Unknown",
            IsActive = user.IsActive,
            ProjectId = user.ProjectId
        };
        
        // 19. Return success response
        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Data = userDto,
            Message = "User created successfully"
        });
    }
    catch (InvalidOperationException ex)
    {
        // 20. Handle business logic errors
        return BadRequest(new ApiResponse<UserDto>
        {
            Success = false,
            Message = ex.Message
        });
    }
    catch (Exception ex)
    {
        // 21. Handle unexpected errors
        _logger.LogError(ex, "Error creating user");
        return StatusCode(500, new ApiResponse<UserDto>
        {
            Success = false,
            Message = "An error occurred while creating the user",
            Error = ex.Message
        });
    }
}
```

#### **BACKEND: UserService Processes Request**

**File:** `Services/UserService.cs`
```csharp
public async Task<User> CreateUserAsync(CreateUserRequest request)
{
    // 22. Validate username uniqueness
    if (await _userRepository.UsernameExistsAsync(request.Username))
    {
        throw new InvalidOperationException("Username already exists");
    }
    
    // 23. Validate email uniqueness
    if (await _userRepository.EmailExistsAsync(request.Email))
    {
        throw new InvalidOperationException("Email already exists");
    }
    
    // 24. Validate password policy
    if (!await ValidatePasswordPolicyAsync(request.Password))
    {
        throw new InvalidOperationException(
            "Password must be at least 8 characters with uppercase, lowercase, number, and special character"
        );
    }
    
    // 25. Validate role exists
    if (!await _roleRepository.ExistsAsync(request.RoleId))
    {
        throw new InvalidOperationException("Invalid role specified");
    }
    
    // 26. Get role to fetch RoleId alternate key
    var role = await _roleRepository.GetByIdAsync(request.RoleId);
    
    // 27. Generate UserId alternate key (USR00001, USR00002, etc.)
    var lastUser = await _userRepository.GetAllAsync();
    var maxId = lastUser.Any() ? lastUser.Max(u => u.Id) : 0;
    var userId = $"USR{(maxId + 1):D5}";
    
    // 28. Use projectId if provided, otherwise default to 1
    int finalProjectId = request.ProjectId ?? 1;
    
    // 29. Create user entity
    var user = new User
    {
        UserId = userId,
        Username = request.Username,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        RoleId = request.RoleId,
        RoleIdRef = role.RoleId,  // Alternate key reference
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        MustChangePassword = request.MustChangePassword,
        CreatedAt = DateTime.UtcNow,
        IsActive = true,
        ProjectId = finalProjectId,
        RestrictedRegion = request.RestrictedRegion,
        RestrictedState = request.RestrictedState,
        RestrictedPlaza = request.RestrictedPlaza,
        RestrictedOffice = request.RestrictedOffice
    };
    
    // 30. Save to database via repository
    var createdUser = await _userRepository.CreateAsync(user);
    
    // 31. Log audit trail
    await _auditService.LogAsync("USER_CREATED", "User", 
        createdUser.Id.ToString(), createdUser.Id, createdUser.Username);
    
    // 32. Reload user with navigation properties
    return await _userRepository.GetByIdAsync(createdUser.Id) ?? createdUser;
}
```

#### **DATABASE: Multiple SQL Queries**

```sql
-- Query 1: Check username exists
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM Users WHERE Username = 'john.doe'
) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END

-- Query 2: Check email exists
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM Users WHERE Email = 'john@example.com'
) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END

-- Query 3: Check role exists
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM Roles WHERE Id = 3
) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END

-- Query 4: Get role details
SELECT r.*, rp.*, p.*
FROM Roles r
LEFT JOIN RolePermissions rp ON r.Id = rp.RoleId
LEFT JOIN Permissions p ON rp.PermissionId = p.Id
WHERE r.Id = 3

-- Query 5: Get all users (to calculate next UserId)
SELECT Id FROM Users

-- Query 6: Insert new user
INSERT INTO Users (
    UserId, Username, Email, FirstName, LastName, 
    RoleId, RoleIdRef, PasswordHash, MustChangePassword, 
    CreatedAt, IsActive, ProjectId, 
    RestrictedRegion, RestrictedState, RestrictedPlaza, RestrictedOffice
)
OUTPUT INSERTED.Id
VALUES (
    'USR00021', 'john.doe', 'john@example.com', 'John', 'Doe',
    3, 'ROLE00003', '$2a$11$...', 1,
    '2026-02-16 11:35:00', 1, 1,
    NULL, NULL, NULL, NULL
)

-- Query 7: Insert audit log
INSERT INTO AuditEntries (
    Action, EntityType, EntityId, UserId, Username, Timestamp
)
VALUES (
    'USER_CREATED', 'User', '21', 21, 'john.doe', '2026-02-16 11:35:00'
)

-- Query 8: Reload user with role
SELECT u.*, r.*
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.Id
WHERE u.Id = 21

-- Query 9: Update activity (from middleware)
UPDATE Users
SET LastActivityAt = '2026-02-16 11:35:00'
WHERE Id = 1  -- Current logged-in user
```

#### **NETWORK: HTTP Response Sent**

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "id": 21,
    "userId": "USR00021",
    "username": "john.doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roleId": 3,
    "roleName": "Project Manager",
    "isActive": true,
    "projectId": 1,
    "lastLoginAt": null
  },
  "message": "User created successfully"
}
```

#### **FRONTEND: Process Response**

**File:** `itams-frontend/src/app/users/users.ts`
```typescript
next: (response: ApiResponse<User>) => {
  if (response.success && response.data) {
    // 33. Add new user to local array (updates UI immediately)
    this.users.push(response.data);
    
    // 34. Apply filters to update filtered list
    this.applyFilters();
    
    // 35. Show success message
    this.success = response.message || 'User created successfully';
    
    // 36. Close modal
    this.loading = false;
    this.closeModals();
  }
}
```

#### **FRONTEND: UI Updates**

```html
<!-- User appears in table immediately -->
<tr *ngFor="let user of filteredUsers">
  <td>
    <div class="d-flex align-items-center">
      <div class="online-status-indicator me-2"></div>
      <strong>john.doe</strong>
    </div>
  </td>
  <td>John Doe</td>
  <td>john@example.com</td>
  <td><span class="badge bg-info">Project Manager</span></td>
  <td><span class="badge bg-success">Active</span></td>
  <td><!-- Action buttons --></td>
</tr>
```

### Create User Flow Summary

```
User fills form and clicks "Create User"
    ↓
Frontend validates input
    ↓
API service adds JWT token to request headers
    ↓
HTTP POST to /api/users
    ↓
Backend middleware pipeline:
  - CORS
  - Authentication (validate JWT)
  - Authorization
  - Activity Tracking (update LastActivityAt)
    ↓
UsersController receives request
    ↓
Maps DTO to service request
    ↓
UserService validates:
  - Username uniqueness
  - Email uniqueness
  - Password policy
  - Role exists
    ↓
Generate UserId alternate key (USR00021)
    ↓
Hash password with BCrypt
    ↓
UserRepository inserts into database
    ↓
AuditService logs USER_CREATED
    ↓
Reload user with navigation properties
    ↓
Map entity to DTO
    ↓
Return success response
    ↓
Frontend adds user to local array
    ↓
UI updates immediately (Angular change detection)
    ↓
Show success message and close modal
```

---

## 3. Load Users List Flow

### Step-by-Step Journey

#### **FRONTEND: Component Initializes**

**File:** `itams-frontend/src/app/users/users.ts`
```typescript
ngOnInit() {
  // 1. Get current user from AuthService
  this.authService.currentUser$.subscribe(user => {
    if (user) {
      this.currentUsername = user.username;
    }
  });
  
  // 2. Load users list
  this.loadUsers();
  
  // 3. Load roles for dropdown
  this.loadRoles();
  
  // 4. Load projects for dropdown
  this.loadProjects();
}

loadUsers() {
  this.loading = true;
  
  // 5. Call API service
  this.api.getUsers().subscribe({
    next: (response: ApiResponse<User[]>) => {
      if (response.success && response.data) {
        // 6. Store users in component
        this.users = response.data;
        
        // 7. Apply filters (initially shows all)
        this.applyFilters();
      }
      this.loading = false;
    },
    error: (error) => {
      this.error = 'Failed to load users';
      this.loading = false;
    }
  });
}
```

#### **FRONTEND: API Service Makes Request**

**File:** `itams-frontend/src/app/services/api.ts`
```typescript
getUsers(): Observable<ApiResponse<User[]>> {
  // 8. Get token from localStorage
  const token = localStorage.getItem('token');
  
  // 9. Add Authorization header
  const headers = new HttpHeaders({
    'Authorization': `Bearer ${token}`
  });
  
  // 10. Make HTTP GET request
  return this.http.get<ApiResponse<User[]>>(
    `${this.baseUrl}/users`,
    { headers }
  );
}
```

#### **NETWORK: HTTP Request**

```
GET http://localhost:5066/api/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### **BACKEND: UsersController**

**File:** `Controllers/UsersController.cs`
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAllUsers()
{
    try
    {
        // 11. Get current user ID from JWT claims
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        
        // 12. Call UserService
        var users = await _userService.GetAllUsersAsync();
        
        // 13. Map to DTOs
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            RoleId = u.RoleId,
            RoleName = u.Role?.Name ?? "Unknown",
            IsActive = u.IsActive,
            IsLocked = u.LockedUntil.HasValue && u.LockedUntil > DateTime.UtcNow,
            MustChangePassword = u.MustChangePassword,
            LastLoginAt = u.LastLoginAt,
            ProjectId = u.ProjectId
        });
        
        // 14. Return response
        return Ok(new ApiResponse<IEnumerable<UserDto>>
        {
            Success = true,
            Data = userDtos,
            Message = "Users retrieved successfully"
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving users");
        return StatusCode(500, new ApiResponse<IEnumerable<UserDto>>
        {
            Success = false,
            Message = "An error occurred while retrieving users"
        });
    }
}
```

#### **BACKEND: UserService**

**File:** `Services/UserService.cs`
```csharp
public async Task<IEnumerable<User>> GetAllUsersAsync()
{
    // 15. Call repository
    return await _userRepository.GetAllAsync();
}
```

#### **BACKEND: UserRepository**

**File:** `Data/Repositories/UserRepository.cs`
```csharp
public async Task<IEnumerable<User>> GetAllAsync()
{
    // 16. Query database with EF Core
    return await _context.Users
        .Include(u => u.Role)  // Eager load role
        .OrderBy(u => u.Username)
        .ToListAsync();
}
```

#### **DATABASE: SQL Query**

```sql
SELECT u.*, r.*
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.Id
ORDER BY u.Username
```

#### **NETWORK: HTTP Response**

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": [
    {
      "id": 1,
      "userId": "USR00001",
      "username": "superadmin",
      "email": "admin@itams.com",
      "firstName": "Super",
      "lastName": "Admin",
      "roleId": 1,
      "roleName": "Super Admin",
      "isActive": true,
      "isLocked": false,
      "mustChangePassword": false,
      "lastLoginAt": "2026-02-16T11:30:00",
      "projectId": 1
    },
    {
      "id": 21,
      "userId": "USR00021",
      "username": "john.doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roleId": 3,
      "roleName": "Project Manager",
      "isActive": true,
      "isLocked": false,
      "mustChangePassword": true,
      "lastLoginAt": null,
      "projectId": 1
    }
  ],
  "message": "Users retrieved successfully"
}
```

#### **FRONTEND: Process and Display**

**File:** `itams-frontend/src/app/users/users.ts`
```typescript
next: (response: ApiResponse<User[]>) => {
  if (response.success && response.data) {
    // 17. Store users
    this.users = response.data;
    
    // 18. Apply filters
    this.applyFilters();
  }
  this.loading = false;
}

applyFilters() {
  // 19. Filter users based on search, role, and status
  this.filteredUsers = this.users.filter(user => {
    const matchesSearch = !this.searchTerm || 
      user.username.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      user.email.toLowerCase().includes(this.searchTerm.toLowerCase());
    
    const matchesRole = this.filterRole === 0 || user.roleId === this.filterRole;
    
    const matchesStatus = this.filterStatus === 'all' ||
      (this.filterStatus === 'active' && user.isActive) ||
      (this.filterStatus === 'inactive' && !user.isActive);
    
    return matchesSearch && matchesRole && matchesStatus;
  });
}
```

#### **FRONTEND: Angular Renders**

```html
<!-- 20. Angular *ngFor renders each user -->
<tr *ngFor="let user of filteredUsers">
  <td>
    <div class="d-flex align-items-center">
      <!-- 21. Check if user is online -->
      <div class="online-status-indicator me-2" 
           [class.online]="isUserOnline(user)">
      </div>
      <strong>{{ user.username }}</strong>
    </div>
  </td>
  <td>{{ user.firstName }} {{ user.lastName }}</td>
  <td>{{ user.email }}</td>
  <td>
    <span class="badge bg-info">{{ user.roleName }}</span>
  </td>
  <td>
    <span class="badge" 
          [class]="user.isActive ? 'bg-success' : 'bg-secondary'">
      {{ user.isActive ? 'Active' : 'Inactive' }}
    </span>
  </td>
  <td>
    <!-- Action buttons -->
  </td>
</tr>
```

### Load Users Flow Summary

```
Component initializes (ngOnInit)
    ↓
Call loadUsers()
    ↓
API service adds JWT token
    ↓
HTTP GET to /api/users
    ↓
Backend validates JWT token
    ↓
UsersController calls UserService
    ↓
UserService calls UserRepository
    ↓
UserRepository queries database with EF Core
    ↓
SQL query with JOIN to get users + roles
    ↓
Return users to service
    ↓
Map entities to DTOs
    ↓
Return JSON response
    ↓
Frontend receives array of users
    ↓
Store in component property
    ↓
Apply filters (search, role, status)
    ↓
Angular renders with *ngFor
    ↓
UI displays user table
```

---

## 4. Create Location Flow

This follows a similar pattern to Create User, with some key differences:

### Key Steps

1. **Frontend**: User fills location form (office or plaza)
2. **Frontend**: Validates required fields based on type
3. **Frontend**: Maps form data to backend format
4. **HTTP POST**: `/api/superadmin/locations`
5. **Backend**: SuperAdminController receives request
6. **Backend**: LocationService validates project exists
7. **Backend**: Generate LocationId (LOC00001)
8. **Backend**: Get Project to fetch ProjectIdRef
9. **Backend**: LocationRepository inserts to database
10. **Backend**: AuditService logs LOCATION_CREATED
11. **Response**: Return created location
12. **Frontend**: Refresh project locations list with 500ms delay
13. **Frontend**: Update UI

### Unique Aspects

- **Alternate Keys**: Both LocationId and ProjectIdRef must be set
- **Type-Specific Fields**: Plaza vs Office have different required fields
- **Delayed Refresh**: 500ms timeout ensures database transaction completes

---

## 5. Session Management Flow

### Concurrent Login Prevention

#### **Login Attempt**
```
User A logs in → Session created
User B tries to login with same account
    ↓
Backend checks User.ActiveSessionId
    ↓
Calculate time since LastActivityAt
    ↓
If < 30 minutes → BLOCK LOGIN
If > 30 minutes → Clear old session, allow login
```

#### **Activity Tracking**
```
Every HTTP request
    ↓
ActivityTrackingMiddleware executes
    ↓
Extract userId from JWT token
    ↓
Update User.LastActivityAt = DateTime.UtcNow
    ↓
Save to database
```

#### **Session Timeout**
```
User inactive for 30+ minutes
    ↓
LastActivityAt becomes stale
    ↓
Next login attempt
    ↓
Backend sees session expired
    ↓
Clear ActiveSessionId
    ↓
Allow new login
```

---

## 6. Activity Tracking Flow

### Every Request Updates Activity

```
Frontend makes ANY HTTP request
    ↓
Request includes JWT token
    ↓
Backend middleware pipeline:
  1. CORS
  2. Authentication (validate JWT)
  3. Authorization
  4. Route to controller
  5. Controller executes
  6. ActivityTrackingMiddleware (AFTER response)
    ↓
Extract userId from JWT claims
    ↓
Find user in database
    ↓
Update LastActivityAt = DateTime.UtcNow
    ↓
SaveChanges to database
    ↓
Response sent to frontend
```

### Online Status Detection

**Frontend checks:**
```typescript
isUserOnline(user: User): boolean {
  // Check if this is current user
  if (this.currentUsername === user.username) {
    return true;
  }
  
  // Check LastLoginAt
  if (!user.lastLoginAt) {
    return false;
  }
  
  // Online if last login within 30 minutes
  const lastLogin = new Date(user.lastLoginAt).getTime();
  const now = Date.now();
  const thirtyMinutes = 30 * 60 * 1000;
  
  return (now - lastLogin) < thirtyMinutes;
}
```

---

## Complete Request-Response Cycle

```
┌─────────────┐
│   Browser   │
│  (Angular)  │
└──────┬──────┘
       │ 1. User Action (click, input, etc.)
       │
       ▼
┌─────────────┐
│  Component  │ 2. Event Handler
│   (.ts)     │ 3. Validation
└──────┬──────┘
       │ 4. Call Service
       ▼
┌─────────────┐
│ API Service │ 5. Add JWT Token
│   (api.ts)  │ 6. HTTP Request
└──────┬──────┘
       │
       │ HTTP (JSON)
       │
       ▼
┌─────────────┐
│   Network   │ 7. TCP/IP
│             │ 8. DNS Resolution
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Backend   │ 9. Kestrel Web Server
│  (ASP.NET)  │ 10. Middleware Pipeline
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ Controller  │ 11. Route Matching
│   (.cs)     │ 12. Model Binding
└──────┬──────┘
       │ 13. Call Service
       ▼
┌─────────────┐
│   Service   │ 14. Business Logic
│   (.cs)     │ 15. Validation
└──────┬──────┘
       │ 16. Call Repository
       ▼
┌─────────────┐
│ Repository  │ 17. EF Core Query
│   (.cs)     │ 18. LINQ to SQL
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Database   │ 19. Execute SQL
│ (SQL Server)│ 20. Return Results
└──────┬──────┘
       │
       │ (Results flow back up)
       │
       ▼
┌─────────────┐
│  Response   │ 21. Map to DTO
│             │ 22. Serialize JSON
│             │ 23. HTTP Response
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Frontend   │ 24. Deserialize JSON
│             │ 25. Update Component State
│             │ 26. Angular Change Detection
│             │ 27. Re-render UI
└─────────────┘
```

---

## Summary

Every user interaction follows this pattern:
1. **User Action** → Event handler in component
2. **Validation** → Frontend checks
3. **HTTP Request** → API service with JWT token
4. **Middleware** → CORS, Auth, Activity tracking
5. **Controller** → Route to appropriate endpoint
6. **Service** → Business logic and validation
7. **Repository** → Database operations via EF Core
8. **Database** → SQL queries execute
9. **Response** → Map to DTO, return JSON
10. **Frontend** → Update state, re-render UI

The beauty of this architecture is:
- **Separation of Concerns**: Each layer has a specific responsibility
- **Type Safety**: TypeScript + C# catch errors at compile time
- **Testability**: Each layer can be tested independently
- **Maintainability**: Changes in one layer don't affect others
- **Security**: JWT tokens, password hashing, session management
- **Audit Trail**: Every action is logged
- **Real-time Updates**: Angular change detection updates UI immediately
