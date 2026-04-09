import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private base = '/api/reports';

  constructor(private http: HttpClient) {}

  private headers() {
    const token = localStorage.getItem('auth_token');
    return { headers: new HttpHeaders({ 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` }) };
  }

  getDashboardKpis(): Observable<any> {
    return this.http.get<any>(`${this.base}/dashboard-kpis`, this.headers());
  }

  getAssetInventory(filter: any = {}): Observable<any> {
    const clean: any = {};
    Object.keys(filter).forEach(k => { if (filter[k] !== undefined && filter[k] !== null) clean[k] = filter[k]; });
    const params = new URLSearchParams(clean).toString();
    return this.http.get<any>(`${this.base}/asset-inventory?${params}`, this.headers());
  }

  getWarrantyExpiry(daysAhead = 30): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/warranty-expiry?daysAhead=${daysAhead}`, this.headers());
  }

  getLicenseExpiry(daysAhead = 30): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/license-expiry?daysAhead=${daysAhead}`, this.headers());
  }

  getContractExpiry(daysAhead = 30): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/contract-expiry?daysAhead=${daysAhead}`, this.headers());
  }

  getMaintenanceSummary(filter: any = {}): Observable<any[]> {
    const params = new URLSearchParams(filter).toString();
    return this.http.get<any[]>(`${this.base}/maintenance-summary?${params}`, this.headers());
  }

  getComplianceStatus(filter: any = {}): Observable<any[]> {
    const params = new URLSearchParams(filter).toString();
    return this.http.get<any[]>(`${this.base}/compliance-status?${params}`, this.headers());
  }

  getTransferHistory(filter: any = {}): Observable<any[]> {
    const params = new URLSearchParams(filter).toString();
    return this.http.get<any[]>(`${this.base}/asset-transfer-history?${params}`, this.headers());
  }

  getUserActivity(filter: any = {}): Observable<any> {
    const params = new URLSearchParams(filter).toString();
    return this.http.get<any>(`${this.base}/user-activity?${params}`, this.headers());
  }

  getAlertSummary(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/alert-summary`, this.headers());
  }

  exportExcel(reportType: string, filter: any = {}): Observable<Blob> {
    return this.http.post(`${this.base}/export/excel`, { reportType, filter }, { ...this.headers(), responseType: 'blob' });
  }

  downloadFile(blob: Blob, filename: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }
}
