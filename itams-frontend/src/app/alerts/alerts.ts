import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../services/api';

@Component({
  selector: 'app-alerts',
  imports: [CommonModule, FormsModule],
  templateUrl: './alerts.html'
})
export class Alerts implements OnInit {
  alerts: any[] = [];
  dashboard: any = null;
  activeTab: 'alerts' | 'dashboard' = 'alerts';
  loading = false;
  dashboardLoading = false;

  filterSeverity = '';
  filterType = '';
  includeResolved = false;
  total = 0;
  page = 1;

  alertTypes = [
    { value: '', label: 'All Types' },
    { value: 'WARRANTY_EXPIRY', label: 'Warranty Expiry' },
    { value: 'LICENSE_EXPIRY', label: 'License Expiry' },
    { value: 'CONTRACT_EXPIRY', label: 'Contract Expiry' },
    { value: 'REPAIR_STUCK', label: 'Stuck in Repair' },
    { value: 'COMPLIANCE_FAILURE', label: 'Compliance Failure' }
  ];

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadAlerts();
    this.loadDashboard();
  }

  loadAlerts() {
    this.loading = true;
    this.api.getAlerts({
      includeResolved: this.includeResolved,
      severity: this.filterSeverity || undefined,
      alertType: this.filterType || undefined,
      page: this.page
    }).subscribe({
      next: (data) => {
        this.alerts = data.alerts;
        this.total = data.total;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  loadDashboard() {
    this.dashboardLoading = true;
    this.api.getAlertsDashboard().subscribe({
      next: (data) => { this.dashboard = data; this.dashboardLoading = false; },
      error: () => { this.dashboardLoading = false; }
    });
  }

  acknowledge(id: number) {
    this.api.acknowledgeAlert(id).subscribe({ next: () => this.loadAlerts() });
  }

  resolve(id: number) {
    this.api.resolveAlert(id).subscribe({ next: () => this.loadAlerts() });
  }

  getSeverityClass(severity: string): string {
    return { Critical: 'danger', High: 'warning', Medium: 'info', Low: 'secondary' }[severity] ?? 'secondary';
  }

  getAlertTypeLabel(type: string): string {
    return this.alertTypes.find(t => t.value === type)?.label ?? type;
  }

  onFilterChange() {
    this.page = 1;
    this.loadAlerts();
  }
}
