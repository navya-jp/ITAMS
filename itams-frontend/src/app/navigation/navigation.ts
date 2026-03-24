import { Component, OnInit, OnDestroy, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Api } from '../services/api';
import { Subscription, interval } from 'rxjs';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-navigation',
  imports: [CommonModule, RouterModule],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss',
})
export class Navigation implements OnInit, OnDestroy {
  currentUser: any = null;
  isAuthenticated = false;
  pageTitle = 'Dashboard';
  currentRoute = '';
  showProfileDropdown = false;
  showAlertsDropdown = false;
  alertSummary: any = { totalUnread: 0, critical: 0, high: 0, medium: 0, low: 0 };
  recentAlerts: any[] = [];
  
  // Properties for time display to avoid change detection issues
  lastActivity = 'Active now';
  sessionTimeRemaining = '30 minutes';
  
  private subscriptions: Subscription[] = [];

  // Navigation items for different user types
  adminNavItems = [
    { path: '/admin/dashboard', icon: 'fas fa-tachometer-alt', label: 'Dashboard', title: 'Admin Dashboard' },
    { path: '/admin/users', icon: 'fas fa-users', label: 'User Management', title: 'User Management' },
    { path: '/admin/roles', icon: 'fas fa-user-shield', label: 'Roles & Permissions', title: 'Roles & Permissions' },
    { path: '/admin/user-permissions', icon: 'fas fa-user-cog', label: 'User Permissions', title: 'User Permissions' },
    { path: '/admin/projects', icon: 'fas fa-folder', label: 'Projects & Locations', title: 'Projects & Locations' },
    { path: '/admin/assets', icon: 'fas fa-server', label: 'Assets', title: 'Asset Management' },
    { path: '/admin/alerts', icon: 'fas fa-bell', label: 'Alerts', title: 'Alerts & Reports' },
    { path: '/admin/audit', icon: 'fas fa-history', label: 'Audit Trail', title: 'Audit Trail' },
    { path: '/admin/settings', icon: 'fas fa-cog', label: 'Settings', title: 'System Settings' }
  ];

  userNavItems = [
    { path: '/user/dashboard', icon: 'fas fa-home', label: 'Dashboard', title: 'My Dashboard' },
    { path: '/user/projects', icon: 'fas fa-folder-open', label: 'My Projects', title: 'My Projects' },
    { path: '/user/assets', icon: 'fas fa-laptop', label: 'My Assets', title: 'My Assets' }
  ];

  constructor(
    private authService: AuthService,
    private api: Api,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Subscribe to authentication state
    const authSub = this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
    this.subscriptions.push(authSub);

    const isAuthSub = this.authService.isAuthenticated$.subscribe(isAuth => {
      this.isAuthenticated = isAuth;
    });
    this.subscriptions.push(isAuthSub);

    // Subscribe to route changes to update page title
    const routeSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.currentRoute = event.url;
        this.updatePageTitle();
      });
    this.subscriptions.push(routeSub);

    // Update time displays every minute
    const timeSub = interval(60000).subscribe(() => {
      this.updateTimeDisplays();
    });
    this.subscriptions.push(timeSub);

    // Poll alert count every 5 minutes
    const alertSub = interval(300000).subscribe(() => {
      if (this.isAuthenticated) this.loadAlertCount();
    });
    this.subscriptions.push(alertSub);

    // Initial updates
    this.updatePageTitle();
    this.updateTimeDisplays();
    this.loadAlertCount();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private updatePageTitle() {
    const navItems = this.isSuperAdmin() ? this.adminNavItems : this.userNavItems;
    const currentItem = navItems.find(item => this.currentRoute.startsWith(item.path));
    this.pageTitle = currentItem ? currentItem.title : 'ITAMS';
  }

  get navigationItems() {
    return this.isSuperAdmin() ? this.adminNavItems : this.userNavItems;
  }

  isSuperAdmin(): boolean {
    return this.currentUser?.roleName === 'Super Admin';
  }

  isActiveRoute(path: string): boolean {
    return this.currentRoute.startsWith(path);
  }

  onLogout() {
    this.authService.logout();
  }

  onChangePassword() {
    // Navigate to change password page without any query params
    // This will show the "Request Password Reset" button for regular users
    this.router.navigate(['/change-password']);
  }

  getUserDisplayName(): string {
    if (!this.currentUser) return 'User';
    return `${this.currentUser.firstName} ${this.currentUser.lastName}`;
  }

  getUserRole(): string {
    return this.currentUser?.roleName || 'User';
  }

  getUserInitials(): string {
    if (!this.currentUser) return 'U';
    const firstName = this.currentUser.firstName || '';
    const lastName = this.currentUser.lastName || '';
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
  }

  private updateTimeDisplays() {
    this.lastActivity = this.getLastActivityString();
    this.sessionTimeRemaining = this.getSessionTimeRemainingString();
  }

  private getLastActivityString(): string {
    const lastActivity = this.authService.getLastActivity();
    const now = Date.now();
    const diffMinutes = Math.floor((now - lastActivity) / 60000);
    
    if (diffMinutes < 1) return 'Active now';
    if (diffMinutes === 1) return '1 minute ago';
    return `${diffMinutes} minutes ago`;
  }

  private getSessionTimeRemainingString(): string {
    const settings = this.authService.settings;
    const lastActivity = this.authService.getLastActivity();
    const sessionTimeout = settings.sessionTimeoutMinutes * 60 * 1000;
    const timeRemaining = sessionTimeout - (Date.now() - lastActivity);
    
    if (timeRemaining <= 0) return 'Expired';
    
    const minutesRemaining = Math.floor(timeRemaining / 60000);
    if (minutesRemaining < 1) return 'Less than 1 minute';
    if (minutesRemaining === 1) return '1 minute';
    return `${minutesRemaining} minutes`;
  }

  refreshActivity() {
    this.authService.updateActivity();
  }

  loadAlertCount() {
    this.api.getAlertUnreadCount().subscribe({
      next: (data) => { this.alertSummary = data; },
      error: () => {}
    });
  }

  loadRecentAlerts() {
    this.api.getAlerts({ page: 1 }).subscribe({
      next: (data) => { this.recentAlerts = data.alerts?.slice(0, 8) ?? []; },
      error: () => {}
    });
  }

  toggleAlertsDropdown() {
    this.showAlertsDropdown = !this.showAlertsDropdown;
    if (this.showAlertsDropdown) {
      this.showProfileDropdown = false;
      this.loadRecentAlerts();
    }
  }

  closeAlertsDropdown() {
    this.showAlertsDropdown = false;
  }

  acknowledgeAlert(id: number, event: Event) {
    event.stopPropagation();
    this.api.acknowledgeAlert(id).subscribe({
      next: () => {
        this.loadAlertCount();
        this.loadRecentAlerts();
      }
    });
  }

  getSeverityClass(severity: string): string {
    return { Critical: 'danger', High: 'warning', Medium: 'info', Low: 'secondary' }[severity] ?? 'secondary';
  }

  toggleProfileDropdown() {
    this.showProfileDropdown = !this.showProfileDropdown;
    if (this.showProfileDropdown) this.showAlertsDropdown = false;
  }

  closeProfileDropdown() {
    this.showProfileDropdown = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const clickedInside = target.closest('.user-header-avatar') || target.closest('.alert-bell-wrapper');
    
    if (!clickedInside) {
      this.showProfileDropdown = false;
      this.showAlertsDropdown = false;
    }
  }
}