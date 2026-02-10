# ITAMS Database Schema - Primary Keys and Foreign Key References

## Overview
This document lists all primary keys in the ITAMS database and shows where they are referenced as foreign keys.

---

## 1. Users Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **Assets.AssignedUserId** → Users.Id
   - Purpose: Track which user an asset is assigned to

2. **AuditEntries.UserId** → Users.Id
   - Purpose: Track which user performed an audit action

3. **AuditLogs.ActorId** → Users.Id
   - Purpose: Track the user who performed an action

4. **RbacAccessAuditLog.UserId** → Users.Id
   - Purpose: Track user access attempts

5. **RbacPermissionAuditLog.ActorUserId** → Users.Id
   - Purpose: Track who made permission changes

6. **RbacPermissionAuditLog.TargetUserId** → Users.Id
   - Purpose: Track whose permissions were changed

7. **RbacPermissions.CreatedBy** → Users.Id
   - Purpose: Track who created the permission

8. **RbacPermissions.DeactivatedBy** → Users.Id
   - Purpose: Track who deactivated the permission

9. **RbacRolePermissions.GrantedBy** → Users.Id
   - Purpose: Track who granted the role permission

10. **RbacRolePermissions.RevokedBy** → Users.Id
    - Purpose: Track who revoked the role permission

11. **RbacRoles.CreatedBy** → Users.Id
    - Purpose: Track who created the role

12. **RbacRoles.DeactivatedBy** → Users.Id
    - Purpose: Track who deactivated the role

13. **RbacUserPermissions.UserId** → Users.Id
    - Purpose: Link user-specific permission overrides

14. **RbacUserPermissions.GrantedBy** → Users.Id
    - Purpose: Track who granted the user permission

15. **RbacUserPermissions.RevokedBy** → Users.Id
    - Purpose: Track who revoked the user permission

16. **RbacUserScope.UserId** → Users.Id
    - Purpose: Link user to their project scope

17. **RbacUserScope.AssignedBy** → Users.Id
    - Purpose: Track who assigned the scope

18. **RbacUserScope.RemovedBy** → Users.Id
    - Purpose: Track who removed the scope

19. **Roles.CreatedBy** → Users.Id
    - Purpose: Track who created the role

20. **Roles.DeactivatedByUserId** → Users.Id
    - Purpose: Track who deactivated the role

21. **SecurityAlerts.UserId** → Users.Id
    - Purpose: Link alert to the user it concerns

22. **SecurityAlerts.AssignedToUserId** → Users.Id
    - Purpose: Track who the alert is assigned to

23. **SecurityAuditLogs.ActorUserId** → Users.Id
    - Purpose: Track who performed the security action

24. **SecurityAuditLogs.TargetUserId** → Users.Id
    - Purpose: Track who was affected by the security action

25. **UserPermissions.UserId** → Users.Id
    - Purpose: Link permission to user

26. **UserPermissions.GrantedByUserId** → Users.Id
    - Purpose: Track who granted the permission

27. **UserProjects.UserId** → Users.Id
    - Purpose: Link user to project

28. **Users.CreatedBy** → Users.Id (Self-reference)
    - Purpose: Track who created the user

29. **Users.DeactivatedBy** → Users.Id (Self-reference)
    - Purpose: Track who deactivated the user

30. **Users.DeactivatedByUserId** → Users.Id (Self-reference)
    - Purpose: Track who deactivated the user (duplicate)

31. **UserScopes.UserId** → Users.Id
    - Purpose: Link user to their scope

32. **UserScopes.AssignedByUserId** → Users.Id
    - Purpose: Track who assigned the scope

---

## 2. Roles Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **Users.RoleId** → Roles.Id
   - Purpose: Assign a role to each user

2. **RolePermissions.RoleId** → Roles.Id
   - Purpose: Link permissions to roles

---

## 3. RbacRoles Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **RbacRolePermissions.RoleId** → RbacRoles.Id
   - Purpose: Link RBAC permissions to RBAC roles

2. **RbacPermissionAuditLog.RoleId** → RbacRoles.Id
   - Purpose: Track role-related permission changes

---

## 4. Permissions Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **RolePermissions.PermissionId** → Permissions.Id
   - Purpose: Link permissions to roles

2. **UserPermissions.PermissionId** → Permissions.Id
   - Purpose: Link permissions to users

3. **UserProjectPermissions.PermissionId** → Permissions.Id
   - Purpose: Link permissions to user-project combinations

---

## 5. RbacPermissions Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **RbacRolePermissions.PermissionId** → RbacPermissions.Id
   - Purpose: Link RBAC permissions to RBAC roles

2. **RbacUserPermissions.PermissionId** → RbacPermissions.Id
   - Purpose: Link RBAC permissions to users (overrides)

3. **RbacPermissionAuditLog.PermissionId** → RbacPermissions.Id
   - Purpose: Track permission-related changes

---

## 6. Projects Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **Assets.ProjectId** → Projects.Id
   - Purpose: Link assets to projects

2. **Locations.ProjectId** → Projects.Id
   - Purpose: Link locations to projects

3. **RbacUserScope.ProjectId** → Projects.Id
   - Purpose: Define user's project scope for RBAC

4. **UserPermissions.ProjectId** → Projects.Id
   - Purpose: Link user permissions to specific projects

5. **UserProjects.ProjectId** → Projects.Id
   - Purpose: Link users to projects

6. **UserScopes.ProjectId** → Projects.Id
   - Purpose: Define user's project scope

---

## 7. Locations Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **Assets.LocationId** → Locations.Id
   - Purpose: Track where assets are physically located

---

## 8. Assets Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
- Currently no tables reference Assets.Id
- Assets is a leaf table in the current schema

---

## 9. UserProjects Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **UserProjectPermissions.UserProjectId** → UserProjects.Id
   - Purpose: Link permissions to specific user-project combinations

---

## 10. SecurityAuditLogs Table
**Primary Key:** `Id` (INT)

### Referenced By (Foreign Keys):
1. **SecurityAlerts.AuditLogId** → SecurityAuditLogs.Id
   - Purpose: Link security alerts to their audit log entries

---

## 11. Tables with No Foreign Key References

The following tables have primary keys but are not currently referenced by any other tables:

1. **RolePermissions.Id** - Junction table for Roles and Permissions
2. **RbacRolePermissions.Id** - Junction table for RBAC Roles and Permissions
3. **RbacUserPermissions.Id** - User-specific RBAC permission overrides
4. **RbacUserScope.Id** - User project scope definitions
5. **UserPermissions.Id** - User-specific permissions
6. **UserProjectPermissions.Id** - User-project-specific permissions
7. **UserScopes.Id** - User scope definitions
8. **SecurityAlerts.Id** - Security alert records

---

## Summary Statistics

- **Total Tables with Primary Keys:** 18
- **Total Foreign Key Relationships:** 51
- **Most Referenced Table:** Users (32 references)
- **Second Most Referenced:** Projects (6 references)
- **Third Most Referenced:** RbacPermissions (3 references)

---

## Key Relationships Diagram

```
Users (Id)
├── Assets (AssignedUserId)
├── Locations (via Projects)
├── Projects (via UserProjects)
├── Roles (RoleId) ← Users
├── RbacRoles (via RbacUserPermissions)
├── RbacPermissions (via RbacUserPermissions)
└── Multiple Audit/Tracking tables

Projects (Id)
├── Assets (ProjectId)
├── Locations (ProjectId)
├── UserProjects (ProjectId)
├── UserPermissions (ProjectId)
├── UserScopes (ProjectId)
└── RbacUserScope (ProjectId)

Roles (Id)
├── Users (RoleId)
└── RolePermissions (RoleId)

RbacRoles (Id)
└── RbacRolePermissions (RoleId)

Permissions (Id)
├── RolePermissions (PermissionId)
├── UserPermissions (PermissionId)
└── UserProjectPermissions (PermissionId)

RbacPermissions (Id)
├── RbacRolePermissions (PermissionId)
└── RbacUserPermissions (PermissionId)
```

---

## Notes

1. **Users table** is the most heavily referenced table, serving as the central entity for authentication, authorization, and audit tracking.

2. **Dual Permission Systems**: The database has both legacy (Permissions, Roles) and new RBAC (RbacPermissions, RbacRoles) systems running in parallel.

3. **Audit Trail**: Most tables have CreatedBy, UpdatedBy, and related tracking fields that reference Users.Id.

4. **Self-Referencing**: Users table has self-referencing foreign keys for tracking who created/deactivated other users.

5. **Junction Tables**: Several many-to-many relationships are implemented through junction tables (RolePermissions, UserProjects, etc.).

---

*Generated: 2026-02-09*
*Database: ITAMS_Shared*
