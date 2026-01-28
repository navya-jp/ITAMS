import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Api } from '../services/api';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-user-dashboard',
  imports: [CommonModule, RouterModule],
  templateUrl: './user-dashboard.html',
  styleUrl: './user-dashboard.scss',
})
export class UserDashboard implements OnInit {
  loading = false;
  error = '';
  
  // User-specific data
  myProjects: any[] = [];
  myAssets: any[] = [];
  recentActivities: any[] = [];
  
  // Dashboard stats
  stats = {
    totalProjects: 0,
    totalAssets: 0,
    pendingTasks: 0,
    recentActivities: 0
  };

  // Current user info (from AuthService)
  get currentUser() {
    const user = this.authService.currentUser;
    if (user) {
      return {
        name: `${user.firstName} ${user.lastName}`,
        role: user.roleName,
        email: user.email,
        avatar: `https://ui-avatars.com/api/?name=${user.firstName}+${user.lastName}&background=007bff&color=fff&size=40`
      };
    }
    return {
      name: 'User',
      role: 'User',
      email: 'user@company.com',
      avatar: 'https://ui-avatars.com/api/?name=U&background=007bff&color=fff&size=40'
    };
  }

  constructor(
    private api: Api,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadUserDashboard();
  }

  loadUserDashboard() {
    this.loading = true;
    
    // Load user-specific data
    this.loadMyProjects();
    this.loadMyAssets();
    this.loadRecentActivities();
    
    this.loading = false;
  }

  loadMyProjects() {
    // TODO: Implement actual API call for user's projects
    // For now, show empty state since no projects exist yet
    this.myProjects = [];
    this.stats.totalProjects = 0;
    
    // Uncomment when user projects API is implemented:
    // this.api.getUserProjects().subscribe({
    //   next: (projects) => {
    //     this.myProjects = projects;
    //     this.stats.totalProjects = projects.length;
    //   },
    //   error: (error) => {
    //     this.error = 'Failed to load projects';
    //   }
    // });
  }

  loadMyAssets() {
    // TODO: Implement actual API call for user's assets
    // For now, show empty state since no assets exist yet
    this.myAssets = [];
    this.stats.totalAssets = 0;
    
    // Uncomment when user assets API is implemented:
    // this.api.getUserAssets().subscribe({
    //   next: (assets) => {
    //     this.myAssets = assets;
    //     this.stats.totalAssets = assets.length;
    //   },
    //   error: (error) => {
    //     this.error = 'Failed to load assets';
    //   }
    // });
  }

  loadRecentActivities() {
    // TODO: Implement actual API call for user's recent activities
    // For now, show empty state since no activities exist yet
    this.recentActivities = [];
    this.stats.recentActivities = 0;
    this.stats.pendingTasks = 0;
    
    // Uncomment when user activities API is implemented:
    // this.api.getUserActivities().subscribe({
    //   next: (activities) => {
    //     this.recentActivities = activities;
    //     this.stats.recentActivities = activities.length;
    //     this.stats.pendingTasks = activities.filter(a => a.status === 'pending').length;
    //   },
    //   error: (error) => {
    //     this.error = 'Failed to load activities';
    //   }
    // });
  }

  getProgressColor(progress: number): string {
    if (progress >= 80) return 'success';
    if (progress >= 50) return 'warning';
    return 'danger';
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return 'bg-success';
      case 'in progress': return 'bg-warning';
      case 'completed': return 'bg-primary';
      case 'assigned': return 'bg-info';
      default: return 'bg-secondary';
    }
  }

  getActivityIcon(type: string): string {
    switch (type) {
      case 'success': return 'fas fa-check-circle text-success';
      case 'info': return 'fas fa-info-circle text-info';
      case 'warning': return 'fas fa-exclamation-triangle text-warning';
      case 'danger': return 'fas fa-times-circle text-danger';
      default: return 'fas fa-circle text-secondary';
    }
  }
}