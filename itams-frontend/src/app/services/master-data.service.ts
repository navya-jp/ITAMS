import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

// =============================================
// Vendor Interfaces
// =============================================
export interface Vendor {
  id: number;
  vendorName: string;
  vendorCode: string;
  contactPerson?: string;
  email: string;
  phoneNumber?: string;
  address?: string;
  website?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateVendorRequest {
  vendorName: string;
  vendorCode: string;
  contactPerson?: string;
  email: string;
  phoneNumber?: string;
  address?: string;
  website?: string;
}

export interface UpdateVendorRequest {
  vendorName: string;
  contactPerson?: string;
  email: string;
  phoneNumber?: string;
  address?: string;
  website?: string;
  isActive: boolean;
}

// =============================================
// Asset Status Interfaces
// =============================================
export interface AssetStatus {
  id: number;
  statusName: string;
  statusCode: string;
  description?: string;
  colorCode: string;
  icon?: string;
  isActive: boolean;
  isPredefined: boolean;
  createdAt: string;
}

export interface CreateAssetStatusRequest {
  statusName: string;
  statusCode: string;
  description?: string;
  colorCode: string;
  icon?: string;
}

export interface UpdateAssetStatusRequest {
  statusName: string;
  description?: string;
  colorCode: string;
  icon?: string;
  isActive: boolean;
}

// =============================================
// Criticality Level Interfaces
// =============================================
export interface CriticalityLevel {
  id: number;
  levelName: string;
  levelCode: string;
  description?: string;
  priorityOrder: number;
  slaHours: number;
  priorityLevel: string;
  notificationThresholdDays: number;
  isActive: boolean;
  isPredefined: boolean;
  createdAt: string;
}

export interface CreateCriticalityLevelRequest {
  levelName: string;
  levelCode: string;
  description?: string;
  priorityOrder: number;
  slaHours: number;
  priorityLevel: string;
  notificationThresholdDays: number;
}

export interface UpdateCriticalityLevelRequest {
  levelName: string;
  description?: string;
  priorityOrder: number;
  slaHours: number;
  priorityLevel: string;
  notificationThresholdDays: number;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class MasterDataService {
  private readonly baseUrl = 'http://localhost:5066/api/masterdata';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): { headers: HttpHeaders } {
    const token = localStorage.getItem('auth_token');
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      })
    };
  }

  // =============================================
  // Vendor Methods
  // =============================================
  getVendors(includeInactive: boolean = false): Observable<Vendor[]> {
    return this.http.get<Vendor[]>(
      `${this.baseUrl}/vendors?includeInactive=${includeInactive}`,
      this.getAuthHeaders()
    );
  }

  getVendor(id: number): Observable<Vendor> {
    return this.http.get<Vendor>(
      `${this.baseUrl}/vendors/${id}`,
      this.getAuthHeaders()
    );
  }

  createVendor(request: CreateVendorRequest): Observable<Vendor> {
    return this.http.post<Vendor>(
      `${this.baseUrl}/vendors`,
      request,
      this.getAuthHeaders()
    );
  }

  updateVendor(id: number, request: UpdateVendorRequest): Observable<any> {
    return this.http.put(
      `${this.baseUrl}/vendors/${id}`,
      request,
      this.getAuthHeaders()
    );
  }

  deleteVendor(id: number): Observable<any> {
    return this.http.delete(
      `${this.baseUrl}/vendors/${id}`,
      this.getAuthHeaders()
    );
  }

  // =============================================
  // Asset Status Methods
  // =============================================
  getAssetStatuses(includeInactive: boolean = false): Observable<AssetStatus[]> {
    return this.http.get<AssetStatus[]>(
      `${this.baseUrl}/asset-statuses?includeInactive=${includeInactive}`,
      this.getAuthHeaders()
    );
  }

  createAssetStatus(request: CreateAssetStatusRequest): Observable<AssetStatus> {
    return this.http.post<AssetStatus>(
      `${this.baseUrl}/asset-statuses`,
      request,
      this.getAuthHeaders()
    );
  }

  updateAssetStatus(id: number, request: UpdateAssetStatusRequest): Observable<any> {
    return this.http.put(
      `${this.baseUrl}/asset-statuses/${id}`,
      request,
      this.getAuthHeaders()
    );
  }

  deleteAssetStatus(id: number): Observable<any> {
    return this.http.delete(
      `${this.baseUrl}/asset-statuses/${id}`,
      this.getAuthHeaders()
    );
  }

  // =============================================
  // Criticality Level Methods
  // =============================================
  getCriticalityLevels(includeInactive: boolean = false): Observable<CriticalityLevel[]> {
    return this.http.get<CriticalityLevel[]>(
      `${this.baseUrl}/criticality-levels?includeInactive=${includeInactive}`,
      this.getAuthHeaders()
    );
  }

  createCriticalityLevel(request: CreateCriticalityLevelRequest): Observable<CriticalityLevel> {
    return this.http.post<CriticalityLevel>(
      `${this.baseUrl}/criticality-levels`,
      request,
      this.getAuthHeaders()
    );
  }

  updateCriticalityLevel(id: number, request: UpdateCriticalityLevelRequest): Observable<any> {
    return this.http.put(
      `${this.baseUrl}/criticality-levels/${id}`,
      request,
      this.getAuthHeaders()
    );
  }

  deleteCriticalityLevel(id: number): Observable<any> {
    return this.http.delete(
      `${this.baseUrl}/criticality-levels/${id}`,
      this.getAuthHeaders()
    );
  }
}
