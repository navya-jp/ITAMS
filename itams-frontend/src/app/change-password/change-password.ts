import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService, ChangePasswordRequest } from '../services/auth.service';

@Component({
  selector: 'app-change-password',
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password.html',
  styleUrl: './change-password.scss',
})
export class ChangePassword implements OnInit {
  passwordForm: ChangePasswordRequest = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  loading = false;
  error = '';
  success = '';
  
  // Component state
  isFirstLogin = false;
  isRequired = false;
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  // Password validation
  passwordStrength = {
    score: 0,
    feedback: '',
    color: 'danger',
    requirements: {
      length: false,
      uppercase: false,
      lowercase: false,
      number: false,
      special: false
    }
  };

  // Password requirements
  passwordRequirements = [
    { key: 'length', label: 'At least 8 characters', icon: 'fas fa-ruler' },
    { key: 'uppercase', label: 'One uppercase letter (A-Z)', icon: 'fas fa-font' },
    { key: 'lowercase', label: 'One lowercase letter (a-z)', icon: 'fas fa-font' },
    { key: 'number', label: 'One number (0-9)', icon: 'fas fa-hashtag' },
    { key: 'special', label: 'One special character (!@#$%^&*)', icon: 'fas fa-asterisk' }
  ];

  constructor(
    protected authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Check authentication
    if (!this.authService.isAuthenticated) {
      this.router.navigate(['/login']);
      return;
    }

    // Get query parameters
    this.route.queryParams.subscribe(params => {
      this.isFirstLogin = params['firstLogin'] === 'true';
      this.isRequired = params['required'] === 'true';
    });

    // Don't redirect - allow users to request password reset
    // Only first-time users will see the password creation form
    // Regular users will see the "Request Password Reset" button
  }

  onChangePassword() {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    this.authService.changePassword(this.passwordForm).subscribe({
      next: (response) => {
        this.loading = false;
        this.success = this.isFirstLogin ? 'Password created successfully!' : 'Password changed successfully!';
        
        // Redirect after successful password change
        setTimeout(() => {
          this.redirectBasedOnRole();
        }, 2000);
      },
      error: (error) => {
        this.loading = false;
        if (error.status === 400) {
          this.error = error.error?.message || 'Invalid current password or password requirements not met';
        } else if (error.status === 422) {
          this.error = 'Password validation failed. Please check the requirements.';
        } else {
          this.error = 'Failed to change password. Please try again.';
        }
      }
    });
  }

  onRequestPasswordReset() {
    const user = this.authService.currentUser;
    if (!user) {
      this.error = 'User not authenticated';
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    this.authService.requestPasswordReset(user.username).subscribe({
      next: (response) => {
        this.loading = false;
        this.success = `Password reset request sent for "${user.username}". Your Super Admin has been notified and will reset your password shortly. You can continue using the system until then.`;
        
        // Don't redirect immediately - let them read the message
        // They can click Cancel or Logout
      },
      error: (error) => {
        this.loading = false;
        this.error = 'Failed to send password reset request. Please try again or contact your administrator directly.';
      }
    });
  }

  private validateForm(): boolean {
    // Current password validation (skip for first login)
    if (!this.isFirstLogin && !this.passwordForm.currentPassword) {
      this.error = 'Current password is required';
      return false;
    }

    // New password validation
    if (!this.passwordForm.newPassword) {
      this.error = 'New password is required';
      return false;
    }

    if (this.passwordStrength.score < 5) {
      this.error = 'Password does not meet all requirements';
      return false;
    }

    // Confirm password validation
    if (!this.passwordForm.confirmPassword) {
      this.error = 'Please confirm your new password';
      return false;
    }

    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      this.error = 'New password and confirmation do not match';
      return false;
    }

    // Check if new password is same as current (for non-first login)
    if (!this.isFirstLogin && this.passwordForm.currentPassword === this.passwordForm.newPassword) {
      this.error = 'New password must be different from current password';
      return false;
    }

    return true;
  }

  onNewPasswordInput() {
    this.checkPasswordStrength(this.passwordForm.newPassword);
    this.clearMessages();
  }

  onConfirmPasswordInput() {
    this.clearMessages();
  }

  private checkPasswordStrength(password: string) {
    const requirements = {
      length: password.length >= 8,
      uppercase: /[A-Z]/.test(password),
      lowercase: /[a-z]/.test(password),
      number: /\d/.test(password),
      special: /[!@#$%^&*(),.?":{}|<>]/.test(password)
    };

    this.passwordStrength.requirements = requirements;
    
    const score = Object.values(requirements).filter(Boolean).length;
    this.passwordStrength.score = score;

    // Update feedback and color
    if (score === 0) {
      this.passwordStrength.color = 'danger';
      this.passwordStrength.feedback = 'Enter a password';
    } else if (score <= 2) {
      this.passwordStrength.color = 'danger';
      this.passwordStrength.feedback = 'Weak password';
    } else if (score <= 3) {
      this.passwordStrength.color = 'warning';
      this.passwordStrength.feedback = 'Fair password';
    } else if (score <= 4) {
      this.passwordStrength.color = 'info';
      this.passwordStrength.feedback = 'Good password';
    } else {
      this.passwordStrength.color = 'success';
      this.passwordStrength.feedback = 'Strong password';
    }
  }

  togglePasswordVisibility(field: 'current' | 'new' | 'confirm') {
    switch (field) {
      case 'current':
        this.showCurrentPassword = !this.showCurrentPassword;
        break;
      case 'new':
        this.showNewPassword = !this.showNewPassword;
        break;
      case 'confirm':
        this.showConfirmPassword = !this.showConfirmPassword;
        break;
    }
  }

  private redirectBasedOnRole() {
    const user = this.authService.currentUser;
    if (user?.roleName === 'Super Admin') {
      this.router.navigate(['/admin/dashboard']);
    } else {
      this.router.navigate(['/user/dashboard']);
    }
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }

  onCancel() {
    if (this.isFirstLogin || this.isRequired) {
      // Cannot cancel mandatory password change
      this.error = 'Password change is required to continue';
      return;
    }
    
    this.redirectBasedOnRole();
  }

  onLogout() {
    this.authService.logout('Password change cancelled');
  }

  getRequirementIcon(requirement: any): string {
    const key = requirement.key as keyof typeof this.passwordStrength.requirements;
    const isMet = this.passwordStrength.requirements[key];
    return isMet ? 'fas fa-check-circle text-success' : 'fas fa-times-circle text-danger';
  }

  getRequirementClass(key: string): string {
    const reqKey = key as keyof typeof this.passwordStrength.requirements;
    return this.passwordStrength.requirements[reqKey] ? 'text-success' : 'text-muted';
  }

  getPasswordMatchStatus(): string {
    if (!this.passwordForm.confirmPassword) return '';
    
    if (this.passwordForm.newPassword === this.passwordForm.confirmPassword) {
      return 'text-success';
    } else {
      return 'text-danger';
    }
  }

  getPasswordMatchIcon(): string {
    if (!this.passwordForm.confirmPassword) return '';
    
    if (this.passwordForm.newPassword === this.passwordForm.confirmPassword) {
      return 'fas fa-check-circle text-success';
    } else {
      return 'fas fa-times-circle text-danger';
    }
  }
}