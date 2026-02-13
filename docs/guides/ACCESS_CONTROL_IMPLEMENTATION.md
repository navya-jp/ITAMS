# Project-Wise and Location-Wise Access Control Implementation

## Overview
Implement granular access control combining:
- Role-based access (RBAC)
- Project-based isolation
- Location-based hierarchical filtering

## Core Rules

### 1. Project Isolation (Mandatory)
- Every user belongs to exactly ONE project
- Users can ONLY access data within their assigned project
- Even Admins are restricted to their project
- SuperAdmin is the only role with cross-project access

### 2. Location Hierarchy
```
Region → State → Plaza → Office
```

**Filtering Logic:**
- Office assigned → Access only that office
- Plaza assigned → Access all offices in that plaza
- State assigned → Access all plazas/offices in that state
- Region assigned → Access all states/plazas/offices in that region
- No restriction → Full access within project

### 3. Sensitive Locations
- Marked locations require explicit permission
- Only SuperAdmin or explicitly authorized users can access

## Database Schema Changes

### Users Table
```sql
ALTER TABLE Users ADD ProjectId INT NULL;
ALTER TABLE Users ADD RestrictedRegion NVARCHAR(100) NULL;
ALTER TABLE Users ADD RestrictedState NVARCHAR(100) NULL;
ALTER TABLE Users ADD RestrictedPlaza NVARCHAR(100) NULL;
ALTER TABLE Users ADD RestrictedOffice NVARCHAR(100) NULL;
ALTER TABLE Users ADD CONSTRAINT FK_Users_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id);
```

### Locations Table
```sql
ALTER TABLE Locations ADD IsSensitive BIT NOT NULL DEFAULT 0;
ALTER TABLE Locations ADD SensitiveReason NVARCHAR(500) NULL;
```

### UserProjectAccess Table (New)
```sql
CREATE TABLE UserProjectAccess (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProjectId INT NOT NULL,
    RoleId INT NOT NULL,
    RestrictedRegion NVARCHAR(100) NULL,
    RestrictedState NVARCHAR(100) NULL,
    RestrictedPlaza NVARCHAR(100) NULL,
    RestrictedOffice NVARCHAR(100) NULL,
    CanAccessSensitiveLocations BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NOT NULL,
    CONSTRAINT FK_UserProjectAccess_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserProjectAccess_Projects FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    CONSTRAINT FK_UserProjectAccess_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT UQ_UserProjectAccess UNIQUE (UserId, ProjectId)
);
```

## Backend Implementation

### 1. Access Control Service
```csharp
public interface IAccessControlService
{
    Task<bool> CanAccessProject(int userId, int projectId);
    Task<bool> CanAccessLocation(int userId, int locationId);
    Task<IQueryable<T>> ApplyProjectFilter<T>(IQueryable<T> query, int userId) where T : IProjectEntity;
    Task<IQueryable<Location>> ApplyLocationFilter(IQueryable<Location> query, int userId);
}
```

### 2. Access Control Middleware
- Intercept all requests
- Extract user context
- Apply project/location filters automatically

### 3. Repository Updates
- All repositories must use IAccessControlService
- Filter queries by project and location
- Prevent cross-project data leakage

## Frontend Implementation

### 1. Super Admin Features
- Assign users to projects
- Set location restrictions
- Manage sensitive locations
- View cross-project data

### 2. User Dashboard
- Show assigned project
- Display location restrictions
- Show accessible locations

### 3. Navigation Updates
- Hide inaccessible features
- Show only relevant projects/locations

## Security Checklist

- [ ] Project isolation enforced at database level
- [ ] Location hierarchy filtering implemented
- [ ] Sensitive locations protected
- [ ] API endpoints secured
- [ ] Frontend hides unauthorized features
- [ ] Audit logging for access attempts
- [ ] No privilege escalation possible
- [ ] Cross-project access blocked
- [ ] SuperAdmin exception handled correctly

## Implementation Phases

### Phase 1: Database (Day 1)
- Create migration scripts
- Add new tables and columns
- Update foreign keys

### Phase 2: Backend Core (Day 2-3)
- Implement AccessControlService
- Create middleware
- Update repositories

### Phase 3: Backend APIs (Day 4)
- Update all controllers
- Add project assignment endpoints
- Add location restriction endpoints

### Phase 4: Frontend (Day 5-6)
- Update user management
- Create project assignment UI
- Update navigation and dashboards

### Phase 5: Testing (Day 7)
- Test project isolation
- Test location filtering
- Test sensitive locations
- Security audit

## Notes
- SuperAdmin (RoleId = 1) bypasses all restrictions
- All other roles must have project assignment
- Location restrictions are optional
- Sensitive locations require explicit permission
