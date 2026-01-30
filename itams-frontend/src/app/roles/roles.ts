import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api, Role, Permission } from '../services/api';

interface RoleWithPermissions extends Role {
  permissions: Permission[];
}

interface PermissionGroup {
  module: string;
  permissions: Permission[];
}

@Component({
  selector: 'app-roles',
  imports: [CommonModule, FormsModule],
  templateUrl: './roles.html',
  styleUrl: './roles.scss',
})
export class Roles implements OnInit {
  roles: RoleWithPermissions[] = [];
  permissions: Permission[] = [];
  permissionGroups: PermissionGroup[] = [];
  loading = false;
  error = '';
  success = '';

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  showPermissionsModal = false;
  selectedRole: RoleWithPermissions | null = null;

  // Form data
  createForm = {
    name: '',
    description: '',
    isSystemRole: false
  };

  editForm = {
    name: '',
    description: '',
    isActive: true
  };

  // Permission management
  selectedPermissions: Set<number> = new Set();
  permissionFilter = '';

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadRbacRoles();
    this.loadRbacPermissions();
  }

  loadRbacRoles() {
    this.loading = true;
    this.api.getRbacRoles().subscribe({
      next: (roles) => {
        this.roles = roles.map(role => ({ 
          id: role.roleId,
          name: role.roleName,
          description: role.description,
          isSystemRole: role.isSystemRole,
          isActive: role.status === 'ACTIVE',
          createdAt: role.createdAt,
          permissions: [] 
        }));
        this.loadRolePermissions();
      },
      error: (error) => {
        this.error = 'Failed to load RBAC roles';
        this.loading = false;
        console.error('Error loading RBAC roles:', error);
      }
    });
  }

  loadRbacPermissions() {
    this.api.getRbacPermissionsGrouped().subscribe({
      next: (groupedPermissions) => {
        this.permissionGroups = groupedPermissions.map(group => ({
          module: group.module,
          permissions: group.permissions.map((p: any) => ({
            id: p.permissionId,
            name: this.formatPermissionName(p.permissionCode),
            code: p.permissionCode,
            description: p.description,
            module: p.module,
            isActive: p.status === 'ACTIVE'
          }))
        }));
        
        // Flatten permissions for easy access
        this.permissions = this.permissionGroups.flatMap(group => group.permissions);
      },
      error: (error) => {
        console.error('Error loading RBAC permissions:', error);
      }
    });
  }

  loadRolePermissions() {
    let completedRoles = 0;
    
    this.roles.forEach(role => {
      this.api.getRbacRolePermissions(role.id).subscribe({
        next: (permissions) => {
          role.permissions = permissions.map((p: any) => ({
            id: p.permissionId,
            name: this.formatPermissionName(p.permissionCode),
            code: p.permissionCode,
            description: p.description,
            module: p.module,
            isActive: p.status === 'ACTIVE'
          }));
          
          completedRoles++;
          if (completedRoles === this.roles.length) {
            this.loading = false;
          }
        },
        error: (error) => {
          console.error(`Error loading permissions for role ${role.name}:`, error);
          completedRoles++;
          if (completedRoles === this.roles.length) {
            this.loading = false;
          }
        }
      });
    });
  }

  formatPermissionName(code: string): string {
    // Convert permission codes like "USER_CREATE" to "Create Users"
    const parts = code.split('_');
    if (parts.length >= 2) {
      const action = parts[parts.length - 1];
      const resource = parts.slice(0, -1).join(' ');
      
      const actionMap: { [key: string]: string } = {
        'CREATE': 'Create',
        'VIEW': 'View',
        'EDIT': 'Edit',
        'DELETE': 'Delete',
        'DEACTIVATE': 'Deactivate',
        'REACTIVATE': 'Reactivate',
        'TRANSFER': 'Transfer',
        'APPROVE': 'Approve',
        'EXPORT': 'Export',
        'DOWNLOAD': 'Download',
        'LOG': 'Log',
        'SCHEDULE': 'Schedule',
        'ASSIGN': 'Assign',
        'OVERRIDE': 'Override'
      };
      
      const resourceMap: { [key: string]: string } = {
        'USER': 'Users',
        'ASSET': 'Assets',
        'LIFECYCLE': 'Lifecycle',
        'REPAIR': 'Repairs',
        'MAINTENANCE': 'Maintenance',
        'REPORT': 'Reports',
        'AUDIT': 'Audit',
        'FINANCIAL': 'Financial',
        'ROLE': 'Roles',
        'PERMISSION': 'Permissions'
      };
      
      const formattedAction = actionMap[action] || action;
      const formattedResource = resourceMap[resource] || resource.replace('_', ' ');
      
      return `${formattedAction} ${formattedResource}`;
    }
    
    return code.replace(/_/g, ' ');
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

  openCreateModal() {
    this.createForm = {
      name: '',
      description: '',
      isSystemRole: false
    };
    this.showCreateModal = true;
    this.error = '';
    this.success = '';
  }

  openEditModal(role: RoleWithPermissions) {
    this.selectedRole = role;
    this.editForm = {
      name: role.name,
      description: role.description || '',
      isActive: role.isActive
    };
    this.showEditModal = true;
    this.error = '';
    this.success = '';
  }

  openPermissionsModal(role: RoleWithPermissions) {
    this.selectedRole = role;
    this.selectedPermissions = new Set(role.permissions.map(p => p.id));
    this.showPermissionsModal = true;
    this.error = '';
    this.success = '';
  }

  closeModals() {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.showPermissionsModal = false;
    this.selectedRole = null;
    this.selectedPermissions.clear();
    this.error = '';
    this.success = '';
  }

  createRole() {
    if (!this.isCreateFormValid()) {
      this.error = 'Please fill all required fields correctly';
      return;
    }

    this.loading = true;
    this.api.createRole(this.createForm).subscribe({
      next: (role) => {
        this.roles.push({ ...role, permissions: [] });
        this.success = 'Role created successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to create role';
        this.loading = false;
        console.error('Error creating role:', error);
      }
    });
  }

  updateRole() {
    if (!this.selectedRole) return;

    this.loading = true;
    this.api.updateRole(this.selectedRole.id, this.editForm).subscribe({
      next: (updatedRole) => {
        const index = this.roles.findIndex(r => r.id === this.selectedRole!.id);
        if (index !== -1) {
          this.roles[index] = { ...updatedRole, permissions: this.roles[index].permissions };
        }
        this.success = 'Role updated successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to update role';
        this.loading = false;
        console.error('Error updating role:', error);
      }
    });
  }

  updateRolePermissions() {
    if (!this.selectedRole) return;

    const permissionIds = Array.from(this.selectedPermissions);
    this.loading = true;
    
    this.api.updateRbacRolePermissions(this.selectedRole.id, permissionIds).subscribe({
      next: () => {
        // Update local role permissions
        this.selectedRole!.permissions = this.permissions.filter(p => 
          this.selectedPermissions.has(p.id)
        );
        this.success = 'Role permissions updated successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to update role permissions';
        this.loading = false;
        console.error('Error updating role permissions:', error);
      }
    });
  }

  togglePermission(permissionId: number) {
    if (this.selectedPermissions.has(permissionId)) {
      this.selectedPermissions.delete(permissionId);
    } else {
      this.selectedPermissions.add(permissionId);
    }
  }

  toggleModulePermissions(module: string, grant: boolean) {
    const modulePermissions = this.permissionGroups.find(g => g.module === module)?.permissions || [];
    
    modulePermissions.forEach(permission => {
      if (grant) {
        this.selectedPermissions.add(permission.id);
      } else {
        this.selectedPermissions.delete(permission.id);
      }
    });
  }

  isModuleFullyGranted(module: string): boolean {
    const modulePermissions = this.permissionGroups.find(g => g.module === module)?.permissions || [];
    return modulePermissions.every(p => this.selectedPermissions.has(p.id));
  }

  isModulePartiallyGranted(module: string): boolean {
    const modulePermissions = this.permissionGroups.find(g => g.module === module)?.permissions || [];
    return modulePermissions.some(p => this.selectedPermissions.has(p.id)) && 
           !this.isModuleFullyGranted(module);
  }

  getFilteredPermissionGroups(): PermissionGroup[] {
    if (!this.permissionFilter) return this.permissionGroups;
    
    const filter = this.permissionFilter.toLowerCase();
    return this.permissionGroups.map(group => ({
      ...group,
      permissions: group.permissions.filter(p => 
        p.name.toLowerCase().includes(filter) ||
        p.description?.toLowerCase().includes(filter) ||
        p.code.toLowerCase().includes(filter)
      )
    })).filter(group => group.permissions.length > 0);
  }

  isCreateFormValid(): boolean {
    return this.createForm.name.length >= 2;
  }

  getRolePermissionCount(role: RoleWithPermissions): number {
    return role.permissions.length;
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }
}