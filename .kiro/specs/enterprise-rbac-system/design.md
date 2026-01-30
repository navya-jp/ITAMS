# Enterprise RBAC System Design Document

## System Architecture Overview

The Enterprise RBAC System for ITAMS follows a **permission-first architecture** with data immutability, comprehensive audit trails, and enterprise-grade security controls. The system is designed for large organizations requiring strict compliance, audit capabilities, and granular access control.

### Core Architectural Principles

1. **Permission-First Authority**: Roles are templates; permissions are the source of truth
2. **Data Immutability**: No hard deletes; all changes are status-based with full audit trails
3. **Super Admin Supremacy**: Unrestricted control with complete audit logging
4. **Zero Hard-Coding**: All permissions and rules stored in database
5. **Scope-Aware Security**: Global vs Project-specific access enforcement

## Database Schema Design

### Core RBAC Tables

```sql
-- Roles table (templates for default permissions)
CREATE TABLE roles (
    role_id INT PRIMARY KEY IDENTITY(1,1),
    role_name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(500),
    is_system_role BIT NOT NULL DEFAULT 0,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    created_by INT NOT NULL,
    deactivated_at DATETIME2 NULL,
    deactivated_by INT NULL,
    CONSTRAINT FK_roles_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT FK_roles_deactivated_by FOREIGN KEY (deactivated_by) REFERENCES users(id)
);

-- Permissions table (atomic authorization units)
CREATE TABLE permissions (
    permission_id INT PRIMARY KEY IDENTITY(1,1),
    permission_code NVARCHAR(100) NOT NULL UNIQUE,
    module NVARCHAR(50) NOT NULL,
    description NVARCHAR(500) NOT NULL,
    resource_type NVARCHAR(50) NOT NULL,
    action NVARCHAR(50) NOT NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    created_by INT NOT NULL,
    deactivated_at DATETIME2 NULL,
    deactivated_by INT NULL,
    CONSTRAINT FK_permissions_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT FK_permissions_deactivated_by FOREIGN KEY (deactivated_by) REFERENCES users(id)
);

-- Role default permissions matrix
CREATE TABLE role_permissions (
    role_permission_id INT PRIMARY KEY IDENTITY(1,1),
    role_id INT NOT NULL,
    permission_id INT NOT NULL,
    allowed BIT NOT NULL DEFAULT 1,
    granted_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    granted_by INT NOT NULL,
    revoked_at DATETIME2 NULL,
    revoked_by INT NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    CONSTRAINT FK_role_permissions_role FOREIGN KEY (role_id) REFERENCES roles(role_id),
    CONSTRAINT FK_role_permissions_permission FOREIGN KEY (permission_id) REFERENCES permissions(permission_id),
    CONSTRAINT FK_role_permissions_granted_by FOREIGN KEY (granted_by) REFERENCES users(id),
    CONSTRAINT FK_role_permissions_revoked_by FOREIGN KEY (revoked_by) REFERENCES users(id),
    CONSTRAINT UQ_role_permissions UNIQUE (role_id, permission_id)
);

-- User-specific permission overrides
CREATE TABLE user_permissions (
    user_permission_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    permission_id INT NOT NULL,
    allowed BIT NOT NULL,
    granted_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    granted_by INT NOT NULL,
    revoked_at DATETIME2 NULL,
    revoked_by INT NULL,
    reason NVARCHAR(500),
    expires_at DATETIME2 NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    CONSTRAINT FK_user_permissions_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_user_permissions_permission FOREIGN KEY (permission_id) REFERENCES permissions(permission_id),
    CONSTRAINT FK_user_permissions_granted_by FOREIGN KEY (granted_by) REFERENCES users(id),
    CONSTRAINT FK_user_permissions_revoked_by FOREIGN KEY (revoked_by) REFERENCES users(id),
    CONSTRAINT UQ_user_permissions UNIQUE (user_id, permission_id)
);

-- User scope assignments
CREATE TABLE user_scope (
    user_scope_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    scope_type NVARCHAR(20) NOT NULL CHECK (scope_type IN ('GLOBAL', 'PROJECT')),
    project_id INT NULL,
    assigned_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    assigned_by INT NOT NULL,
    removed_at DATETIME2 NULL,
    removed_by INT NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    CONSTRAINT FK_user_scope_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT FK_user_scope_project FOREIGN KEY (project_id) REFERENCES projects(id),
    CONSTRAINT FK_user_scope_assigned_by FOREIGN KEY (assigned_by) REFERENCES users(id),
    CONSTRAINT FK_user_scope_removed_by FOREIGN KEY (removed_by) REFERENCES users(id),
    CONSTRAINT CHK_project_scope CHECK (
        (scope_type = 'GLOBAL' AND project_id IS NULL) OR 
        (scope_type = 'PROJECT' AND project_id IS NOT NULL)
    )
);

-- Comprehensive audit trail
CREATE TABLE permission_audit_log (
    audit_id INT PRIMARY KEY IDENTITY(1,1),
    actor_user_id INT NOT NULL,
    target_user_id INT NULL,
    role_id INT NULL,
    permission_id INT NULL,
    action_type NVARCHAR(50) NOT NULL, -- GRANT, REVOKE, ROLE_ASSIGN, SCOPE_CHANGE, etc.
    entity_type NVARCHAR(50) NOT NULL, -- USER_PERMISSION, ROLE_PERMISSION, USER_SCOPE
    old_value NVARCHAR(MAX),
    new_value NVARCHAR(MAX),
    reason NVARCHAR(500),
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(500),
    session_id NVARCHAR(100),
    timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_audit_actor FOREIGN KEY (actor_user_id) REFERENCES users(id),
    CONSTRAINT FK_audit_target FOREIGN KEY (target_user_id) REFERENCES users(id),
    CONSTRAINT FK_audit_role FOREIGN KEY (role_id) REFERENCES roles(role_id),
    CONSTRAINT FK_audit_permission FOREIGN KEY (permission_id) REFERENCES permissions(permission_id)
);

-- Access attempt logging
CREATE TABLE access_audit_log (
    access_audit_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    permission_code NVARCHAR(100) NOT NULL,
    resource_id NVARCHAR(100),
    resource_type NVARCHAR(50),
    action_attempted NVARCHAR(50),
    access_granted BIT NOT NULL,
    denial_reason NVARCHAR(500),
    ip_address NVARCHAR(45),
    user_agent NVARCHAR(500),
    session_id NVARCHAR(100),
    timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_access_audit_user FOREIGN KEY (user_id) REFERENCES users(id)
);
```

### Enhanced User Table (Data Immutability)

```sql
-- Updated users table with immutability principles
ALTER TABLE users ADD COLUMN status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE';
ALTER TABLE users ADD COLUMN deactivated_at DATETIME2 NULL;
ALTER TABLE users ADD COLUMN deactivated_by INT NULL;
ALTER TABLE users ADD CONSTRAINT FK_users_deactivated_by FOREIGN KEY (deactivated_by) REFERENCES users(id);
ALTER TABLE users ADD CONSTRAINT CHK_user_status CHECK (status IN ('ACTIVE', 'INACTIVE', 'SUSPENDED', 'LOCKED'));
```

## Permission Model Implementation

### Atomic Permission Structure

```typescript
interface Permission {
  permission_id: number;
  permission_code: string; // e.g., "USER_CREATE", "ASSET_VIEW"
  module: string; // e.g., "USER_MANAGEMENT", "ASSET_MASTER"
  description: string;
  resource_type: string; // e.g., "USER", "ASSET", "REPORT"
  action: string; // e.g., "CREATE", "VIEW", "EDIT", "TRANSFER"
  status: 'ACTIVE' | 'INACTIVE';
}
```

### Permission Modules

#### 1. User Management Module
```sql
INSERT INTO permissions (permission_code, module, description, resource_type, action, created_by) VALUES
('USER_CREATE', 'USER_MANAGEMENT', 'Create new users in the system', 'USER', 'CREATE', 1),
('USER_VIEW', 'USER_MANAGEMENT', 'View user profiles and information', 'USER', 'VIEW', 1),
('USER_EDIT', 'USER_MANAGEMENT', 'Edit user profiles and settings', 'USER', 'EDIT', 1),
('USER_DEACTIVATE', 'USER_MANAGEMENT', 'Deactivate user accounts', 'USER', 'DEACTIVATE', 1),
('USER_REACTIVATE', 'USER_MANAGEMENT', 'Reactivate deactivated user accounts', 'USER', 'REACTIVATE', 1),
('ROLE_ASSIGN', 'USER_MANAGEMENT', 'Assign roles to users', 'USER', 'ROLE_ASSIGN', 1),
('PERMISSION_OVERRIDE', 'USER_MANAGEMENT', 'Override user permissions beyond role defaults', 'USER', 'PERMISSION_OVERRIDE', 1);
```

#### 2. Asset Management Module
```sql
INSERT INTO permissions (permission_code, module, description, resource_type, action, created_by) VALUES
('ASSET_CREATE', 'ASSET_MANAGEMENT', 'Create new assets in the system', 'ASSET', 'CREATE', 1),
('ASSET_VIEW', 'ASSET_MANAGEMENT', 'View asset details and information', 'ASSET', 'VIEW', 1),
('ASSET_EDIT', 'ASSET_MANAGEMENT', 'Edit asset information and properties', 'ASSET', 'EDIT', 1),
('ASSET_TRANSFER', 'ASSET_MANAGEMENT', 'Transfer assets between locations/users', 'ASSET', 'TRANSFER', 1),
('ASSET_DECOMMISSION', 'ASSET_MANAGEMENT', 'Decommission assets (soft delete)', 'ASSET', 'DECOMMISSION', 1),
('ASSET_REACTIVATE', 'ASSET_MANAGEMENT', 'Reactivate decommissioned assets', 'ASSET', 'REACTIVATE', 1);
```

#### 3. Lifecycle & Repairs Module
```sql
INSERT INTO permissions (permission_code, module, description, resource_type, action, created_by) VALUES
('LIFECYCLE_LOG', 'LIFECYCLE_REPAIRS', 'Log lifecycle events for assets', 'LIFECYCLE', 'LOG', 1),
('LIFECYCLE_VIEW', 'LIFECYCLE_REPAIRS', 'View asset lifecycle history', 'LIFECYCLE', 'VIEW', 1),
('LIFECYCLE_APPROVE', 'LIFECYCLE_REPAIRS', 'Approve lifecycle transitions', 'LIFECYCLE', 'APPROVE', 1),
('REPAIR_ADD', 'LIFECYCLE_REPAIRS', 'Add repair records for assets', 'REPAIR', 'ADD', 1),
('REPAIR_VIEW', 'LIFECYCLE_REPAIRS', 'View repair history and records', 'REPAIR', 'VIEW', 1),
('MAINTENANCE_SCHEDULE', 'LIFECYCLE_REPAIRS', 'Schedule maintenance activities', 'MAINTENANCE', 'SCHEDULE', 1);
```

#### 4. Reports & Audits Module
```sql
INSERT INTO permissions (permission_code, module, description, resource_type, action, created_by) VALUES
('REPORT_VIEW', 'REPORTS_AUDITS', 'View system reports and dashboards', 'REPORT', 'VIEW', 1),
('REPORT_EXPORT', 'REPORTS_AUDITS', 'Export reports to external formats', 'REPORT', 'EXPORT', 1),
('AUDIT_VIEW', 'REPORTS_AUDITS', 'View audit trails and logs', 'AUDIT', 'VIEW', 1),
('AUDIT_DOWNLOAD', 'REPORTS_AUDITS', 'Download audit data for compliance', 'AUDIT', 'DOWNLOAD', 1),
('FINANCIAL_VIEW', 'REPORTS_AUDITS', 'View financial reports and cost data', 'FINANCIAL', 'VIEW', 1);
```

## Permission Resolution Logic

### Core Resolution Algorithm

```typescript
class PermissionResolver {
  async hasPermission(
    userId: number, 
    permissionCode: string, 
    resourceId?: string,
    projectId?: number
  ): Promise<PermissionResult> {
    
    // Step 1: Check if user is active
    const user = await this.getUserById(userId);
    if (user.status !== 'ACTIVE') {
      return this.denyAccess('USER_INACTIVE', 'User account is not active');
    }

    // Step 2: Check user-specific permission overrides FIRST
    const userOverride = await this.getUserPermissionOverride(userId, permissionCode);
    if (userOverride) {
      if (userOverride.allowed) {
        // Still need to check scope even with override
        const scopeCheck = await this.validateScope(userId, projectId);
        if (!scopeCheck.valid) {
          return this.denyAccess('SCOPE_VIOLATION', scopeCheck.reason);
        }
        return this.grantAccess('USER_OVERRIDE', userOverride);
      } else {
        return this.denyAccess('USER_OVERRIDE_DENIED', 'Explicitly denied by user override');
      }
    }

    // Step 3: Check role-based permissions
    const rolePermission = await this.getRolePermission(user.roleId, permissionCode);
    if (rolePermission && rolePermission.allowed) {
      // Validate scope for role-based permissions
      const scopeCheck = await this.validateScope(userId, projectId);
      if (!scopeCheck.valid) {
        return this.denyAccess('SCOPE_VIOLATION', scopeCheck.reason);
      }
      return this.grantAccess('ROLE_PERMISSION', rolePermission);
    }

    // Step 4: Check if resource is decommissioned/inactive
    if (resourceId) {
      const resourceStatus = await this.getResourceStatus(resourceId);
      if (resourceStatus === 'DECOMMISSIONED' || resourceStatus === 'INACTIVE') {
        return this.denyAccess('RESOURCE_INACTIVE', 'Cannot access inactive/decommissioned resource');
      }
    }

    // Step 5: Default deny
    return this.denyAccess('NO_PERMISSION', 'No explicit permission granted');
  }

  private async validateScope(userId: number, projectId?: number): Promise<ScopeValidation> {
    const userScopes = await this.getUserScopes(userId);
    
    // If user has global scope, allow access to any project
    if (userScopes.some(scope => scope.scope_type === 'GLOBAL' && scope.status === 'ACTIVE')) {
      return { valid: true, reason: 'Global scope granted' };
    }

    // If no project specified and user has project scope, deny
    if (!projectId && userScopes.every(scope => scope.scope_type === 'PROJECT')) {
      return { valid: false, reason: 'Project-specific user accessing global resource' };
    }

    // If project specified, check if user has access to that project
    if (projectId) {
      const hasProjectAccess = userScopes.some(scope => 
        scope.scope_type === 'PROJECT' && 
        scope.project_id === projectId && 
        scope.status === 'ACTIVE'
      );
      
      if (!hasProjectAccess) {
        return { valid: false, reason: `No access to project ${projectId}` };
      }
    }

    return { valid: true, reason: 'Scope validation passed' };
  }
}
```

### Caching Strategy

```typescript
class PermissionCache {
  private cache = new Map<string, CachedPermission>();
  private readonly TTL = 300000; // 5 minutes

  async getPermission(userId: number, permissionCode: string): Promise<PermissionResult | null> {
    const key = `${userId}:${permissionCode}`;
    const cached = this.cache.get(key);
    
    if (cached && (Date.now() - cached.timestamp) < this.TTL) {
      return cached.result;
    }
    
    return null;
  }

  setPermission(userId: number, permissionCode: string, result: PermissionResult): void {
    const key = `${userId}:${permissionCode}`;
    this.cache.set(key, {
      result,
      timestamp: Date.now()
    });
  }

  invalidateUser(userId: number): void {
    for (const [key] of this.cache) {
      if (key.startsWith(`${userId}:`)) {
        this.cache.delete(key);
      }
    }
  }

  invalidatePermission(permissionCode: string): void {
    for (const [key] of this.cache) {
      if (key.endsWith(`:${permissionCode}`)) {
        this.cache.delete(key);
      }
    }
  }
}
```

## Default Role Configuration

### Role Permission Matrix

```sql
-- Super Admin (Global scope, all permissions)
INSERT INTO role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 1, permission_id, 1, 1 FROM permissions WHERE status = 'ACTIVE';

-- Admin (Project scope, user and asset management)
INSERT INTO role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 2, permission_id, 1, 1 FROM permissions 
WHERE permission_code IN (
  'USER_CREATE', 'USER_VIEW', 'USER_EDIT', 'USER_DEACTIVATE', 'ROLE_ASSIGN',
  'ASSET_CREATE', 'ASSET_VIEW', 'ASSET_EDIT', 'ASSET_TRANSFER',
  'LIFECYCLE_LOG', 'LIFECYCLE_VIEW', 'REPAIR_ADD', 'REPAIR_VIEW',
  'REPORT_VIEW', 'REPORT_EXPORT'
);

-- IT Staff (Project scope, asset operations only)
INSERT INTO role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 3, permission_id, 1, 1 FROM permissions 
WHERE permission_code IN (
  'ASSET_VIEW', 'ASSET_EDIT',
  'LIFECYCLE_LOG', 'LIFECYCLE_VIEW', 'REPAIR_ADD', 'REPAIR_VIEW',
  'MAINTENANCE_SCHEDULE'
);

-- Auditor (Global scope, read-only access)
INSERT INTO role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 4, permission_id, 1, 1 FROM permissions 
WHERE action IN ('VIEW', 'DOWNLOAD') AND status = 'ACTIVE';

-- Project Manager (Project scope, read-only dashboards)
INSERT INTO role_permissions (role_id, permission_id, allowed, granted_by)
SELECT 5, permission_id, 1, 1 FROM permissions 
WHERE permission_code IN ('REPORT_VIEW', 'ASSET_VIEW', 'LIFECYCLE_VIEW');
```

## API Design

### Permission Management Endpoints

```typescript
// Role Management
@Controller('api/rbac/roles')
export class RoleController {
  
  @Get()
  @RequirePermission('ROLE_VIEW')
  async getRoles(@Query() filters: RoleFilters): Promise<Role[]> {
    return this.roleService.getRoles(filters);
  }

  @Post()
  @RequirePermission('ROLE_CREATE')
  async createRole(@Body() roleData: CreateRoleDto): Promise<Role> {
    return this.roleService.createRole(roleData);
  }

  @Put(':roleId/permissions')
  @RequirePermission('ROLE_PERMISSION_MANAGE')
  async updateRolePermissions(
    @Param('roleId') roleId: number,
    @Body() permissions: RolePermissionUpdate[]
  ): Promise<void> {
    return this.roleService.updateRolePermissions(roleId, permissions);
  }

  @Patch(':roleId/deactivate')
  @RequirePermission('ROLE_DEACTIVATE')
  async deactivateRole(@Param('roleId') roleId: number): Promise<void> {
    return this.roleService.deactivateRole(roleId);
  }
}

// User Permission Management
@Controller('api/rbac/users')
export class UserPermissionController {
  
  @Get(':userId/permissions')
  @RequirePermission('USER_PERMISSION_VIEW')
  async getUserPermissions(@Param('userId') userId: number): Promise<UserPermissionSummary> {
    return this.permissionService.getUserPermissionSummary(userId);
  }

  @Post(':userId/permissions')
  @RequirePermission('PERMISSION_OVERRIDE')
  async grantUserPermission(
    @Param('userId') userId: number,
    @Body() permissionGrant: PermissionGrantDto
  ): Promise<void> {
    return this.permissionService.grantUserPermission(userId, permissionGrant);
  }

  @Delete(':userId/permissions/:permissionId')
  @RequirePermission('PERMISSION_OVERRIDE')
  async revokeUserPermission(
    @Param('userId') userId: number,
    @Param('permissionId') permissionId: number
  ): Promise<void> {
    return this.permissionService.revokeUserPermission(userId, permissionId);
  }

  @Put(':userId/scope')
  @RequirePermission('USER_SCOPE_MANAGE')
  async updateUserScope(
    @Param('userId') userId: number,
    @Body() scopeUpdate: UserScopeUpdate
  ): Promise<void> {
    return this.permissionService.updateUserScope(userId, scopeUpdate);
  }
}

// Permission Resolution
@Controller('api/rbac/check')
export class PermissionCheckController {
  
  @Post('permission')
  async checkPermission(@Body() request: PermissionCheckRequest): Promise<PermissionResult> {
    return this.permissionResolver.hasPermission(
      request.userId,
      request.permissionCode,
      request.resourceId,
      request.projectId
    );
  }

  @Post('bulk-check')
  async checkMultiplePermissions(@Body() request: BulkPermissionCheckRequest): Promise<BulkPermissionResult> {
    return this.permissionResolver.checkMultiplePermissions(request);
  }
}

// Audit Trail
@Controller('api/rbac/audit')
export class AuditController {
  
  @Get('permissions')
  @RequirePermission('AUDIT_VIEW')
  async getPermissionAuditLog(@Query() filters: AuditFilters): Promise<PaginatedAuditLog> {
    return this.auditService.getPermissionAuditLog(filters);
  }

  @Get('access')
  @RequirePermission('AUDIT_VIEW')
  async getAccessAuditLog(@Query() filters: AccessAuditFilters): Promise<PaginatedAccessLog> {
    return this.auditService.getAccessAuditLog(filters);
  }

  @Get('export')
  @RequirePermission('AUDIT_DOWNLOAD')
  async exportAuditData(@Query() filters: AuditExportFilters): Promise<StreamableFile> {
    return this.auditService.exportAuditData(filters);
  }
}
```

### Middleware for Permission Enforcement

```typescript
@Injectable()
export class PermissionGuard implements CanActivate {
  constructor(
    private permissionResolver: PermissionResolver,
    private auditService: AuditService
  ) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const request = context.switchToHttp().getRequest();
    const user = request.user;
    
    if (!user) {
      return false;
    }

    const requiredPermission = this.getRequiredPermission(context);
    if (!requiredPermission) {
      return true; // No permission required
    }

    const resourceId = request.params.id || request.body.resourceId;
    const projectId = request.params.projectId || request.body.projectId;

    const permissionResult = await this.permissionResolver.hasPermission(
      user.id,
      requiredPermission,
      resourceId,
      projectId
    );

    // Log access attempt
    await this.auditService.logAccessAttempt({
      userId: user.id,
      permissionCode: requiredPermission,
      resourceId,
      resourceType: this.getResourceType(context),
      actionAttempted: this.getActionAttempted(context),
      accessGranted: permissionResult.granted,
      denialReason: permissionResult.reason,
      ipAddress: request.ip,
      userAgent: request.headers['user-agent'],
      sessionId: request.sessionID
    });

    return permissionResult.granted;
  }

  private getRequiredPermission(context: ExecutionContext): string | null {
    const handler = context.getHandler();
    const classRef = context.getClass();
    
    return Reflect.getMetadata('permission', handler) || 
           Reflect.getMetadata('permission', classRef);
  }
}

// Decorator for permission requirements
export const RequirePermission = (permission: string) => {
  return SetMetadata('permission', permission);
};
```

## UI/UX Design Components

### Role Permission Matrix Component

```typescript
@Component({
  selector: 'app-role-permission-matrix',
  template: `
    <div class="permission-matrix">
      <h3>Role Permission Matrix: {{ role.name }}</h3>
      
      <div class="matrix-container">
        <table class="permission-table">
          <thead>
            <tr>
              <th>Module</th>
              <th>Permission</th>
              <th>Description</th>
              <th>Allowed</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let permission of permissions" 
                [class.modified]="isPermissionModified(permission)">
              <td>{{ permission.module }}</td>
              <td>{{ permission.permission_code }}</td>
              <td>{{ permission.description }}</td>
              <td>
                <input type="checkbox" 
                       [checked]="isPermissionAllowed(permission)"
                       (change)="togglePermission(permission, $event)"
                       [disabled]="!canModifyPermissions">
              </td>
              <td>
                <button *ngIf="isPermissionModified(permission)"
                        class="btn btn-sm btn-warning"
                        (click)="revertPermission(permission)">
                  Revert
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="matrix-actions">
        <button class="btn btn-primary" 
                (click)="saveChanges()"
                [disabled]="!hasChanges">
          Save Changes
        </button>
        <button class="btn btn-secondary" 
                (click)="resetChanges()">
          Reset All
        </button>
      </div>
    </div>
  `
})
export class RolePermissionMatrixComponent {
  @Input() role: Role;
  @Input() permissions: Permission[];
  @Input() canModifyPermissions: boolean = false;

  private originalPermissions: Map<number, boolean> = new Map();
  private modifiedPermissions: Map<number, boolean> = new Map();

  ngOnInit() {
    this.loadRolePermissions();
  }

  isPermissionAllowed(permission: Permission): boolean {
    return this.modifiedPermissions.get(permission.permission_id) ?? 
           this.originalPermissions.get(permission.permission_id) ?? false;
  }

  isPermissionModified(permission: Permission): boolean {
    const original = this.originalPermissions.get(permission.permission_id);
    const current = this.modifiedPermissions.get(permission.permission_id);
    return current !== undefined && current !== original;
  }

  togglePermission(permission: Permission, event: any): void {
    this.modifiedPermissions.set(permission.permission_id, event.target.checked);
  }

  get hasChanges(): boolean {
    return this.modifiedPermissions.size > 0;
  }
}
```

### User Permission Override Panel

```typescript
@Component({
  selector: 'app-user-permission-override',
  template: `
    <div class="permission-override-panel">
      <h4>Permission Overrides for {{ user.firstName }} {{ user.lastName }}</h4>
      
      <div class="user-info">
        <span class="badge badge-info">Role: {{ user.roleName }}</span>
        <span class="badge badge-secondary">Scope: {{ user.scope }}</span>
      </div>

      <div class="override-section">
        <h5>Active Overrides</h5>
        <div *ngIf="userOverrides.length === 0" class="no-overrides">
          No permission overrides for this user.
        </div>
        
        <div *ngFor="let override of userOverrides" class="override-item">
          <div class="override-info">
            <span class="permission-code">{{ override.permission_code }}</span>
            <span class="permission-desc">{{ override.description }}</span>
            <span class="badge" 
                  [class.badge-success]="override.allowed"
                  [class.badge-danger]="!override.allowed">
              {{ override.allowed ? 'GRANTED' : 'DENIED' }}
            </span>
          </div>
          <div class="override-meta">
            <small>Granted by {{ override.granted_by_name }} on {{ override.granted_at | date }}</small>
            <button class="btn btn-sm btn-outline-danger" 
                    (click)="revokeOverride(override)">
              Revoke
            </button>
          </div>
        </div>
      </div>

      <div class="add-override-section">
        <h5>Grant New Override</h5>
        <form [formGroup]="overrideForm" (ngSubmit)="grantOverride()">
          <div class="form-group">
            <label>Permission</label>
            <select formControlName="permissionId" class="form-control">
              <option value="">Select permission...</option>
              <option *ngFor="let permission of availablePermissions" 
                      [value]="permission.permission_id">
                {{ permission.permission_code }} - {{ permission.description }}
              </option>
            </select>
          </div>
          
          <div class="form-group">
            <label>Access Type</label>
            <div class="radio-group">
              <label>
                <input type="radio" formControlName="allowed" [value]="true">
                Grant Access
              </label>
              <label>
                <input type="radio" formControlName="allowed" [value]="false">
                Deny Access
              </label>
            </div>
          </div>

          <div class="form-group">
            <label>Reason</label>
            <textarea formControlName="reason" 
                      class="form-control" 
                      placeholder="Reason for this override..."></textarea>
          </div>

          <div class="form-group">
            <label>Expires At (Optional)</label>
            <input type="datetime-local" 
                   formControlName="expiresAt" 
                   class="form-control">
          </div>

          <button type="submit" 
                  class="btn btn-primary"
                  [disabled]="overrideForm.invalid">
            Grant Override
          </button>
        </form>
      </div>
    </div>
  `
})
export class UserPermissionOverrideComponent {
  @Input() user: User;
  
  userOverrides: UserPermissionOverride[] = [];
  availablePermissions: Permission[] = [];
  overrideForm: FormGroup;

  constructor(
    private permissionService: PermissionService,
    private fb: FormBuilder
  ) {
    this.overrideForm = this.fb.group({
      permissionId: ['', Validators.required],
      allowed: [true, Validators.required],
      reason: ['', Validators.required],
      expiresAt: ['']
    });
  }

  async grantOverride(): Promise<void> {
    if (this.overrideForm.valid) {
      const override = this.overrideForm.value;
      await this.permissionService.grantUserPermission(this.user.id, override);
      await this.loadUserOverrides();
      this.overrideForm.reset();
    }
  }
}
```

## Security Best Practices

### 1. Input Validation and Sanitization

```typescript
export class PermissionValidationPipe implements PipeTransform {
  transform(value: any, metadata: ArgumentMetadata): any {
    if (metadata.type === 'body' && metadata.metatype === PermissionGrantDto) {
      // Validate permission codes against whitelist
      if (!this.isValidPermissionCode(value.permissionCode)) {
        throw new BadRequestException('Invalid permission code');
      }
      
      // Sanitize reason field
      value.reason = this.sanitizeInput(value.reason);
      
      // Validate expiration date
      if (value.expiresAt && new Date(value.expiresAt) <= new Date()) {
        throw new BadRequestException('Expiration date must be in the future');
      }
    }
    
    return value;
  }

  private isValidPermissionCode(code: string): boolean {
    return /^[A-Z_]+$/.test(code) && code.length <= 100;
  }

  private sanitizeInput(input: string): string {
    return input?.replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '');
  }
}
```

### 2. Rate Limiting for Permission Operations

```typescript
@Injectable()
export class PermissionRateLimitGuard implements CanActivate {
  private attempts = new Map<string, number[]>();
  private readonly maxAttempts = 100; // per hour
  private readonly windowMs = 3600000; // 1 hour

  canActivate(context: ExecutionContext): boolean {
    const request = context.switchToHttp().getRequest();
    const key = `${request.user.id}:${request.ip}`;
    
    const now = Date.now();
    const userAttempts = this.attempts.get(key) || [];
    
    // Remove old attempts outside the window
    const recentAttempts = userAttempts.filter(time => now - time < this.windowMs);
    
    if (recentAttempts.length >= this.maxAttempts) {
      throw new TooManyRequestsException('Permission operation rate limit exceeded');
    }
    
    recentAttempts.push(now);
    this.attempts.set(key, recentAttempts);
    
    return true;
  }
}
```

### 3. Encryption for Sensitive Data

```typescript
@Injectable()
export class PermissionEncryptionService {
  private readonly algorithm = 'aes-256-gcm';
  private readonly key = Buffer.from(process.env.PERMISSION_ENCRYPTION_KEY, 'hex');

  encryptSensitiveData(data: string): EncryptedData {
    const iv = crypto.randomBytes(16);
    const cipher = crypto.createCipher(this.algorithm, this.key);
    cipher.setAAD(Buffer.from('permission-data'));
    
    let encrypted = cipher.update(data, 'utf8', 'hex');
    encrypted += cipher.final('hex');
    
    const authTag = cipher.getAuthTag();
    
    return {
      encrypted,
      iv: iv.toString('hex'),
      authTag: authTag.toString('hex')
    };
  }

  decryptSensitiveData(encryptedData: EncryptedData): string {
    const decipher = crypto.createDecipher(this.algorithm, this.key);
    decipher.setAAD(Buffer.from('permission-data'));
    decipher.setAuthTag(Buffer.from(encryptedData.authTag, 'hex'));
    
    let decrypted = decipher.update(encryptedData.encrypted, 'hex', 'utf8');
    decrypted += decipher.final('utf8');
    
    return decrypted;
  }
}
```

## Data Immutability Implementation

### Soft Delete Service

```typescript
@Injectable()
export class SoftDeleteService {
  
  async deactivateUser(userId: number, deactivatedBy: number, reason: string): Promise<void> {
    await this.db.query(`
      UPDATE users 
      SET status = 'INACTIVE',
          deactivated_at = GETUTCDATE(),
          deactivated_by = @deactivatedBy
      WHERE id = @userId AND status = 'ACTIVE'
    `, { userId, deactivatedBy });

    await this.auditService.logAction({
      actorUserId: deactivatedBy,
      targetUserId: userId,
      actionType: 'USER_DEACTIVATE',
      entityType: 'USER',
      oldValue: 'ACTIVE',
      newValue: 'INACTIVE',
      reason
    });
  }

  async decommissionAsset(assetId: number, decommissionedBy: number, reason: string): Promise<void> {
    await this.db.query(`
      UPDATE assets 
      SET status = 'DECOMMISSIONED',
          decommissioned_at = GETUTCDATE(),
          decommissioned_by = @decommissionedBy
      WHERE id = @assetId AND status != 'DECOMMISSIONED'
    `, { assetId, decommissionedBy });

    await this.auditService.logAction({
      actorUserId: decommissionedBy,
      entityType: 'ASSET',
      actionType: 'ASSET_DECOMMISSION',
      resourceId: assetId.toString(),
      newValue: 'DECOMMISSIONED',
      reason
    });
  }

  async reactivateUser(userId: number, reactivatedBy: number, reason: string): Promise<void> {
    await this.db.query(`
      UPDATE users 
      SET status = 'ACTIVE',
          deactivated_at = NULL,
          deactivated_by = NULL
      WHERE id = @userId AND status = 'INACTIVE'
    `, { userId, reactivatedBy });

    await this.auditService.logAction({
      actorUserId: reactivatedBy,
      targetUserId: userId,
      actionType: 'USER_REACTIVATE',
      entityType: 'USER',
      oldValue: 'INACTIVE',
      newValue: 'ACTIVE',
      reason
    });
  }
}
```

## Compliance and Audit Features

### Comprehensive Audit Service

```typescript
@Injectable()
export class ComplianceAuditService {
  
  async generateComplianceReport(filters: ComplianceFilters): Promise<ComplianceReport> {
    const report: ComplianceReport = {
      generatedAt: new Date(),
      generatedBy: filters.requestedBy,
      period: filters.period,
      sections: []
    };

    // Permission Changes Section
    const permissionChanges = await this.getPermissionChanges(filters);
    report.sections.push({
      title: 'Permission Changes',
      data: permissionChanges,
      summary: {
        totalChanges: permissionChanges.length,
        grantsCount: permissionChanges.filter(c => c.action_type === 'GRANT').length,
        revocationsCount: permissionChanges.filter(c => c.action_type === 'REVOKE').length
      }
    });

    // Access Violations Section
    const violations = await this.getAccessViolations(filters);
    report.sections.push({
      title: 'Access Violations',
      data: violations,
      summary: {
        totalViolations: violations.length,
        uniqueUsers: new Set(violations.map(v => v.user_id)).size,
        criticalViolations: violations.filter(v => v.severity === 'CRITICAL').length
      }
    });

    // Super Admin Actions Section
    const superAdminActions = await this.getSuperAdminActions(filters);
    report.sections.push({
      title: 'Super Admin Actions',
      data: superAdminActions,
      summary: {
        totalActions: superAdminActions.length,
        uniqueAdmins: new Set(superAdminActions.map(a => a.actor_user_id)).size
      }
    });

    return report;
  }

  async validateDataIntegrity(): Promise<IntegrityReport> {
    const issues: IntegrityIssue[] = [];

    // Check for orphaned permissions
    const orphanedPermissions = await this.db.query(`
      SELECT up.user_permission_id, up.user_id, up.permission_id
      FROM user_permissions up
      LEFT JOIN users u ON up.user_id = u.id
      LEFT JOIN permissions p ON up.permission_id = p.permission_id
      WHERE u.id IS NULL OR p.permission_id IS NULL
    `);

    if (orphanedPermissions.length > 0) {
      issues.push({
        type: 'ORPHANED_PERMISSIONS',
        severity: 'HIGH',
        count: orphanedPermissions.length,
        description: 'User permissions referencing non-existent users or permissions'
      });
    }

    // Check for users without roles
    const usersWithoutRoles = await this.db.query(`
      SELECT u.id, u.username
      FROM users u
      LEFT JOIN roles r ON u.role_id = r.role_id
      WHERE r.role_id IS NULL AND u.status = 'ACTIVE'
    `);

    if (usersWithoutRoles.length > 0) {
      issues.push({
        type: 'USERS_WITHOUT_ROLES',
        severity: 'MEDIUM',
        count: usersWithoutRoles.length,
        description: 'Active users without valid role assignments'
      });
    }

    return {
      checkedAt: new Date(),
      issuesFound: issues.length,
      issues
    };
  }
}
```

This enterprise RBAC system provides:

1. **Complete Data Immutability** - No hard deletes, all changes tracked
2. **Permission-First Architecture** - Roles are templates, permissions are authority
3. **Super Admin Supremacy** - Unrestricted control with full audit trails
4. **Comprehensive Audit System** - Every change logged for compliance
5. **Granular Scope Control** - Global vs Project-specific access
6. **Enterprise Security** - Rate limiting, encryption, input validation
7. **Scalable Performance** - Caching, optimized queries, efficient resolution
8. **Compliance Ready** - Built for banking/government audit requirements

The system is designed to handle thousands of users with sub-second permission resolution while maintaining complete audit trails for regulatory compliance.