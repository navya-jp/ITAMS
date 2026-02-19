import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Permission {
  id: number;
  name: string;
  code: string;
  description?: string;
  module: string;
  isActive: boolean;
}

export interface CreateRole {
  name: string;
  description?: string;
  isSystemRole: boolean;
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
  createdAt: string;
  lastLoginAt?: string;
  lastActivityAt?: string;
  mustChangePassword: boolean;
  isLocked: boolean;
  passwordResetRequested?: boolean;
  passwordResetRequestedAt?: string;
  projectId?: number;
  restrictedRegion?: string;
  restrictedState?: string;
  restrictedPlaza?: string;
  restrictedOffice?: string;
  activeSessionId?: string;
  sessionStartedAt?: string;
}

export interface CreateUser {
  username: string;  // 3-100 chars, alphanumeric + underscore/dots only
  email: string;     // valid email format
  firstName: string; // 1-100 chars, required
  lastName: string;  // 1-100 chars, required
  roleId: number;    // required, > 0
  password: string;  // 8+ chars, must have uppercase, lowercase, digit, special char (@$!%*?&)
  mustChangePassword: boolean;
  projectId?: number; // Project assignment
  restrictedRegion?: string; // Location restriction - Region level
  restrictedState?: string;  // Location restriction - State level
  restrictedPlaza?: string;  // Location restriction - Plaza level
  restrictedOffice?: string; // Location restriction - Office level
}

export interface UpdateUser {
  email?: string;
  firstName?: string;
  lastName?: string;
  roleId?: number;
  isActive?: boolean;
  projectId?: number; // Project assignment
  restrictedRegion?: string; // Location restriction - Region level
  restrictedState?: string;  // Location restriction - State level
  restrictedPlaza?: string;  // Location restriction - Plaza level
  restrictedOffice?: string; // Location restriction - Office level
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
  error?: string;
  validationErrors?: { [key: string]: string[] };
}

export interface Role {
  id: number;
  name: string;
  description: string;
  isSystemRole: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface Project {
  id: number;
  name: string;
  preferredName?: string;
  description?: string;
  code: string;
  spvName: string;
  states: string[];
  isActive: boolean;
  createdAt: string;
  locationCount: number;
  userCount: number;
}

export interface CreateProject {
  name: string; // SPV Name
  preferredName?: string;
  code: string;
  spvName: string;
  states: string[];
  description?: string;
}

export interface Location {
  id: number;
  name: string;
  type: 'office' | 'plaza';
  projectId: number;
  isActive: boolean;
  assetCount: number;
  
  // Office specific
  district?: string;
  
  // Plaza specific
  plazaCode?: string;
  governmentCode?: string;
  chainageNumber?: string;
  latitude?: number;
  longitude?: number;
  numberOfLanes?: number;
  lanes?: Lane[];
  internalLocations?: string[];
  
  // Backend fields
  region?: string;
  state?: string;
  plaza?: string;
  lane?: string;
  office?: string;
  address?: string;
}

export interface Lane {
  id: number;
  laneNumber: number;
  locationId: number;
  // Future expandable properties
}

export interface CreateLocation {
  name: string;
  type: 'office' | 'site';
  projectId: number;
  
  // Office specific
  district?: string;
  
  // Site specific
  siteCode?: string;
  chainageNumber?: string;
  latitude?: number;
  longitude?: number;
  numberOfLanes?: number;
  internalLocations?: string[];
  
  // Backend required fields
  region?: string;
  state?: string;
  site?: string;
  lane?: string;
  office?: string;
  address?: string;
}

@Injectable({
  providedIn: 'root',
})
export class Api {
  private readonly baseUrl = 'http://localhost:5066/api';
  private readonly httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  constructor(private http: HttpClient) { }

  private getAuthHeaders() {
    const token = localStorage.getItem('auth_token');
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      })
    };
  }

  // Users - Using dedicated Users controller
  getUsers(): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<User[]>>(`${this.baseUrl}/users`);
  }

  getUser(id: number): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.baseUrl}/users/${id}`);
  }

  createUser(user: CreateUser): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(`${this.baseUrl}/users`, user, this.httpOptions);
  }

  updateUser(id: number, user: UpdateUser): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.baseUrl}/users/${id}`, user, this.httpOptions);
  }

  deactivateUser(id: number): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.baseUrl}/users/${id}/deactivate`, {}, this.httpOptions);
  }

  activateUser(id: number): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.baseUrl}/users/${id}/activate`, {}, this.httpOptions);
  }

  lockUser(id: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/users/${id}/lock`, {}, this.httpOptions);
  }

  unlockUser(id: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/users/${id}/unlock`, {}, this.httpOptions);
  }

  resetPassword(id: number, newPassword: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/users/${id}/reset-password`, 
      { newPassword }, this.httpOptions);
  }

  checkUsernameAvailability(username: string): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(`${this.baseUrl}/users/check-username/${encodeURIComponent(username)}`);
  }

  checkEmailAvailability(email: string): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(`${this.baseUrl}/users/check-email/${encodeURIComponent(email)}`);
  }

  getUsersByRole(roleId: number): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<User[]>>(`${this.baseUrl}/users/by-role/${roleId}`);
  }

  // RBAC - Using new RBAC controller endpoints
  getRbacRoles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/rbac/roles`);
  }

  getRbacPermissions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/rbac/permissions`);
  }

  getRbacPermissionsGrouped(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/rbac/permissions/grouped`);
  }

  getRbacRolePermissions(roleId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/rbac/roles/${roleId}/permissions`);
  }

  updateRbacRolePermissions(roleId: number, permissionIds: number[]): Observable<any> {
    return this.http.put(`${this.baseUrl}/rbac/roles/${roleId}/permissions`, 
      { permissionIds }, this.httpOptions);
  }

  // Legacy roles (keeping for backward compatibility)
  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.baseUrl}/superadmin/roles`);
  }

  createRole(role: CreateRole): Observable<Role> {
    return this.http.post<Role>(`${this.baseUrl}/superadmin/roles`, role, this.httpOptions);
  }

  updateRole(id: number, role: Partial<Role>): Observable<Role> {
    return this.http.put<Role>(`${this.baseUrl}/superadmin/roles/${id}`, role, this.httpOptions);
  }

  // Permissions
  getAllPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.baseUrl}/superadmin/permissions`);
  }

  getRolePermissions(roleId: number): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.baseUrl}/superadmin/roles/${roleId}/permissions`);
  }

  updateRolePermissions(roleId: number, permissionIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/superadmin/roles/${roleId}/permissions`, 
      { permissionIds }, this.httpOptions);
  }

  // Projects
  getProjects(): Observable<Project[]> {
    return this.http.get<any[]>(`${this.baseUrl}/superadmin/projects`).pipe(
      map(projects => projects.map(p => ({
        ...p,
        states: p.states ? (typeof p.states === 'string' ? p.states.split(',').map((s: string) => s.trim()).filter((s: string) => s) : p.states) : []
      })))
    );
  }

  createProject(project: CreateProject): Observable<Project> {
    // Convert states array to comma-separated string for backend
    const projectData = {
      ...project,
      states: project.states.join(',')
    };
    return this.http.post<Project>(`${this.baseUrl}/superadmin/projects`, projectData, this.httpOptions);
  }

  updateProject(id: number, project: Partial<Project>): Observable<Project> {
    return this.http.put<Project>(`${this.baseUrl}/superadmin/projects/${id}`, project, this.httpOptions);
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/superadmin/projects/${id}`);
  }

  // Locations
  getLocations(): Observable<Location[]> {
    return this.http.get<Location[]>(`${this.baseUrl}/superadmin/locations`, this.getAuthHeaders());
  }

  createLocation(location: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/superadmin/locations`, location, this.getAuthHeaders());
  }

  getProjectLocations(projectId: number): Observable<Location[]> {
    return this.http.get<Location[]>(`${this.baseUrl}/superadmin/projects/${projectId}/locations`, this.getAuthHeaders());
  }

  updateLocation(id: number, location: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/superadmin/locations/${id}`, location, this.getAuthHeaders());
  }

  deleteLocation(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/superadmin/locations/${id}`, this.getAuthHeaders());
  }

  // Login Audit
  getLoginAudits(pageSize: number = 100, startDate?: Date, endDate?: Date): Observable<any[]> {
    let url = `${this.baseUrl}/superadmin/login-audit?pageSize=${pageSize}`;
    
    if (startDate) {
      url += `&fromDate=${startDate.toISOString()}`;
    }
    if (endDate) {
      url += `&toDate=${endDate.toISOString()}`;
    }
    
    return this.http.get<any[]>(url);
  }

  // System Settings
  getSystemSettings(): Observable<any> {
    return this.http.get(`${this.baseUrl}/settings`, this.getAuthHeaders());
  }

  getSettingsByCategory(category: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/settings/category/${category}`, this.getAuthHeaders());
  }

  updateSetting(id: number, value: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/settings/${id}`, { id, settingValue: value }, this.getAuthHeaders());
  }

  bulkUpdateSettings(settings: Array<{id: number, settingValue: string}>): Observable<any> {
    return this.http.post(`${this.baseUrl}/settings/bulk-update`, settings, this.getAuthHeaders());
  }

  // User Projects
  getMyProject(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/users/my-project`, this.getAuthHeaders());
  }
}
