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
    this.loadRoles();
    this.loadPermissions();
  }

  loadRoles() {
    this.loading = true;
    this.api.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles.map(role => ({ ...role, permissions: [] }));
        this.loadRolePermissions();
      },
      error: (error) => {
        this.error = 'Failed to load roles';
        this.loading = false;
        console.error('Error loading roles:', error);
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

  loadRolePermissions() {
    const rolePromises = this.roles.map(role => 
      this.api.getRolePermissions(role.id).then(permissions => {
        role.permissions = permissions;
      })
    );

    Promise.all(rolePromises).then(() => {
      this.loading = false;
    }).catch(error => {
      this.error = 'Failed to load role permissions';
      this.loading = false;
      console.error('Error loading role permissions:', error);
    });
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
    
    this.api.updateRolePermissions(this.selectedRole.id, permissionIds).subscribe({
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