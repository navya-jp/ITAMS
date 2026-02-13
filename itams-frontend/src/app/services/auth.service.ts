import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Router } from '@angular/router';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  token?: string;
  user?: User;
  message?: string;
  requiresPasswordChange?: boolean;
  isFirstLogin?: boolean;
  lockoutInfo?: {
    isLocked: boolean;
    lockoutEnd?: string;
    attemptsRemaining?: number;
  };
}

export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  roleId: number;
  roleName: string;
  isActive: boolean;
  mustChangePassword: boolean;
  isFirstLogin: boolean;
  lastLoginAt?: string;
  sessionId?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface SecuritySettings {
  maxLoginAttempts: number;
  lockoutDurationMinutes: number;
  sessionTimeoutMinutes: number;
  passwordExpiryDays: number;
  requirePasswordChange: boolean;
  allowMultipleSessions: boolean;
  autoLogoutWarningMinutes: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = 'http://localhost:5066/api/auth';
  private readonly httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  // Authentication state
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  // Session management
  private sessionTimer: any;
  private warningTimer: any;
  private activityTimer: any;
  private lastActivity = Date.now();

  // Security settings (loaded from backend)
  private securitySettings: SecuritySettings = {
    maxLoginAttempts: 5,
    lockoutDurationMinutes: 30,
    sessionTimeoutMinutes: 30,
    passwordExpiryDays: 90,
    requirePasswordChange: true,
    allowMultipleSessions: false,
    autoLogoutWarningMinutes: 5
  };

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeAuth();
    this.setupActivityTracking();
  }

  private initializeAuth() {
    const token = localStorage.getItem('auth_token');
    const user = localStorage.getItem('current_user');
    
    if (token && user) {
      try {
        const userData = JSON.parse(user);
        this.currentUserSubject.next(userData);
        this.isAuthenticatedSubject.next(true);
        this.startSessionTimer();
        this.validateSession();
      } catch (error) {
        this.logout();
      }
    }
  }

  private setupActivityTracking() {
    // Track user activity
    const events = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'];
    
    events.forEach(event => {
      document.addEventListener(event, () => {
        this.lastActivity = Date.now();
      }, true);
    });

    // Check activity every minute
    this.activityTimer = setInterval(() => {
      this.checkSessionTimeout();
    }, 60000); // Check every minute
  }

  private checkSessionTimeout() {
    const now = Date.now();
    const timeoutMs = this.securitySettings.sessionTimeoutMinutes * 60 * 1000;
    const warningMs = this.securitySettings.autoLogoutWarningMinutes * 60 * 1000;
    
    if (this.isAuthenticated && (now - this.lastActivity) > timeoutMs) {
      this.logout('Session expired due to inactivity');
    } else if (this.isAuthenticated && (now - this.lastActivity) > (timeoutMs - warningMs)) {
      this.showAutoLogoutWarning();
    }
  }

  private showAutoLogoutWarning() {
    // TODO: Show warning modal/toast
    const remainingMinutes = Math.ceil((this.securitySettings.sessionTimeoutMinutes * 60 * 1000 - (Date.now() - this.lastActivity)) / 60000);
    console.warn(`Session will expire in ${remainingMinutes} minutes due to inactivity`);
  }

  private startSessionTimer() {
    this.clearTimers();
    
    const timeoutMs = this.securitySettings.sessionTimeoutMinutes * 60 * 1000;
    this.sessionTimer = setTimeout(() => {
      this.logout('Session expired');
    }, timeoutMs);
  }

  private clearTimers() {
    if (this.sessionTimer) {
      clearTimeout(this.sessionTimer);
    }
    if (this.warningTimer) {
      clearTimeout(this.warningTimer);
    }
  }

  // Public methods
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return new Observable(observer => {
      this.http.post<LoginResponse>(`${this.baseUrl}/login`, credentials, this.httpOptions)
        .subscribe({
          next: (response) => {
            if (response.success && response.token && response.user) {
              // Store authentication data
              localStorage.setItem('auth_token', response.token);
              localStorage.setItem('current_user', JSON.stringify(response.user));
              
              // Update state
              this.currentUserSubject.next(response.user);
              this.isAuthenticatedSubject.next(true);
              
              // Start session management
              this.startSessionTimer();
              this.lastActivity = Date.now();
              
              // Load security settings
              this.loadSecuritySettings();
            }
            observer.next(response);
            observer.complete();
          },
          error: (error) => {
            observer.error(error);
          }
        });
    });
  }

  logout(reason?: string): void {
    // Clear timers
    this.clearTimers();
    if (this.activityTimer) {
      clearInterval(this.activityTimer);
    }

    // Notify backend about logout with userId
    const currentUser = this.currentUser;
    const token = localStorage.getItem('auth_token');
    if (token && currentUser) {
      this.http.post(`${this.baseUrl}/logout`, 
        { userId: currentUser.id }, 
        {
          headers: { 
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}` 
          }
        }
      ).subscribe({
        next: () => console.log('Logout successful'),
        error: (err) => console.error('Logout error:', err)
      });
    }

    // Clear local storage
    localStorage.removeItem('auth_token');
    localStorage.removeItem('current_user');
    localStorage.removeItem('security_settings');

    // Update state
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);

    // Navigate to login
    this.router.navigate(['/login'], { 
      queryParams: reason ? { message: reason } : {} 
    });
  }

  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/change-password`, request, this.getAuthHeaders());
  }

  validateSession(): Observable<any> {
    return new Observable(observer => {
      this.http.get(`${this.baseUrl}/validate-session`, this.getAuthHeaders())
        .subscribe({
          next: (response) => {
            observer.next(response);
            observer.complete();
          },
          error: (error) => {
            if (error.status === 401) {
              this.logout('Session invalid');
            }
            observer.error(error);
          }
        });
    });
  }

  refreshToken(): Observable<any> {
    return this.http.post(`${this.baseUrl}/refresh-token`, {}, this.getAuthHeaders());
  }

  checkUserStatus(username: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/check-user-status/${encodeURIComponent(username)}`, this.httpOptions);
  }

  loadSecuritySettings(): void {
    this.http.get<SecuritySettings>(`${this.baseUrl}/security-settings`, this.getAuthHeaders())
      .subscribe({
        next: (settings) => {
          this.securitySettings = settings;
          localStorage.setItem('security_settings', JSON.stringify(settings));
        },
        error: (error) => {
          console.error('Failed to load security settings:', error);
        }
      });
  }

  private getAuthHeaders() {
    const token = localStorage.getItem('auth_token');
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      })
    };
  }

  // Getters
  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  get token(): string | null {
    return localStorage.getItem('auth_token');
  }

  get settings(): SecuritySettings {
    const stored = localStorage.getItem('security_settings');
    return stored ? JSON.parse(stored) : this.securitySettings;
  }

  // Role-based access
  hasRole(roleName: string): boolean {
    return this.currentUser?.roleName === roleName;
  }

  isSuperAdmin(): boolean {
    return this.hasRole('Super Admin');
  }

  isUser(): boolean {
    return !this.isSuperAdmin();
  }

  // Activity tracking
  updateActivity(): void {
    this.lastActivity = Date.now();
  }

  getLastActivity(): number {
    return this.lastActivity;
  }
}