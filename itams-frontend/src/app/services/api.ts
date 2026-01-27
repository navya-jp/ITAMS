import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  mustChangePassword: boolean;
  isLocked: boolean;
}

export interface CreateUser {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  roleId: number;
  password: string;
  mustChangePassword: boolean;
}

export interface UpdateUser {
  email?: string;
  firstName?: string;
  lastName?: string;
  roleId?: number;
  isActive?: boolean;
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
  description?: string;
  code: string;
  isActive: boolean;
  createdAt: string;
  locationCount: number;
  userCount: number;
}

export interface CreateProject {
  name: string;
  code: string;
  description?: string;
}

export interface Location {
  id: number;
  name: string;
  region: string;
  state: string;
  plaza?: string;
  lane?: string;
  office?: string;
  address?: string;
  isActive: boolean;
  projectId: number;
  assetCount: number;
}

export interface CreateLocation {
  name: string;
  projectId: number;
  region: string;
  state: string;
  plaza?: string;
  lane?: string;
  office?: string;
  address?: string;
}

@Injectable({
  providedIn: 'root',
})
export class Api {
  private readonly baseUrl = 'http://localhost:5068/api';
  private readonly httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  constructor(private http: HttpClient) { }

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

  getUsersByRole(roleId: number): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<User[]>>(`${this.baseUrl}/users/by-role/${roleId}`);
  }

  // Roles - Using SuperAdmin controller
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

  getRolePermissions(roleId: number): Promise<Permission[]> {
    return this.http.get<Permission[]>(`${this.baseUrl}/superadmin/roles/${roleId}/permissions`).toPromise() as Promise<Permission[]>;
  }

  updateRolePermissions(roleId: number, permissionIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/superadmin/roles/${roleId}/permissions`, 
      { permissionIds }, this.httpOptions);
  }

  // Projects
  getProjects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.baseUrl}/superadmin/projects`);
  }

  createProject(project: CreateProject): Observable<Project> {
    return this.http.post<Project>(`${this.baseUrl}/superadmin/projects`, project, this.httpOptions);
  }

  updateProject(id: number, project: Partial<Project>): Observable<Project> {
    return this.http.put<Project>(`${this.baseUrl}/superadmin/projects/${id}`, project, this.httpOptions);
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/superadmin/projects/${id}`);
  }

  // Locations
  getLocations(): Observable<Location[]> {
    return this.http.get<Location[]>(`${this.baseUrl}/superadmin/locations`);
  }

  createLocation(location: CreateLocation): Observable<Location> {
    return this.http.post<Location>(`${this.baseUrl}/superadmin/locations`, location, this.httpOptions);
  }

  getProjectLocations(projectId: number): Observable<Location[]> {
    return this.http.get<Location[]>(`${this.baseUrl}/superadmin/projects/${projectId}/locations`);
  }
}
