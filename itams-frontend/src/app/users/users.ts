import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api, User, CreateUser, Role } from '../services/api';

@Component({
  selector: 'app-users',
  imports: [CommonModule, FormsModule],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users implements OnInit {
  users: User[] = [];
  roles: Role[] = [];
  loading = false;
  error = '';
  success = '';

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  selectedUser: User | null = null;

  // Form data
  createForm: CreateUser = {
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    roleId: 0,
    password: '',
    mustChangePassword: true
  };

  editForm: Partial<User> = {};

  // Password validation
  passwordRequirements = {
    minLength: false,
    hasUppercase: false,
    hasLowercase: false,
    hasNumber: false,
    hasSpecial: false
  };

  // Automated dropdown options (only for meaningful selections)
  emailDomains = ['@company.com', '@itams.com', '@organization.com'];

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadUsers();
    this.loadRoles();
  }

  loadUsers() {
    this.loading = true;
    this.api.getUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load users';
        this.loading = false;
        console.error('Error loading users:', error);
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

  openCreateModal() {
    this.createForm = {
      username: '',
      email: '',
      firstName: '',
      lastName: '',
      roleId: 0,
      password: '',
      mustChangePassword: true
    };
    this.resetPasswordRequirements();
    this.showCreateModal = true;
    this.error = '';
    this.success = '';
  }

  openEditModal(user: User) {
    this.selectedUser = user;
    this.editForm = {
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      roleId: user.roleId,
      isActive: user.isActive
    };
    this.showEditModal = true;
    this.error = '';
    this.success = '';
  }

  closeModals() {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.selectedUser = null;
    this.error = '';
    this.success = '';
  }

  // Auto-generate username from first and last name
  generateUsername() {
    if (this.createForm.firstName && this.createForm.lastName) {
      const username = (this.createForm.firstName.toLowerCase() + '.' + this.createForm.lastName.toLowerCase())
        .replace(/[^a-z0-9.]/g, '');
      this.createForm.username = username;
    }
  }

  // Auto-complete email with domain
  completeEmail(domain: string) {
    if (this.createForm.username) {
      this.createForm.email = this.createForm.username + domain;
    }
  }

  validatePassword() {
    const password = this.createForm.password;
    this.passwordRequirements = {
      minLength: password.length >= 8,
      hasUppercase: /[A-Z]/.test(password),
      hasLowercase: /[a-z]/.test(password),
      hasNumber: /\d/.test(password),
      hasSpecial: /[@$!%*?&]/.test(password)
    };
  }

  isPasswordValid(): boolean {
    return Object.values(this.passwordRequirements).every(req => req);
  }

  validateEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  resetPasswordRequirements() {
    this.passwordRequirements = {
      minLength: false,
      hasUppercase: false,
      hasLowercase: false,
      hasNumber: false,
      hasSpecial: false
    };
  }

  createUser() {
    if (!this.isFormValid()) {
      this.error = 'Please fill all required fields correctly';
      return;
    }

    this.loading = true;
    this.api.createUser(this.createForm).subscribe({
      next: (user) => {
        this.users.push(user);
        this.success = 'User created successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to create user';
        this.loading = false;
        console.error('Error creating user:', error);
      }
    });
  }

  updateUser() {
    if (!this.selectedUser) return;

    this.loading = true;
    this.api.updateUser(this.selectedUser.id, this.editForm).subscribe({
      next: (updatedUser) => {
        const index = this.users.findIndex(u => u.id === this.selectedUser!.id);
        if (index !== -1) {
          this.users[index] = updatedUser;
        }
        this.success = 'User updated successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to update user';
        this.loading = false;
        console.error('Error updating user:', error);
      }
    });
  }

  // Deactivate user instead of delete
  deactivateUser(user: User) {
    if (confirm(`Are you sure you want to deactivate user ${user.username}?`)) {
      this.loading = true;
      this.api.updateUser(user.id, { isActive: false }).subscribe({
        next: (updatedUser) => {
          const index = this.users.findIndex(u => u.id === user.id);
          if (index !== -1) {
            this.users[index] = updatedUser;
          }
          this.success = 'User deactivated successfully';
          this.loading = false;
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to deactivate user';
          this.loading = false;
          console.error('Error deactivating user:', error);
        }
      });
    }
  }

  // Activate user
  activateUser(user: User) {
    this.loading = true;
    this.api.updateUser(user.id, { isActive: true }).subscribe({
      next: (updatedUser) => {
        const index = this.users.findIndex(u => u.id === user.id);
        if (index !== -1) {
          this.users[index] = updatedUser;
        }
        this.success = 'User activated successfully';
        this.loading = false;
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to activate user';
        this.loading = false;
        console.error('Error activating user:', error);
      }
    });
  }

  lockUser(user: User) {
    this.api.lockUser(user.id).subscribe({
      next: () => {
        user.isLocked = true;
        this.success = 'User locked successfully';
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to lock user';
        console.error('Error locking user:', error);
      }
    });
  }

  unlockUser(user: User) {
    this.api.unlockUser(user.id).subscribe({
      next: () => {
        user.isLocked = false;
        this.success = 'User unlocked successfully';
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to unlock user';
        console.error('Error unlocking user:', error);
      }
    });
  }

  resetPassword(user: User) {
    const newPassword = prompt('Enter new password:');
    if (!newPassword) return;

    this.api.resetPassword(user.id, newPassword).subscribe({
      next: () => {
        this.success = 'Password reset successfully';
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to reset password';
        console.error('Error resetting password:', error);
      }
    });
  }

  isFormValid(): boolean {
    return this.createForm.username.length >= 3 &&
           this.validateEmail(this.createForm.email) &&
           this.createForm.firstName.length > 0 &&
           this.createForm.lastName.length > 0 &&
           this.createForm.roleId > 0 &&
           this.isPasswordValid();
  }

  getRoleName(roleId: number): string {
    const role = this.roles.find(r => r.id === roleId);
    return role ? role.name : 'Unknown';
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }
}
