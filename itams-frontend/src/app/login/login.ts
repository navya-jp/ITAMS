import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService, LoginRequest } from '../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  loginData: LoginRequest = {
    username: '',
    password: ''
  };

  loading = false;
  error = '';
  message = '';
  showPassword = false;
  
  // Login attempt tracking
  attemptCount = 0;
  maxAttempts = 5;
  isLocked = false;
  lockoutEnd: Date | null = null;
  // Track if this is a first login attempt
  isFirstLoginAttempt = false;
  checkingUserStatus = false;
  
  // Timer for lockout countdown
  private lockoutTimer: any;

  constructor(
    protected authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Check if already authenticated
    if (this.authService.isAuthenticated) {
      this.redirectBasedOnRole();
      return;
    }

    // Check for messages from query params
    this.route.queryParams.subscribe(params => {
      if (params['message']) {
        this.message = params['message'];
      }
      if (params['error']) {
        this.error = params['error'];
      }
    });

    // Load security settings
    this.maxAttempts = this.authService.settings.maxLoginAttempts;
    
    // Check if user is locked out
    this.checkLockoutStatus();
  }

  ngOnDestroy() {
    if (this.lockoutTimer) {
      clearInterval(this.lockoutTimer);
    }
    if (this.usernameCheckTimeout) {
      clearTimeout(this.usernameCheckTimeout);
    }
  }

  onLogin() {
    if (!this.validateForm()) {
      return;
    }

    if (this.isLocked) {
      this.error = 'Account is locked. Please try again later.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.authService.login(this.loginData).subscribe({
      next: (response) => {
        this.loading = false;
        
        if (response.success) {
          // Reset attempt count on successful login
          this.resetAttemptCount();
          
          // Handle different login scenarios
          if (response.isFirstLogin || response.requiresPasswordChange) {
            this.router.navigate(['/change-password'], {
              queryParams: { 
                firstLogin: response.isFirstLogin,
                required: response.requiresPasswordChange
              }
            });
          } else {
            this.redirectBasedOnRole();
          }
        } else {
          this.handleLoginFailure(response);
        }
      },
      error: (error) => {
        this.loading = false;
        this.handleLoginError(error);
      }
    });
  }

  private validateForm(): boolean {
    if (!this.loginData.username.trim()) {
      this.error = 'Username is required';
      return false;
    }

    if (!this.loginData.password) {
      this.error = 'Password is required';
      return false;
    }

    if (this.loginData.username.length < 3) {
      this.error = 'Username must be at least 3 characters';
      return false;
    }

    return true;
  }

  private handleLoginFailure(response: any) {
    this.incrementAttemptCount();
    
    if (response.lockoutInfo?.isLocked) {
      this.handleLockout(response.lockoutInfo);
    } else {
      const remaining = this.maxAttempts - this.attemptCount;
      if (remaining > 0) {
        this.error = `${response.message || 'Invalid credentials'}. ${remaining} attempt(s) remaining.`;
      } else {
        this.handleLockout({
          isLocked: true,
          lockoutEnd: new Date(Date.now() + (this.authService.settings.lockoutDurationMinutes * 60 * 1000)).toISOString()
        });
      }
    }
  }

  private handleLoginError(error: any) {
    if (error.status === 423) {
      // Account locked
      this.handleLockout(error.error?.lockoutInfo);
    } else if (error.status === 401) {
      this.handleLoginFailure(error.error);
    } else {
      this.error = 'Login failed. Please try again later.';
    }
  }

  private handleLockout(lockoutInfo: any) {
    this.isLocked = true;
    
    if (lockoutInfo?.lockoutEnd) {
      this.lockoutEnd = new Date(lockoutInfo.lockoutEnd);
      this.startLockoutTimer();
      this.error = `Account locked until ${this.lockoutEnd.toLocaleString()}`;
    } else {
      this.error = `Account locked for ${this.authService.settings.lockoutDurationMinutes} minutes`;
    }
  }

  private incrementAttemptCount() {
    this.attemptCount++;
    localStorage.setItem('login_attempts', this.attemptCount.toString());
    localStorage.setItem('last_attempt', Date.now().toString());
  }

  private resetAttemptCount() {
    this.attemptCount = 0;
    localStorage.removeItem('login_attempts');
    localStorage.removeItem('last_attempt');
    localStorage.removeItem('lockout_end');
  }

  private checkLockoutStatus() {
    const attempts = localStorage.getItem('login_attempts');
    const lockoutEnd = localStorage.getItem('lockout_end');
    
    if (attempts) {
      this.attemptCount = parseInt(attempts);
    }

    if (lockoutEnd) {
      const lockoutTime = new Date(lockoutEnd);
      if (lockoutTime > new Date()) {
        this.isLocked = true;
        this.lockoutEnd = lockoutTime;
        this.startLockoutTimer();
        this.error = `Account locked until ${lockoutTime.toLocaleString()}`;
      } else {
        // Lockout expired
        this.resetAttemptCount();
      }
    }
  }

  private startLockoutTimer() {
    this.lockoutTimer = setInterval(() => {
      if (this.lockoutEnd && new Date() >= this.lockoutEnd) {
        this.isLocked = false;
        this.lockoutEnd = null;
        this.resetAttemptCount();
        this.error = '';
        this.message = 'Account unlocked. You may now try logging in again.';
        clearInterval(this.lockoutTimer);
      }
    }, 1000);
  }

  private redirectBasedOnRole() {
    const user = this.authService.currentUser;
    if (user?.roleName === 'Super Admin') {
      this.router.navigate(['/admin/dashboard']);
    } else {
      this.router.navigate(['/user/dashboard']);
    }
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  onPasswordInput() {
    // No password strength checking during login
    // Password strength is only for password setup/change
  }

  onUsernameChange() {
    this.clearMessages();
    this.checkUserStatus();
  }

  checkUserStatus() {
    const username = this.loginData.username?.trim();
    
    if (!username || username.length < 3) {
      this.isFirstLoginAttempt = false;
      return;
    }

    // Debounce the check
    clearTimeout(this.usernameCheckTimeout);
    this.usernameCheckTimeout = setTimeout(() => {
      this.checkIfFirstLogin(username);
    }, 500);
  }

  private usernameCheckTimeout: any;

  checkIfFirstLogin(username: string) {
    this.checkingUserStatus = true;
    
    // Make a simple API call to check user status
    this.authService.checkUserStatus(username).subscribe({
      next: (response) => {
        this.isFirstLoginAttempt = response.isFirstLogin || false;
        this.checkingUserStatus = false;
      },
      error: (error) => {
        // If user doesn't exist or error, assume it's not first login
        this.isFirstLoginAttempt = false;
        this.checkingUserStatus = false;
      }
    });
  }

  getPasswordLabel(): string {
    return this.isFirstLoginAttempt ? 'Create New Password' : 'Password';
  }

  getPasswordPlaceholder(): string {
    return this.isFirstLoginAttempt ? 'Create your new password' : 'Enter your password';
  }

  clearMessages() {
    this.error = '';
    this.message = '';
  }

  onForgotPassword() {
    if (!this.loginData.username.trim()) {
      this.error = 'Please enter your username first, then click "Forgot Password"';
      return;
    }

    // Show message about contacting Super Admin
    this.message = `Password reset request noted for "${this.loginData.username}". Please contact your Super Admin who can reset your password in the User Management section.`;
    this.loginData.username = ''; // Clear username for security
  }

  getLockoutTimeRemaining(): string {
    if (!this.lockoutEnd) return '';
    
    const now = new Date();
    const diff = this.lockoutEnd.getTime() - now.getTime();
    
    if (diff <= 0) return '';
    
    const minutes = Math.floor(diff / 60000);
    const seconds = Math.floor((diff % 60000) / 1000);
    
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }
}