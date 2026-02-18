import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api, User, CreateUser, UpdateUser, Role, ApiResponse } from '../services/api';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-users',
  imports: [CommonModule, FormsModule],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users implements OnInit {
  users: User[] = [];
  filteredUsers: User[] = [];
  roles: Role[] = [];
  projects: any[] = []; // List of projects for assignment
  loading = false;
  error = '';
  success = '';
  currentUsername = '';

  // Search and filter
  searchTerm = '';
  filterRole = 0; // 0 = All Roles
  filterStatus = 'all'; // 'all', 'active', 'inactive'

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  selectedUser: User | null = null;
  showLocationRestrictions = false;
  showEditLocationRestrictions = false;

  // Form data
  createForm: CreateUser = {
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    roleId: 0,
    password: '',
    mustChangePassword: true,
    projectId: 0,
    restrictedRegion: '',
    restrictedState: '',
    restrictedPlaza: '',
    restrictedOffice: ''
  };

  editForm: UpdateUser = {
    email: '',
    firstName: '',
    lastName: '',
    roleId: 0,
    isActive: true,
    projectId: undefined,
    restrictedRegion: '',
    restrictedState: '',
    restrictedPlaza: '',
    restrictedOffice: ''
  };

  // Password validation
  passwordRequirements = {
    minLength: false,
    hasUppercase: false,
    hasLowercase: false,
    hasNumber: false,
    hasSpecial: false
  };

  // Username validation and availability
  isUsernameValid = false;
  usernameAvailable: boolean | null = null;
  checkingUsername = false;
  private usernameCheckTimeout: any;
  userHasEditedUsername = false;

  constructor(private api: Api, private authService: AuthService) {}

  ngOnInit() {
    // Get current user
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.currentUsername = user.username;
      }
    });
    
    this.loadUsers();
    this.loadRoles();
    this.loadProjects();
  }

  loadProjects() {
    this.api.getProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
      },
      error: (error) => {
        console.error('Error loading projects:', error);
      }
    });
  }

  loadUsers() {
    this.loading = true;
    this.api.getUsers().subscribe({
      next: (response: ApiResponse<User[]>) => {
        if (response.success && response.data) {
          this.users = response.data;
          this.applyFilters();
        } else {
          this.error = response.message || 'Failed to load users';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load users';
        this.loading = false;
        console.error('Error loading users:', error);
      }
    });
  }

  applyFilters() {
    this.filteredUsers = this.users.filter(user => {
      // Search filter
      const matchesSearch = !this.searchTerm || 
        user.username.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.firstName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.lastName.toLowerCase().includes(this.searchTerm.toLowerCase());

      // Role filter
      const matchesRole = this.filterRole === 0 || user.roleId === this.filterRole;

      // Status filter
      const matchesStatus = this.filterStatus === 'all' ||
        (this.filterStatus === 'active' && user.isActive) ||
        (this.filterStatus === 'inactive' && !user.isActive);

      return matchesSearch && matchesRole && matchesStatus;
    });
  }

  onSearchChange() {
    this.applyFilters();
  }

  onRoleFilterChange() {
    this.applyFilters();
  }

  onStatusFilterChange() {
    this.applyFilters();
  }

  clearFilters() {
    this.searchTerm = '';
    this.filterRole = 0;
    this.filterStatus = 'all';
    this.applyFilters();
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
      mustChangePassword: true,
      projectId: 0,
      restrictedRegion: '',
      restrictedState: '',
      restrictedPlaza: '',
      restrictedOffice: ''
    };
    this.resetPasswordRequirements();
    this.resetUsernameValidation();
    this.userHasEditedUsername = false; // Reset the flag
    this.showCreateModal = true;
    this.error = '';
    this.success = '';
    
    // Initialize password validation to show all requirements as invalid
    this.validatePassword();
  }

  openEditModal(user: User) {
    this.selectedUser = user;
    this.editForm = {
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      roleId: user.roleId,
      isActive: user.isActive,
      projectId: user.projectId || undefined,
      restrictedRegion: user.restrictedRegion || '',
      restrictedState: user.restrictedState || '',
      restrictedPlaza: user.restrictedPlaza || '',
      restrictedOffice: user.restrictedOffice || ''
    };
    
    // Auto-expand location restrictions if any are set
    this.showEditLocationRestrictions = !!(
      this.editForm.restrictedRegion || 
      this.editForm.restrictedState || 
      this.editForm.restrictedPlaza || 
      this.editForm.restrictedOffice
    );
    
    this.showEditModal = true;
    this.error = '';
    this.success = '';
  }

  closeModals() {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.selectedUser = null;
    this.showLocationRestrictions = false;
    this.showEditLocationRestrictions = false;
    this.error = '';
    this.success = '';
  }

  toggleLocationRestrictions() {
    this.showLocationRestrictions = !this.showLocationRestrictions;
  }

  toggleEditLocationRestrictions() {
    this.showEditLocationRestrictions = !this.showEditLocationRestrictions;
  }

  // Generate username from name (for button click)
  generateUsernameFromName() {
    if (this.createForm.firstName && this.createForm.lastName) {
      const username = (this.createForm.firstName.toLowerCase() + '.' + this.createForm.lastName.toLowerCase())
        .replace(/[^a-z0-9.]/g, '');
      this.createForm.username = username;
      this.onUsernameChange();
    }
  }

  // Get the base username (firstname.lastname)
  getBaseUsername(): string {
    if (this.createForm.firstName && this.createForm.lastName) {
      return (this.createForm.firstName.toLowerCase() + '.' + this.createForm.lastName.toLowerCase())
        .replace(/[^a-z0-9.]/g, '');
    }
    return '';
  }

  // Handle name changes - auto-update username
  onNameChange() {
    const baseUsername = this.getBaseUsername();
    // Preserve any suffix the user added
    const currentUsername = this.createForm.username || '';
    const currentBase = this.getBaseUsername();
    
    if (currentUsername.startsWith(currentBase)) {
      // Keep the suffix
      const suffix = currentUsername.substring(currentBase.length);
      this.createForm.username = baseUsername + suffix;
    } else {
      // Reset to base
      this.createForm.username = baseUsername;
    }
    
    this.onUsernameChange();
  }

  // Handle username input - prevent erasing base
  onUsernameInput(event: any) {
    const baseUsername = this.getBaseUsername();
    const currentValue = event.target.value;
    
    // If user tries to erase the base, restore it
    if (baseUsername && !currentValue.startsWith(baseUsername)) {
      // Find what they're trying to add
      if (currentValue.length < baseUsername.length) {
        // They're trying to delete the base - prevent it
        this.createForm.username = baseUsername;
        event.target.value = baseUsername;
      } else {
        // They're typing something that doesn't match - reset to base + their addition
        const suffix = currentValue.replace(/[^0-9]/g, ''); // Only allow numbers as suffix
        this.createForm.username = baseUsername + suffix;
        event.target.value = baseUsername + suffix;
      }
    } else {
      // They're adding to the base - only allow numbers
      const suffix = currentValue.substring(baseUsername.length).replace(/[^0-9]/g, '');
      this.createForm.username = baseUsername + suffix;
      event.target.value = baseUsername + suffix;
    }
    
    this.onUsernameChange();
  }

  // Handle keydown to prevent deleting base username
  onUsernameKeydown(event: KeyboardEvent) {
    const baseUsername = this.getBaseUsername();
    const input = event.target as HTMLInputElement;
    const cursorPosition = input.selectionStart || 0;
    
    // Prevent backspace/delete if it would affect the base username
    if ((event.key === 'Backspace' || event.key === 'Delete') && cursorPosition <= baseUsername.length) {
      event.preventDefault();
      // Move cursor to end of base username
      setTimeout(() => {
        input.setSelectionRange(baseUsername.length, baseUsername.length);
      }, 0);
    }
  }

  // Username validation and availability checking
  onUsernameChange() {
    this.validateUsername();
    
    // Clear previous timeout
    if (this.usernameCheckTimeout) {
      clearTimeout(this.usernameCheckTimeout);
    }
    
    // Reset availability status
    this.usernameAvailable = null;
    
    // Only check availability if username is valid
    if (this.isUsernameValid && this.createForm.username.length >= 3) {
      // Debounce the availability check
      this.usernameCheckTimeout = setTimeout(() => {
        this.checkUsernameAvailability();
      }, 500);
    }
  }

  validateUsername() {
    const username = this.createForm.username;
    // Username must be 3-50 characters, alphanumeric, dots, and underscores only
    const usernameRegex = /^[a-zA-Z0-9._]{3,50}$/;
    this.isUsernameValid = usernameRegex.test(username);
  }

  checkUsernameAvailability() {
    if (!this.isUsernameValid || !this.createForm.username) {
      return;
    }

    this.checkingUsername = true;
    this.api.checkUsernameAvailability(this.createForm.username).subscribe({
      next: (response: ApiResponse<boolean>) => {
        this.usernameAvailable = response.success && response.data === true;
        this.checkingUsername = false;
      },
      error: (error) => {
        console.error('Error checking username availability:', error);
        this.usernameAvailable = null;
        this.checkingUsername = false;
      }
    });
  }

  resetUsernameValidation() {
    this.isUsernameValid = false;
    this.usernameAvailable = null;
    this.checkingUsername = false;
    if (this.usernameCheckTimeout) {
      clearTimeout(this.usernameCheckTimeout);
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
    console.log('Password validation:', this.passwordRequirements); // Debug log
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

  onCreateUserClick() {
    // Simple validation check
    if (!this.createForm.firstName || !this.createForm.lastName || !this.createForm.username || 
        !this.createForm.email || !this.createForm.password || this.createForm.roleId === 0) {
      this.error = 'Please fill in all required fields';
      return;
    }
    
    // Convert roleId and projectId to numbers (HTML forms return strings)
    this.createForm.roleId = Number(this.createForm.roleId);
    if (this.createForm.projectId) {
      this.createForm.projectId = Number(this.createForm.projectId);
    }
    
    console.log('Creating user with data:', JSON.stringify(this.createForm)); // Debug log
    
    this.createUser();
  }

  createUser() {
    this.loading = true;
    this.error = '';
    
    this.api.createUser(this.createForm).subscribe({
      next: (response: ApiResponse<User>) => {
        if (response.success && response.data) {
          this.users.push(response.data);
          this.success = response.message || 'User created successfully';
          this.loading = false;
          this.closeModals(); // Automatically close the modal
        } else {
          this.error = response.message || 'Failed to create user';
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Create user error:', error);
        console.log('Error details:', error.error); // Add more detailed logging
        
        if (error.error?.validationErrors) {
          this.error = Object.values(error.error.validationErrors).flat().join(', ');
        } else if (error.error?.message) {
          this.error = error.error.message;
        } else if (error.error?.Message) {
          this.error = error.error.Message;
        } else if (error.message) {
          this.error = error.message;
        } else {
          this.error = 'Failed to create user. Please check all fields and try again.';
        }
        this.loading = false;
      }
    });
  }

  updateUser() {
    if (!this.selectedUser) return;

    this.loading = true;
    this.api.updateUser(this.selectedUser.id, this.editForm).subscribe({
      next: (response: ApiResponse<User>) => {
        if (response.success && response.data) {
          const index = this.users.findIndex(u => u.id === this.selectedUser!.id);
          if (index !== -1) {
            this.users[index] = response.data;
          }
          this.success = response.message || 'User updated successfully';
          this.loading = false;
          this.closeModals();
        } else {
          this.error = response.message || 'Failed to update user';
          this.loading = false;
        }
      },
      error: (error) => {
        if (error.error?.validationErrors) {
          this.error = Object.values(error.error.validationErrors).flat().join(', ');
        } else {
          this.error = error.error?.message || 'Failed to update user';
        }
        this.loading = false;
        console.error('Error updating user:', error);
      }
    });
  }

  // Deactivate user instead of delete
  deactivateUser(user: User) {
    if (confirm(`Are you sure you want to deactivate user ${user.username}?`)) {
      this.loading = true;
      this.api.deactivateUser(user.id).subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success) {
            const index = this.users.findIndex(u => u.id === user.id);
            if (index !== -1) {
              this.users[index].isActive = false;
            }
            this.success = response.message || 'User deactivated successfully';
            this.loading = false;
          } else {
            this.error = response.message || 'Failed to deactivate user';
            this.loading = false;
          }
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
    this.api.activateUser(user.id).subscribe({
      next: (response: ApiResponse<any>) => {
        if (response.success) {
          const index = this.users.findIndex(u => u.id === user.id);
          if (index !== -1) {
            this.users[index].isActive = true;
          }
          this.success = response.message || 'User activated successfully';
          this.loading = false;
        } else {
          this.error = response.message || 'Failed to activate user';
          this.loading = false;
        }
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to activate user';
        this.loading = false;
        console.error('Error activating user:', error);
      }
    });
  }

  lockUser(user: User) {
    if (confirm(`Are you sure you want to lock user ${user.username}?`)) {
      this.loading = true;
      this.api.lockUser(user.id).subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success) {
            const index = this.users.findIndex(u => u.id === user.id);
            if (index !== -1) {
              this.users[index].isLocked = true;
            }
            this.success = response.message || 'User locked successfully';
            this.loading = false;
          } else {
            this.error = response.message || 'Failed to lock user';
            this.loading = false;
          }
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to lock user';
          this.loading = false;
          console.error('Error locking user:', error);
        }
      });
    }
  }

  unlockUser(user: User) {
    this.loading = true;
    this.api.unlockUser(user.id).subscribe({
      next: (response: ApiResponse<any>) => {
        if (response.success) {
          const index = this.users.findIndex(u => u.id === user.id);
          if (index !== -1) {
            this.users[index].isLocked = false;
          }
          this.success = response.message || 'User unlocked successfully';
          this.loading = false;
        } else {
          this.error = response.message || 'Failed to unlock user';
          this.loading = false;
        }
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to unlock user';
        this.loading = false;
        console.error('Error unlocking user:', error);
      }
    });
  }

  resetPassword(user: User) {
    const newPassword = prompt('Enter new password for ' + user.username + ':');
    if (newPassword) {
      this.loading = true;
      this.api.resetPassword(user.id, newPassword).subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success) {
            this.success = response.message || 'Password reset successfully';
            this.loading = false;
          } else {
            this.error = response.message || 'Failed to reset password';
            this.loading = false;
          }
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to reset password';
          this.loading = false;
          console.error('Error resetting password:', error);
        }
      });
    }
  }

  isFormValid(): boolean {
    return this.createForm.username.length >= 3 &&
           this.isUsernameValid &&
           this.usernameAvailable === true &&
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

  isUserOnline(user: User): boolean {
    // Check if this is the currently logged-in user
    if (this.currentUsername && this.currentUsername === user.username) {
      return true;
    }
    
    if (!user.lastLoginAt) {
      return false;
    }
    
    // Consider a user online if their last login was within the last 30 minutes
    const lastLogin = new Date(user.lastLoginAt).getTime();
    const now = Date.now();
    const thirtyMinutes = 30 * 60 * 1000;
    const timeSinceLogin = now - lastLogin;
    
    return timeSinceLogin < thirtyMinutes;
  }
}