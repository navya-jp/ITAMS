import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api, User, Permission, Role } from '../services/api';

interface UserWithPermissions extends User {
  rolePermissions: Permission[];
  userOverrides: UserPermissionOverride[];
  effectivePermissions: Permission[];
}

interface UserPermissionOverride {
  id: number;
  userId: number;
  permissionId: number;
  permission: Permission;
  allowed: boolean;
  reason?: string;
  expiresAt?: string;
  grantedBy: string;
  grantedAt: string;
}

interface PermissionGroup {
  module: string;
  permissions: Permission[];
}

@Component({
  selector: 'app-user-permissions',
  imports: [CommonModule, FormsModule],
  templateUrl: './user-permissions.html',
  styleUrl: './user-permissions.scss',
})
export class UserPermissions implements OnInit {
  users: UserWithPermissions[] = [];
  permissions: Permission[] = [];
  roles: Role[] = [];
  permissionGroups: PermissionGroup[] = [];
  
  loading = false;
  error = '';
  success = '';

  // Modal states
  showPermissionsModal = false;
  showOverrideModal = false;
  selectedUser: UserWithPermissions | null = null;

  // Filters
  userFilter = '';
  roleFilter = '';
  statusFilter = 'all';

  // Permission override form
  overrideForm = {
    permissionId: 0,
    allowed: true,
    reason: '',
    expiresAt: ''
  };
  
  // Permission filter mode for override modal
  overridePermissionFilter: 'all' | 'granted' | 'not-granted' = 'all';

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadUsers();
    this.loadPermissions();
    this.loadRoles();
  }

  loadUsers() {
    this.loading = true;
    this.api.getUsers().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.users = response.data.map(user => ({
            ...user,
            rolePermissions: [],
            userOverrides: [],
            effectivePermissions: []
          }));
          this.loadUserPermissions();
        }
      },
      error: (error) => {
        this.error = 'Failed to load users';
        this.loading = false;
        console.error('Error loading users:', error);
      }
    });
  }

  loadPermissions() {
    this.api.getAllPermissions().subscribe({
      next: (permissions) => {
        this.permissions = permissions;
        this.groupPermissionsByModule();
      },
      error: (error) => {
        console.error('Error loading permissions:', error);
      }
    });
  }

  loadRoles() {
    this.api.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
      },
      error: (error) => {
        console.error('Error loading roles:', error);
      }
    });
  }

  loadUserPermissions() {
    const userPromises = this.users.map(async user => {
      try {
        // Load RBAC role permissions (not legacy role permissions)
        const rolePermissions = await this.api.getRbacRolePermissions(user.roleId).toPromise();
        user.rolePermissions = (rolePermissions || []).map((p: any) => ({
          id: p.permissionId,
          name: p.permissionCode,
          code: p.permissionCode,
          description: p.description,
          module: p.module,
          isActive: p.status === 'ACTIVE'
        }));
        
        // TODO: Load user-specific overrides when API is available
        user.userOverrides = [];
        
        // Calculate effective permissions (role + overrides)
        user.effectivePermissions = this.calculateEffectivePermissions(user);
      } catch (error) {
        console.error(`Error loading permissions for user ${user.username}:`, error);
      }
    });

    Promise.all(userPromises).then(() => {
      this.loading = false;
    }).catch(error => {
      this.error = 'Failed to load user permissions';
      this.loading = false;
      console.error('Error loading user permissions:', error);
    });
  }

  calculateEffectivePermissions(user: UserWithPermissions): Permission[] {
    const effectivePermissions = new Map<number, Permission>();
    
    // Start with role permissions
    user.rolePermissions.forEach(permission => {
      effectivePermissions.set(permission.id, permission);
    });
    
    // Apply user overrides
    user.userOverrides.forEach(override => {
      if (override.allowed) {
        effectivePermissions.set(override.permissionId, override.permission);
      } else {
        effectivePermissions.delete(override.permissionId);
      }
    });
    
    return Array.from(effectivePermissions.values());
  }

  groupPermissionsByModule() {
    const groups = new Map<string, Permission[]>();
    
    this.permissions.forEach(permission => {
      if (!groups.has(permission.module)) {
        groups.set(permission.module, []);
      }
      groups.get(permission.module)!.push(permission);
    });

    this.permissionGroups = Array.from(groups.entries()).map(([module, permissions]) => ({
      module,
      permissions: permissions.sort((a, b) => a.name.localeCompare(b.name))
    })).sort((a, b) => a.module.localeCompare(b.module));
  }

  openPermissionsModal(user: UserWithPermissions) {
    this.selectedUser = user;
    this.showPermissionsModal = true;
    this.error = '';
    this.success = '';
  }

  openOverrideModal(user: UserWithPermissions) {
    this.selectedUser = user;
    this.overrideForm = {
      permissionId: 0,
      allowed: true,
      reason: '',
      expiresAt: ''
    };
    this.overridePermissionFilter = 'all'; // Default to showing all permissions
    this.showOverrideModal = true;
    this.error = '';
    this.success = '';
  }

  closeModals() {
    this.showPermissionsModal = false;
    this.showOverrideModal = false;
    this.selectedUser = null;
    this.error = '';
    this.success = '';
  }

  grantPermissionOverride() {
    if (!this.selectedUser || !this.overrideForm.permissionId) {
      this.error = 'Please select a permission';
      return;
    }

    // Convert permissionId to number
    const permissionId = Number(this.overrideForm.permissionId);

    // TODO: Implement API call for granting permission override
    console.log('Granting permission override:', {
      userId: this.selectedUser.id,
      permissionId: permissionId,
      allowed: this.overrideForm.allowed,
      reason: this.overrideForm.reason,
      expiresAt: this.overrideForm.expiresAt
    });

    // For now, simulate the change locally
    // In production, this should call the API and then reload
    const permission = this.permissions.find(p => p.id === permissionId);
    if (!permission) {
      this.error = 'Permission not found';
      return;
    }

    // Find the user in the main array
    const userIndex = this.users.findIndex(u => u.id === this.selectedUser!.id);
    if (userIndex === -1) {
      this.error = 'User not found';
      return;
    }

    // Create a new user object with updated data (immutable update)
    const updatedUser = { ...this.users[userIndex] };
    
    if (this.overrideForm.allowed) {
      // Grant permission - add to effective permissions if not already there
      if (!updatedUser.effectivePermissions.some(p => p.id === permission.id)) {
        updatedUser.effectivePermissions = [...updatedUser.effectivePermissions, permission];
      }
      // Add to overrides (create new array)
      updatedUser.userOverrides = [...updatedUser.userOverrides, {
        id: Date.now(), // temporary ID
        userId: updatedUser.id,
        permissionId: permission.id,
        permission: permission,
        allowed: true,
        reason: this.overrideForm.reason,
        expiresAt: this.overrideForm.expiresAt,
        grantedBy: 'Current User',
        grantedAt: new Date().toISOString()
      }];
    } else {
      // Revoke permission - remove from effective permissions
      updatedUser.effectivePermissions = updatedUser.effectivePermissions.filter(
        p => p.id !== permission.id
      );
      // Add deny override (create new array)
      updatedUser.userOverrides = [...updatedUser.userOverrides, {
        id: Date.now(), // temporary ID
        userId: updatedUser.id,
        permissionId: permission.id,
        permission: permission,
        allowed: false,
        reason: this.overrideForm.reason,
        expiresAt: this.overrideForm.expiresAt,
        grantedBy: 'Current User',
        grantedAt: new Date().toISOString()
      }];
    }
    
    // Update the user in the users array (immutable)
    this.users = [
      ...this.users.slice(0, userIndex),
      updatedUser,
      ...this.users.slice(userIndex + 1)
    ];
    
    // Update selectedUser reference
    this.selectedUser = updatedUser;

    this.success = this.overrideForm.allowed ? 
      'Permission granted successfully' : 
      'Permission revoked successfully';
    this.closeModals();
  }

  revokePermissionOverride(user: UserWithPermissions, overrideId: number) {
    // TODO: Implement API call for revoking permission override
    console.log('Revoking permission override:', { userId: user.id, overrideId });
    
    this.success = 'Permission override revoked successfully';
  }

  getFilteredUsers(): UserWithPermissions[] {
    return this.users.filter(user => {
      const matchesName = !this.userFilter || 
        user.firstName.toLowerCase().includes(this.userFilter.toLowerCase()) ||
        user.lastName.toLowerCase().includes(this.userFilter.toLowerCase()) ||
        user.username.toLowerCase().includes(this.userFilter.toLowerCase()) ||
        user.email.toLowerCase().includes(this.userFilter.toLowerCase());
      
      const matchesRole = !this.roleFilter || user.roleId.toString() === this.roleFilter;
      
      const matchesStatus = this.statusFilter === 'all' || 
        (this.statusFilter === 'active' && user.isActive) ||
        (this.statusFilter === 'inactive' && !user.isActive);
      
      return matchesName && matchesRole && matchesStatus;
    });
  }

  getRoleName(roleId: number): string {
    const role = this.roles.find(r => r.id === roleId);
    return role ? role.name : 'Unknown';
  }

  hasPermissionOverride(user: UserWithPermissions, permissionId: number): UserPermissionOverride | null {
    return user.userOverrides.find(override => override.permissionId === permissionId) || null;
  }

  hasEffectivePermission(user: UserWithPermissions, permissionId: number): boolean {
    return user.effectivePermissions.some(p => p.id === permissionId);
  }

  getPermissionSource(user: UserWithPermissions, permission: Permission): 'role' | 'override-grant' | 'override-deny' | 'none' {
    const override = this.hasPermissionOverride(user, permission.id);
    if (override) {
      return override.allowed ? 'override-grant' : 'override-deny';
    }
    
    const hasRolePermission = user.rolePermissions.some(p => p.id === permission.id);
    if (hasRolePermission) {
      return 'role';
    }
    
    return 'none';
  }

  getAvailablePermissionsForOverride(): Permission[] {
    if (!this.selectedUser) return [];
    
    if (this.overridePermissionFilter === 'granted') {
      // Show only permissions user currently has (from role or override-grant)
      return this.permissions.filter(permission => 
        this.hasEffectivePermission(this.selectedUser!, permission.id)
      );
    } else if (this.overridePermissionFilter === 'not-granted') {
      // Show only permissions user doesn't have
      return this.permissions.filter(permission => 
        !this.hasEffectivePermission(this.selectedUser!, permission.id)
      );
    }
    
    // Show all permissions
    return this.permissions;
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }

  exportUserPermissions() {
    // TODO: Implement export functionality
    console.log('Exporting user permissions...');
  }
}