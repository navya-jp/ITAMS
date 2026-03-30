import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DecommissionService {
  private baseUrl = '/api/decommission';

  constructor(private http: HttpClient) {}

  private headers() {
    const token = localStorage.getItem('auth_token');
    return { headers: new HttpHeaders({ 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` }) };
  }

  getEolCandidates(params?: any): Observable<any[]> {
    const q = params ? '?' + new URLSearchParams(params).toString() : '';
    return this.http.get<any[]>(`${this.baseUrl}/end-of-life-candidates${q}`, this.headers());
  }

  initiateDecommission(dto: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/initiate`, dto, this.headers());
  }

  getRequests(filters?: any): Observable<any> {
    const q = filters ? '?' + new URLSearchParams(filters).toString() : '';
    return this.http.get<any>(`${this.baseUrl}/requests${q}`, this.headers());
  }

  getMyPendingRequests(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/requests/my-pending`, this.headers());
  }

  approveRequest(requestId: number, comments: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/requests/${requestId}/approve`, { comments }, this.headers());
  }

  rejectRequest(requestId: number, reason: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/requests/${requestId}/reject`, { reason }, this.headers());
  }

  getArchive(page = 1): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/archive?page=${page}`, this.headers());
  }

  getArchiveByAsset(assetId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/archive/${assetId}`, this.headers());
  }
}
