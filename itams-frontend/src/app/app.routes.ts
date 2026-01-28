import { Routes } from '@angular/router';
import { Dashboard } from './dashboard/dashboard';
import { Users } from './users/users';
import { Projects } from './projects/projects';
import { Login } from './login/login';
import { ChangePassword } from './change-password/change-password';
import { UserDashboard } from './user-dashboard/user-dashboard';
import { UserProjects } from './user-projects/user-projects';
import { AuthGuard, AdminGuard, UserGuard, LoginGuard } from './guards/auth.guard';

export const routes: Routes = [
  // Authentication routes
  { path: 'login', component: Login, canActivate: [LoginGuard] },
  { path: 'change-password', component: ChangePassword, canActivate: [AuthGuard] },
  
  // Default redirect based on authentication
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  
  // Admin routes (Super Admin)
  { path: 'admin/dashboard', component: Dashboard, canActivate: [AdminGuard] },
  { path: 'admin/users', component: Users, canActivate: [AdminGuard] },
  { path: 'admin/projects', component: Projects, canActivate: [AdminGuard] },
  { path: 'admin/assets', component: Dashboard, canActivate: [AdminGuard] }, // Placeholder
  { path: 'admin/audit', component: Dashboard, canActivate: [AdminGuard] }, // Placeholder
  
  // User routes (Regular Users)
  { path: 'user/dashboard', component: UserDashboard, canActivate: [UserGuard] },
  { path: 'user/projects', component: UserProjects, canActivate: [UserGuard] },
  { path: 'user/assets', component: UserDashboard, canActivate: [UserGuard] }, // Placeholder
  
  // Legacy redirects for backward compatibility
  { path: 'dashboard', redirectTo: '/admin/dashboard', pathMatch: 'full' },
  { path: 'users', redirectTo: '/admin/users', pathMatch: 'full' },
  { path: 'projects', redirectTo: '/admin/projects', pathMatch: 'full' },
  { path: 'assets', redirectTo: '/admin/assets', pathMatch: 'full' },
  { path: 'audit', redirectTo: '/admin/audit', pathMatch: 'full' },
  
  // Catch all redirect
  { path: '**', redirectTo: '/login' }
];
