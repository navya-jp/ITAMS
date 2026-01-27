import { Routes } from '@angular/router';
import { Dashboard } from './dashboard/dashboard';
import { Users } from './users/users';
import { Projects } from './projects/projects';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard },
  { path: 'users', component: Users },
  { path: 'projects', component: Projects },
  { path: 'assets', component: Dashboard }, // Placeholder
  { path: 'audit', component: Dashboard }, // Placeholder
];
