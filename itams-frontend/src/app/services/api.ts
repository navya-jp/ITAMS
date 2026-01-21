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
  private readonly baseUrl = 'http://localhost:5066/api/superadmin';
  private readonly httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  constructor(private http: HttpClient) { }

  // Users
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/users`);
  }

  createUser(user: CreateUser): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/users`, user, this.httpOptions);
  }

  updateUser(id: number, user: Partial<User>): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/users/${id}`, user, this.httpOptions);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${id}`);
  }

  lockUser(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${id}/lock`, {}, this.httpOptions);
  }

  unlockUser(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${id}/unlock`, {}, this.httpOptions);
  }

  resetPassword(id: number, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${id}/reset-password`, 
      { newPassword }, this.httpOptions);
  }

  // Roles
  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.baseUrl}/roles`);
  }

  createRole(role: CreateRole): Observable<Role> {
    return this.http.post<Role>(`${this.baseUrl}/roles`, role, this.httpOptions);
  }

  updateRole(id: number, role: Partial<Role>): Observable<Role> {
    return this.http.put<Role>(`${this.baseUrl}/roles/${id}`, role, this.httpOptions);
  }

  // Permissions
  getAllPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.baseUrl}/permissions`);
  }

  getRolePermissions(roleId: number): Promise<Permission[]> {
    return this.http.get<Permission[]>(`${this.baseUrl}/roles/${roleId}/permissions`).toPromise() as Promise<Permission[]>;
  }

  updateRolePermissions(roleId: number, permissionIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/roles/${roleId}/permissions`, 
      { permissionIds }, this.httpOptions);
  }

  // Projects
  getProjects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.baseUrl}/projects`);
  }

  createProject(project: CreateProject): Observable<Project> {
    return this.http.post<Project>(`${this.baseUrl}/projects`, project, this.httpOptions);
  }

  updateProject(id: number, project: Partial<Project>): Observable<Project> {
    return this.http.put<Project>(`${this.baseUrl}/projects/${id}`, project, this.httpOptions);
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/projects/${id}`);
  }

  // Locations
  getLocations(): Observable<Location[]> {
    return this.http.get<Location[]>(`${this.baseUrl}/locations`);
  }

  createLocation(location: CreateLocation): Observable<Location> {
    return this.http.post<Location>(`${this.baseUrl}/locations`, location, this.httpOptions);
  }

  getProjectLocations(projectId: number): Observable<Location[]> {
    return this.http.get<Location[]>(`${this.baseUrl}/projects/${projectId}/locations`);
  }
}
