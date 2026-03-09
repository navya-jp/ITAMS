# ITAMS Database Structure

## 📊 Complete Database Schema Documentation

---

## Overview

The ITAMS database uses **SQL Server** with **Entity Framework Core** ORM. The database follows a **normalized relational design** with proper foreign key relationships, indexes, and constraints.

---

## 🗂️ Database Tables

### **Core Tables (11 tables)**

#### 1. **Users**
Stores user account information and authentication details.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| UserId | NVARCHAR(50) | UNIQUE, NOT NULL | Alternate key (USR00001, USR00002...) |
| Username | NVARCHAR(100) | UNIQUE, NOT NULL | Login username |
| Email | NVARCHAR(255) | UNIQUE, NOT NULL | User email address |
| PasswordHash | NVARCHAR(MAX) | NOT NULL | BCrypt hashed password |
| FirstName | NVARCHAR(100) | NOT NULL | User's first name |
| LastName | NVARCHAR(100) | NOT NULL | User's last name |
| RoleId | INT | FOREIGN KEY → Roles | User's role |
| ProjectId | INT | FOREIGN KEY → Projects, NULL | Assigned project |
| RestrictedRegion | NVARCHAR(100) | NULL | Location restriction - Region |
| RestrictedState | NVARCHAR(100) | NULL | Location restriction - State |
| RestrictedPlaza | NVARCHAR(100) | NULL | Location restriction - Plaza |
| RestrictedOffice | NVARCHAR(100) | NULL | Location restriction - Office |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Account active status |
| IsLocked | BIT | NOT NULL, DEFAULT 0 | Account locked status |
| MustChangePassword | BIT | NOT NULL, DEFAULT 0 | Force password change flag |
| PasswordResetRequested | BIT | NULL | Password reset requested |
| PasswordResetRequestedAt | DATETIME | NULL | Password reset request time |
| LastLoginAt | DATETIME | NULL | Last successful login |
| LastActivityAt | DATETIME | NULL | Last activity timestamp |
| ActiveSessionId | NVARCHAR(255) | NULL | Current active session ID |
| SessionStartedAt | DATETIME | NULL | Session start time |
| CreatedAt | DATETIME | NOT NULL | Account creation date |
| UpdatedAt | DATETIME | NULL | Last update date |

**Indexes:**
- UNIQUE INDEX on Username
- UNIQUE INDEX on Email
- UNIQUE INDEX on UserId
- INDEX on RoleId (FK)

---

#### 2. **Roles**
Defines user roles for RBAC system.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| RoleId | NVARCHAR(50) | UNIQUE, NOT NULL | Alternate key (ROL00001, ROL00002...) |
| Name | NVARCHAR(100) | UNIQUE, NOT NULL | Role name |
| Description | NVARCHAR(500) | NULL | Role description |
| IsSystemRole | BIT | NOT NULL, DEFAULT 0 | System-defined role flag |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Role active status |
| CreatedAt | DATETIME | NOT NULL | Creation date |
| UpdatedAt | DATETIME | NULL | Last update date |

**Default Roles:**
1. Super Admin (Full system access)
2. Admin (Project administration)
3. IT Staff (Asset management)
4. Read Only User (View-only)
5. Auditor (Audit and reporting)

**Indexes:**
- UNIQUE INDEX on Name
- UNIQUE INDEX on RoleId

---

#### 3. **Permissions**
Defines granular permissions for the system.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| PermissionId | NVARCHAR(50) | UNIQUE, NOT NULL | Alternate key (PER00001, PER00002...) |
| Name | NVARCHAR(100) | NOT NULL | Permission display name |
| Code | NVARCHAR(100) | UNIQUE, NOT NULL | Permission code (e.g., USERS_VIEW) |
| Module | NVARCHAR(50) | NOT NULL | Module name (Users, Assets, etc.) |
| Description | NVARCHAR(500) | NULL | Permission description |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Permission active status |

**Permission Modules:**
- Users (5 permissions)
- Projects (4 permissions)
- Locations (4 permissions)
- Assets (5 permissions)
- Reports (2 permissions)
- Settings (2 permissions)
- Audit (1 permission)

**Total: 23 default permissions**

**Indexes:**
- UNIQUE INDEX on Code
- UNIQUE INDEX on PermissionId

---

#### 4. **RolePermissions**
Many-to-many relationship between Roles and Permissions.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| RoleId | INT | FOREIGN KEY → Roles | Role reference |
| PermissionId | INT | FOREIGN KEY → Permissions | Permission reference |
| IsGranted | BIT | NOT NULL, DEFAULT 1 | Permission granted flag |
| CreatedAt | DATETIME | NOT NULL | Assignment date |

**Indexes:**
- UNIQUE INDEX on (RoleId, PermissionId)

---

#### 5. **Projects**
Stores project/organization information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| ProjectId | NVARCHAR(50) | UNIQUE, NOT NULL | Alternate key (PRJ00001, PRJ00002...) |
| Name | NVARCHAR(100) | NOT NULL | Project name (SPV Name) |
| PreferredName | NVARCHAR(100) | NULL | Preferred display name |
| Code | NVARCHAR(50) | UNIQUE, NOT NULL | Project code |
| SpvName | NVARCHAR(200) | NULL | SPV (Special Purpose Vehicle) name |
| States | NVARCHAR(MAX) | NULL | Comma-separated states |
| Description | NVARCHAR(500) | NULL | Project description |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Project active status |
| CreatedAt | DATETIME | NOT NULL | Creation date |
| UpdatedAt | DATETIME | NULL | Last update date |

**Indexes:**
- UNIQUE INDEX on Code
- UNIQUE INDEX on ProjectId

---

#### 6. **Locations**
Stores location information (offices, plazas, sites).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| LocationId | NVARCHAR(50) | UNIQUE, NOT NULL | Alternate key (LOC00001, LOC00002...) |
| Name | NVARCHAR(100) | NOT NULL | Location name |
| Type | NVARCHAR(50) | NOT NULL | Location type (office/plaza) |
| ProjectId | INT | FOREIGN KEY → Projects | Parent project |
| Region | NVARCHAR(100) | NOT NULL | Region name |
| State | NVARCHAR(100) | NOT NULL | State name |
| Plaza | NVARCHAR(200) | NULL | Plaza name |
| Office | NVARCHAR(200) | NULL | Office name |
| District | NVARCHAR(100) | NULL | District (for offices) |
| PlazaCode | NVARCHAR(50) | NULL | Plaza code |
| GovernmentCode | NVARCHAR(50) | NULL | Government-assigned code |
| ChainageNumber | NVARCHAR(50) | NULL | Chainage number |
| Latitude | DECIMAL(10,7) | NULL | GPS latitude |
| Longitude | DECIMAL(10,7) | NULL | GPS longitude |
| NumberOfLanes | INT | NULL | Number of lanes (for plazas) |
| Address | NVARCHAR(500) | NULL | Full address |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Location active status |
| CreatedAt | DATETIME | NOT NULL | Creation date |
| UpdatedAt | DATETIME | NULL | Last update date |

**Indexes:**
- UNIQUE INDEX on LocationId
- INDEX on ProjectId (FK)

---

#### 7. **Assets**
Main table storing all asset information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| AssetId | NVARCHAR(50) | UNIQUE, NOT NULL | Auto-generated ID (AST00001, AST00002...) |
| AssetTag | NVARCHAR(50) | UNIQUE, NOT NULL | User-defined asset tag |
| ProjectId | INT | FOREIGN KEY → Projects | Parent project |
| LocationId | INT | FOREIGN KEY → Locations | Asset location |
| **Basic Information** | | | |
| UsageCategory | INT | NOT NULL | Enum: TMS, ITNonTMS |
| Classification | NVARCHAR(100) | NULL | Asset classification |
| Status | INT | NOT NULL | Enum: InUse, Spare, Repair, Decommissioned |
| Placing | NVARCHAR(50) | NOT NULL | Physical placement area |
| **Location Details** | | | |
| Region | NVARCHAR(100) | NULL | Region name |
| State | NVARCHAR(100) | NULL | State name |
| PlazaName | NVARCHAR(200) | NULL | Plaza name |
| LocationText | NVARCHAR(200) | NULL | Free-text location |
| Department | NVARCHAR(100) | NULL | Department name |
| **Asset Details** | | | |
| AssetType | NVARCHAR(100) | NOT NULL | Hardware/Software |
| SubType | NVARCHAR(100) | NULL | Asset sub-type |
| Make | NVARCHAR(100) | NOT NULL | Manufacturer |
| Model | NVARCHAR(100) | NOT NULL | Model name/number |
| SerialNumber | NVARCHAR(100) | NULL | Serial number |
| **Technical Details** | | | |
| OsType | NVARCHAR(100) | NULL | Operating system type |
| OsVersion | NVARCHAR(100) | NULL | OS version |
| DbType | NVARCHAR(100) | NULL | Database type |
| DbVersion | NVARCHAR(100) | NULL | Database version |
| IpAddress | NVARCHAR(50) | NULL | IP address |
| **User Assignment** | | | |
| AssignedUserId | INT | FOREIGN KEY → Users, NULL | Assigned user |
| AssignedUserText | NVARCHAR(200) | NULL | User name (free text) |
| UserRole | NVARCHAR(100) | NULL | User's role |
| AssignedUserRole | NVARCHAR(100) | NULL | Assignment role |
| **Status & Security** | | | |
| PatchStatus | NVARCHAR(100) | NULL | Patch/update status |
| UsbBlockingStatus | NVARCHAR(100) | NULL | USB blocking status |
| **Procurement** | | | |
| ProcurementDate | DATETIME | NULL | Purchase date |
| ProcurementCost | DECIMAL(18,2) | NULL | Purchase cost |
| Vendor | NVARCHAR(200) | NULL | Vendor name |
| ProcuredBy | NVARCHAR(200) | NULL | Procured by (person/dept) |
| WarrantyStartDate | DATETIME | NULL | Warranty start |
| WarrantyEndDate | DATETIME | NULL | Warranty end |
| CommissioningDate | DATETIME | NULL | Commissioning date |
| **Additional** | | | |
| Remarks | NVARCHAR(MAX) | NULL | Additional notes |
| CreatedAt | DATETIME | NOT NULL | Creation date |
| UpdatedAt | DATETIME | NULL | Last update date |
| CreatedBy | INT | FOREIGN KEY → Users | Creator user |
| UpdatedBy | INT | FOREIGN KEY → Users, NULL | Last updater |

**Enums:**
- **UsageCategory**: TMS (0), ITNonTMS (1)
- **Status**: InUse (0), Spare (1), Repair (2), Decommissioned (3)
- **Placing**: Lane Area, Booth Area, Plaza Area, Server Room, Control Room, Admin Building

**Indexes:**
- UNIQUE INDEX on AssetTag
- UNIQUE INDEX on AssetId
- INDEX on ProjectId (FK)
- INDEX on LocationId (FK)
- INDEX on AssignedUserId (FK)

---

#### 8. **UserProjects**
Many-to-many relationship between Users and Projects.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| UserId | INT | FOREIGN KEY → Users | User reference |
| ProjectId | INT | FOREIGN KEY → Projects | Project reference |
| AssignedAt | DATETIME | NOT NULL | Assignment date |

**Indexes:**
- UNIQUE INDEX on (UserId, ProjectId)

---

#### 9. **UserProjectPermissions**
User-specific permission overrides for projects.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| UserProjectId | INT | FOREIGN KEY → UserProjects | User-project reference |
| PermissionId | INT | FOREIGN KEY → Permissions | Permission reference |
| IsGranted | BIT | NOT NULL | Permission granted/denied |
| CreatedAt | DATETIME | NOT NULL | Assignment date |

**Indexes:**
- UNIQUE INDEX on (UserProjectId, PermissionId)

---

#### 10. **AuditEntries**
General audit trail for entity changes.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| UserId | INT | FOREIGN KEY → Users | User who made change |
| UserName | NVARCHAR(100) | NOT NULL | Username (denormalized) |
| Action | NVARCHAR(100) | NOT NULL | Action performed (Create, Update, Delete) |
| EntityType | NVARCHAR(100) | NOT NULL | Entity type (User, Asset, etc.) |
| EntityId | INT | NOT NULL | Entity ID |
| Changes | NVARCHAR(MAX) | NULL | JSON of changes |
| IpAddress | NVARCHAR(50) | NULL | User's IP address |
| Timestamp | DATETIME | NOT NULL | Action timestamp |

**Indexes:**
- INDEX on UserId (FK)
- INDEX on Timestamp
- INDEX on EntityType

---

#### 11. **LoginAudit**
Tracks user login/logout activity.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| UserId | INT | FOREIGN KEY → Users | User reference |
| Username | NVARCHAR(100) | NOT NULL | Username (denormalized) |
| LoginTime | DATETIME | NOT NULL | Login timestamp |
| LogoutTime | DATETIME | NULL | Logout timestamp |
| IpAddress | NVARCHAR(50) | NULL | User's IP address |
| BrowserType | NVARCHAR(100) | NULL | Browser information |
| OperatingSystem | NVARCHAR(100) | NULL | OS information |
| SessionId | NVARCHAR(255) | NULL | Session identifier |
| Status | NVARCHAR(50) | NOT NULL | Session status |

**Status Values:**
- ACTIVE: Currently logged in
- LOGGED_OUT: Normal logout
- SESSION_TIMEOUT: Timed out
- FORCED_LOGOUT: Admin forced logout

**Indexes:**
- INDEX on UserId (FK)
- INDEX on LoginTime
- INDEX on SessionId

---

### **System Configuration Tables (1 table)**

#### 12. **SystemSettings**
Stores configurable system parameters.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| SettingKey | NVARCHAR(100) | UNIQUE, NOT NULL | Setting identifier |
| SettingValue | NVARCHAR(500) | NOT NULL | Setting value |
| Category | NVARCHAR(50) | NOT NULL | Setting category |
| DataType | NVARCHAR(20) | NOT NULL | Value data type |
| Description | NVARCHAR(500) | NULL | Setting description |
| IsEditable | BIT | NOT NULL, DEFAULT 1 | Can be edited by users |
| CreatedAt | DATETIME | NOT NULL | Creation date |
| UpdatedAt | DATETIME | NULL | Last update date |

**Categories:**
- Security (session timeout, password policy)
- System (file upload limits, pagination)
- Notifications (email settings)
- Audit (retention period)

**Indexes:**
- UNIQUE INDEX on SettingKey

---

### **Master Data Tables (8 tables)**

#### 13. **Vendors**
Stores vendor/supplier information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| VendorId | NVARCHAR(50) | UNIQUE, NOT NULL | Vendor code |
| Name | NVARCHAR(200) | NOT NULL | Vendor name |
| ContactPerson | NVARCHAR(100) | NULL | Contact person |
| Email | NVARCHAR(255) | NULL | Email address |
| Phone | NVARCHAR(20) | NULL | Phone number |
| Address | NVARCHAR(500) | NULL | Address |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active status |
| CreatedAt | DATETIME | NOT NULL | Creation date |

---

#### 14. **AssetStatuses**
Master data for asset statuses.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| StatusCode | NVARCHAR(50) | UNIQUE, NOT NULL | Status code |
| StatusName | NVARCHAR(100) | NOT NULL | Status display name |
| Description | NVARCHAR(500) | NULL | Status description |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active status |

---

#### 15. **CriticalityLevels**
Master data for asset criticality levels.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| LevelCode | NVARCHAR(50) | UNIQUE, NOT NULL | Level code |
| LevelName | NVARCHAR(100) | NOT NULL | Level display name |
| Description | NVARCHAR(500) | NULL | Level description |
| Priority | INT | NOT NULL | Priority order |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active status |

---

#### 16. **AssetMasterFields**
Defines custom fields for assets.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| FieldName | NVARCHAR(100) | NOT NULL | Field name |
| FieldType | NVARCHAR(50) | NOT NULL | Data type |
| IsRequired | BIT | NOT NULL, DEFAULT 0 | Required field flag |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active status |

---

#### 17. **AssetCategories**
Asset category master data.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| CategoryCode | NVARCHAR(50) | UNIQUE, NOT NULL | Category code |
| CategoryName | NVARCHAR(100) | NOT NULL | Category name |
| Description | NVARCHAR(500) | NULL | Description |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active status |

---

#### 18. **AssetTypes**
Asset type master data.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| TypeCode | NVARCHAR(50) | UNIQUE, NOT NULL | Type code |
| TypeName | NVARCHAR(100) | NOT NULL | Type name |
| CategoryId | INT | FOREIGN KEY → AssetCategories | Parent category |
| Description | NVARCHAR(500) | NULL | Description |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active status |

---

#### 19. **AssetSubTypes**
Asset sub-type master data.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| SubTypeCode | NVARCHAR(50) | UNIQUE, NOT NULL | Sub-type code |
| SubTypeName | NVARCHAR(100) | NOT NULL | Sub-type name |
| AssetTypeId | INT | FOREIGN KEY → AssetTypes | Parent type |
| Description | NVARCHAR(500) | NULL | Description |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active status |

---

#### 20. **TypeFieldMappings**
Maps custom fields to asset types.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PRIMARY KEY, IDENTITY | Auto-increment primary key |
| AssetTypeId | INT | FOREIGN KEY → AssetTypes | Asset type |
| FieldId | INT | FOREIGN KEY → AssetMasterFields | Custom field |
| IsRequired | BIT | NOT NULL, DEFAULT 0 | Required for this type |
| DisplayOrder | INT | NOT NULL | Display order |

---

### **Workflow Tables (6 tables)**

#### 21-26. **Approval Workflow Tables**
Tables for approval workflow management (future enhancement):
- ApprovalWorkflows
- ApprovalLevels
- ApprovalRequests
- ApprovalHistory
- EscalationRules
- EscalationLogs

---

## 🔗 Entity Relationships

### **One-to-Many Relationships**

1. **Roles → Users**
   - One role can have many users
   - FK: Users.RoleId → Roles.Id

2. **Projects → Users**
   - One project can have many users
   - FK: Users.ProjectId → Projects.Id

3. **Projects → Locations**
   - One project can have many locations
   - FK: Locations.ProjectId → Projects.Id

4. **Projects → Assets**
   - One project can have many assets
   - FK: Assets.ProjectId → Projects.Id

5. **Locations → Assets**
   - One location can have many assets
   - FK: Assets.LocationId → Locations.Id

6. **Users → Assets (Assigned)**
   - One user can be assigned many assets
   - FK: Assets.AssignedUserId → Users.Id

7. **Users → AuditEntries**
   - One user can have many audit entries
   - FK: AuditEntries.UserId → Users.Id

8. **Users → LoginAudit**
   - One user can have many login records
   - FK: LoginAudit.UserId → Users.Id

### **Many-to-Many Relationships**

1. **Roles ↔ Permissions**
   - Through: RolePermissions
   - A role can have many permissions
   - A permission can belong to many roles

2. **Users ↔ Projects**
   - Through: UserProjects
   - A user can be assigned to many projects
   - A project can have many users

3. **UserProjects ↔ Permissions**
   - Through: UserProjectPermissions
   - User-specific permission overrides per project

---

## 📈 Database Diagram

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Roles     │────────>│    Users     │<────────│  Projects   │
└─────────────┘         └──────────────┘         └─────────────┘
      │                        │                         │
      │                        │                         │
      ▼                        ▼                         ▼
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│Permissions  │         │ LoginAudit   │         │  Locations  │
└─────────────┘         └──────────────┘         └─────────────┘
      │                        │                         │
      │                        │                         │
      ▼                        ▼                         ▼
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│RolePermis-  │         │ AuditEntries │         │   Assets    │
│sions        │         └──────────────┘         └─────────────┘
└─────────────┘
```

---

## 🔐 Security Features

### **Data Protection**
1. **Password Hashing**: BCrypt with salt
2. **Unique Constraints**: Username, Email, AssetTag
3. **Foreign Key Constraints**: Referential integrity
4. **Cascade Delete**: Controlled cascade on relationships
5. **Soft Delete**: IsActive flags for logical deletion

### **Audit Trail**
1. **LoginAudit**: Complete login/logout tracking
2. **AuditEntries**: Entity change tracking
3. **Timestamps**: CreatedAt, UpdatedAt on all tables
4. **User Tracking**: CreatedBy, UpdatedBy fields

### **Access Control**
1. **Role-Based**: Through Roles and Permissions
2. **Location-Based**: RestrictedRegion/State/Plaza fields
3. **Project-Based**: User-project assignments
4. **Permission Overrides**: User-specific permissions

---

## 📊 Indexes Summary

### **Unique Indexes** (for data integrity)
- Users: Username, Email, UserId
- Roles: Name, RoleId
- Permissions: Code, PermissionId
- Projects: Code, ProjectId
- Locations: LocationId
- Assets: AssetTag, AssetId
- SystemSettings: SettingKey

### **Foreign Key Indexes** (for performance)
- All foreign key columns automatically indexed
- Composite indexes on junction tables

### **Query Optimization Indexes**
- LoginAudit: LoginTime, SessionId
- AuditEntries: Timestamp, EntityType
- Assets: ProjectId, LocationId, Status

---

## 💾 Storage Estimates

### **Typical Database Size**
- **Small Organization** (100 users, 1000 assets): ~50 MB
- **Medium Organization** (500 users, 10,000 assets): ~500 MB
- **Large Organization** (2000 users, 50,000 assets): ~2 GB

### **Growth Rate**
- **Assets**: ~2 KB per asset
- **Users**: ~1 KB per user
- **Audit Logs**: ~500 bytes per entry
- **Login Audit**: ~300 bytes per login

---

## 🔧 Maintenance

### **Regular Tasks**
1. **Index Maintenance**: Rebuild fragmented indexes monthly
2. **Statistics Update**: Update statistics weekly
3. **Backup**: Daily full backup, hourly transaction log backup
4. **Audit Cleanup**: Archive old audit logs (>1 year)
5. **Session Cleanup**: Automated via background service

### **Performance Monitoring**
1. Monitor slow queries
2. Check index usage
3. Review execution plans
4. Monitor database size growth
5. Check deadlocks and blocking

---

## 📝 Notes

1. **Alternate Keys**: All major entities have alternate keys (UserId, AssetId, etc.) for user-friendly references
2. **Enums**: Status and UsageCategory stored as integers for performance
3. **Denormalization**: Some fields (Username in audit tables) denormalized for query performance
4. **Future Expansion**: Workflow tables prepared but not yet implemented
5. **Scalability**: Designed to handle 100,000+ assets efficiently

---

**Last Updated**: March 5, 2026
