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

export interface Asset {
  id: number;
  assetId: string;
  assetTag: string;
  projectId: number;
  projectName?: string;
  locationId: number;
  locationName?: string;
  region?: string;
  state?: string;
  site?: string;
  plazaName?: string;
  locationText?: string;
  department?: string;
  classification?: string;
  osType?: string;
  osVersion?: string;
  dbType?: string;
  dbVersion?: string;
  ipAddress?: string;
  assignedUserText?: string;
  userRole?: string;
  procuredBy?: string;
  patchStatus?: string;
  usbBlockingStatus?: string;
  remarks?: string;
  usageCategory: string;
  criticality: string;
  assetType: string;
  subType?: string;
  make: string;
  model: string;
  serialNumber?: string;
  procurementDate?: Date;
  procurementCost?: number;
  vendor?: string;
  warrantyStartDate?: Date;
  warrantyEndDate?: Date;
  commissioningDate?: Date;
  status: string;
  assignedUserId?: number;
  assignedUserName?: string;
  assignedUserRole?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateAsset {
  assetTag: string;
  projectId: number;
  locationId: number;
  usageCategory: string;
  criticality: string;
  assetType: string;
  subType?: string;
  make: string;
  model: string;
  serialNumber?: string;
  procurementDate?: Date;
  procurementCost?: number;
  vendor?: string;
  warrantyStartDate?: Date;
  warrantyEndDate?: Date;
  commissioningDate?: Date;
  status: string;
  assignedUserId?: number;
  assignedUserRole?: string;
}

@Injectable({
  providedIn: 'root',
})
export class Api {
  private readonly baseUrl = '/api'; // Use relative URL with proxy
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
    return this.http.get<ApiResponse<User[]>>(`${this.baseUrl}/users`, this.getAuthHeaders());
  }

  getUser(id: number): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.baseUrl}/users/${id}`, this.getAuthHeaders());
  }

  createUser(user: CreateUser): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(`${this.baseUrl}/users`, user, this.getAuthHeaders());
  }

  updateUser(id: number, user: UpdateUser): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.baseUrl}/users/${id}`, user, this.getAuthHeaders());
  }

  deactivateUser(id: number): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.baseUrl}/users/${id}/deactivate`, {}, this.getAuthHeaders());
  }

  activateUser(id: number): Observable<ApiResponse<any>> {
    return this.http.patch<ApiResponse<any>>(`${this.baseUrl}/users/${id}/activate`, {}, this.getAuthHeaders());
  }

  lockUser(id: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/users/${id}/lock`, {}, this.getAuthHeaders());
  }

  unlockUser(id: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/users/${id}/unlock`, {}, this.getAuthHeaders());
  }

  resetPassword(id: number, newPassword: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/users/${id}/reset-password`, 
      { newPassword }, this.getAuthHeaders());
  }

  checkUsernameAvailability(username: string): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(`${this.baseUrl}/users/check-username/${encodeURIComponent(username)}`, this.getAuthHeaders());
  }

  checkEmailAvailability(email: string): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(`${this.baseUrl}/users/check-email/${encodeURIComponent(email)}`, this.getAuthHeaders());
  }

  getUsersByRole(roleId: number): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<User[]>>(`${this.baseUrl}/users/by-role/${roleId}`, this.getAuthHeaders());
  }

  // RBAC - Using new RBAC controller endpoints
  getRbacRoles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/rbac/roles`, this.getAuthHeaders());
  }

  getRbacPermissions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/rbac/permissions`, this.getAuthHeaders());
  }

  getRbacPermissionsGrouped(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/rbac/permissions/grouped`, this.getAuthHeaders());
  }

  getRbacRolePermissions(roleId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/rbac/roles/${roleId}/permissions`, this.getAuthHeaders());
  }

  updateRbacRolePermissions(roleId: number, permissionIds: number[]): Observable<any> {
    return this.http.put(`${this.baseUrl}/rbac/roles/${roleId}/permissions`, 
      { permissionIds }, this.getAuthHeaders());
  }

  // Legacy roles (keeping for backward compatibility)
  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.baseUrl}/superadmin/roles`, this.getAuthHeaders());
  }

  createRole(role: CreateRole): Observable<Role> {
    return this.http.post<Role>(`${this.baseUrl}/superadmin/roles`, role, this.getAuthHeaders());
  }

  updateRole(id: number, role: Partial<Role>): Observable<Role> {
    return this.http.put<Role>(`${this.baseUrl}/superadmin/roles/${id}`, role, this.getAuthHeaders());
  }

  // Permissions
  getAllPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.baseUrl}/superadmin/permissions`, this.getAuthHeaders());
  }

  getRolePermissions(roleId: number): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.baseUrl}/superadmin/roles/${roleId}/permissions`, this.getAuthHeaders());
  }

  updateRolePermissions(roleId: number, permissionIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/superadmin/roles/${roleId}/permissions`, 
      { permissionIds }, this.getAuthHeaders());
  }

  // Projects
  getProjects(): Observable<Project[]> {
    return this.http.get<any[]>(`${this.baseUrl}/superadmin/projects`, this.getAuthHeaders()).pipe(
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
    return this.http.post<Project>(`${this.baseUrl}/superadmin/projects`, projectData, this.getAuthHeaders());
  }

  updateProject(id: number, project: Partial<Project>): Observable<Project> {
    return this.http.put<Project>(`${this.baseUrl}/superadmin/projects/${id}`, project, this.getAuthHeaders());
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/superadmin/projects/${id}`, this.getAuthHeaders());
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
    
    return this.http.get<any[]>(url, this.getAuthHeaders());
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

  // Assets
  getAssets(): Observable<Asset[]> {
    return this.http.get<Asset[]>(`${this.baseUrl}/assets`, this.getAuthHeaders());
  }

  getMyAssets(): Observable<Asset[]> {
    return this.http.get<Asset[]>(`${this.baseUrl}/assets/my-assets`, this.getAuthHeaders());
  }

  getAsset(id: number): Observable<Asset> {
    return this.http.get<Asset>(`${this.baseUrl}/assets/${id}`, this.getAuthHeaders());
  }

  createAsset(asset: CreateAsset): Observable<Asset> {
    return this.http.post<Asset>(`${this.baseUrl}/assets`, asset, this.getAuthHeaders());
  }

  updateAsset(id: number, asset: Partial<CreateAsset>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/assets/${id}`, asset, this.getAuthHeaders());
  }

  deleteAsset(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/assets/${id}`, this.getAuthHeaders());
  }
}
