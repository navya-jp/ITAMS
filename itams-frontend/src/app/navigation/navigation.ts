import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../services/auth.service';
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
  
  // Properties for time display to avoid change detection issues
  lastActivity = 'Active now';
  sessionTimeRemaining = '30 minutes';
  
  private subscriptions: Subscription[] = [];

  // Navigation items for different user types
  adminNavItems = [
    { path: '/admin/dashboard', icon: 'fas fa-tachometer-alt', label: 'Dashboard', title: 'Admin Dashboard' },
    { path: '/admin/users', icon: 'fas fa-users', label: 'User Management', title: 'User Management' },
    { path: '/admin/projects', icon: 'fas fa-folder', label: 'Projects & Locations', title: 'Projects & Locations' },
    { path: '/admin/assets', icon: 'fas fa-server', label: 'Assets', title: 'Asset Management' },
    { path: '/admin/audit', icon: 'fas fa-history', label: 'Audit Trail', title: 'Audit Trail' }
  ];

  userNavItems = [
    { path: '/user/dashboard', icon: 'fas fa-home', label: 'Dashboard', title: 'My Dashboard' },
    { path: '/user/projects', icon: 'fas fa-folder-open', label: 'My Projects', title: 'My Projects' },
    { path: '/user/assets', icon: 'fas fa-laptop', label: 'My Assets', title: 'My Assets' }
  ];

  constructor(
    private authService: AuthService,
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

    // Initial updates
    this.updatePageTitle();
    this.updateTimeDisplays();
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
}