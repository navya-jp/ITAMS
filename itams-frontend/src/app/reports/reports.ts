import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from './report.service';

declare var Chart: any;

@Component({
  selector: 'app-reports',
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.html'
})
export class Reports implements OnInit, AfterViewInit {
  currentView: 'asset-inventory' | 'warranty' | 'license'
    | 'maintenance' | 'compliance' | 'transfers' | 'user-activity' | 'alerts' = 'asset-inventory';

  kpis: any = null;
  reportData: any[] = [];
  reportTotal = 0;
  loading = false;
  exporting = false;

  // Filters
  daysAhead = 30;
  maintenanceStatus = '';
  page = 1;
  pageSize = 50;
  locationType = 'office'; // default to Head Office
  selectedProjectId: number | null = null;
  userActivityFrom = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);
  userActivityTo = new Date().toISOString().slice(0, 10);
  userActivityRange = 'month';
  projects = [
    { id: 1, name: 'SBHL' }, { id: 2, name: 'BAEL' }, { id: 3, name: 'BKEL' },
    { id: 4, name: 'MBEL' }, { id: 5, name: 'PSRDCL' }, { id: 6, name: 'CNTL' },
    { id: 7, name: 'HREL' }
  ];

  charts: any[] = [];

  reportNav = [
    { view: 'asset-inventory', icon: 'fas fa-server', label: 'Asset Inventory' },
    { view: 'warranty', icon: 'fas fa-shield-alt', label: 'Warranty Expiry' },
    { view: 'license', icon: 'fas fa-key', label: 'License Expiry' },
    { view: 'maintenance', icon: 'fas fa-tools', label: 'Maintenance Summary' },
    { view: 'compliance', icon: 'fas fa-check-circle', label: 'Compliance Status' },
    { view: 'transfers', icon: 'fas fa-exchange-alt', label: 'Transfer History' },
    { view: 'user-activity', icon: 'fas fa-user-clock', label: 'User Activity' },
    { view: 'alerts', icon: 'fas fa-bell', label: 'Alert Summary' }
  ];

  constructor(private svc: ReportService) {}

  ngOnInit() { this.loadReport(); }

  ngAfterViewInit() { }

  get currentViewLabel(): string {
    return this.reportNav.find(r => r.view === this.currentView)?.label ?? 'Reports';
  }

  navigateTo(view: string) {
    this.currentView = view as any;
    this.page = 1;
    this.reportData = [];
    this.locationType = 'office';
    this.selectedProjectId = null;
    this.destroyCharts();
    this.loadReport();
  }

  onUserActivityRangeChange() {
    const today = new Date();
    const toStr = today.toISOString().slice(0, 10);
    switch (this.userActivityRange) {
      case 'today':
        this.userActivityFrom = toStr;
        this.userActivityTo = toStr;
        break;
      case 'week':
        this.userActivityFrom = new Date(Date.now() - 7 * 86400000).toISOString().slice(0, 10);
        this.userActivityTo = toStr;
        break;
      case 'month':
        this.userActivityFrom = new Date(Date.now() - 30 * 86400000).toISOString().slice(0, 10);
        this.userActivityTo = toStr;
        break;
      case 'year':
        this.userActivityFrom = new Date(Date.now() - 365 * 86400000).toISOString().slice(0, 10);
        this.userActivityTo = toStr;
        break;
      // 'custom' — don't change dates, let user pick
    }
    if (this.userActivityRange !== 'custom') this.loadReport();
  }

  onLocationTypeChange() {
    this.selectedProjectId = null;
    this.page = 1;
    this.loadReport();
  }

  get totalPages(): number { return Math.ceil(this.reportTotal / this.pageSize); }

  prevPage() { if (this.page > 1) { this.page--; this.loadReport(); } }
  nextPage() { if (this.page < this.totalPages) { this.page++; this.loadReport(); } }

  loadDashboard() {
    this.loading = true;
    this.svc.getDashboardKpis().subscribe({
      next: (data) => {
        this.kpis = data;
        this.loading = false;
        setTimeout(() => this.renderCharts(), 100);
      },
      error: () => { this.loading = false; }
    });
  }

  loadReport() {
    this.loading = true;
    let obs: any;
    switch (this.currentView) {
      case 'asset-inventory': obs = this.svc.getAssetInventory({
        pageNumber: this.page, pageSize: this.pageSize,
        locationType: this.locationType || undefined,
        projectId: this.selectedProjectId || undefined
      }); break;
      case 'warranty': obs = this.svc.getWarrantyExpiry(this.daysAhead); break;
      case 'license': obs = this.svc.getLicenseExpiry(this.daysAhead); break;
      case 'maintenance': obs = this.svc.getMaintenanceSummary(this.maintenanceStatus ? { status: this.maintenanceStatus } : {}); break;
      case 'compliance': obs = this.svc.getComplianceStatus({}); break;
      case 'transfers': obs = this.svc.getTransferHistory({}); break;
      case 'user-activity': obs = this.svc.getUserActivity({ from: this.userActivityFrom, to: this.userActivityTo }); break;
      case 'alerts': obs = this.svc.getAlertSummary(); break;
      default: this.loading = false; return;
    }
    obs.subscribe({
      next: (data: any) => {
        if (data?.items) { this.reportData = data.items; this.reportTotal = data.total; }
        else if (data?.items === undefined && Array.isArray(data)) { this.reportData = data; this.reportTotal = data.length; }
        else { this.reportData = data?.items ?? []; this.reportTotal = data?.total ?? 0; }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  exportExcel() {
    this.exporting = true;
    const filter: any = { daysAhead: String(this.daysAhead) };
    if (this.currentView === 'asset-inventory') {
      filter.locationType = this.locationType;
      if (this.selectedProjectId) filter.projectId = String(this.selectedProjectId);
    }
    if (this.currentView === 'user-activity') {
      filter.from = this.userActivityFrom;
      filter.to = this.userActivityTo;
    }
    this.svc.exportExcel(this.currentView, filter).subscribe({
      next: (blob) => {
        this.svc.downloadFile(blob, `${this.currentView}_${new Date().toISOString().slice(0,10)}.xlsx`);
        this.exporting = false;
      },
      error: () => { this.exporting = false; }
    });
  }

  getSeverityClass(s: string): string {
    return { Critical: 'danger', High: 'warning', Medium: 'info', Low: 'secondary' }[s] ?? 'secondary';
  }

  getResultClass(r: string): string {
    return r === 'Pass' ? 'success' : r === 'Warning' ? 'warning' : 'danger';
  }

  // ── Charts ────────────────────────────────────────────────────────────────

  destroyCharts() {
    this.charts.forEach(c => { try { c.destroy(); } catch {} });
    this.charts = [];
  }

  renderCharts() {
    this.destroyCharts();
    setTimeout(() => {
      this.renderBar('chartByType', this.kpis.assetsByType?.map((x: any) => x.label), this.kpis.assetsByType?.map((x: any) => x.count), 'Assets by Type', '#5E35B1');
      this.renderDoughnut('chartByStatus', this.kpis.assetsByStatus?.map((x: any) => x.label), this.kpis.assetsByStatus?.map((x: any) => x.count));
      this.renderBar('chartByLocation', this.kpis.assetsByLocation?.map((x: any) => x.label), this.kpis.assetsByLocation?.map((x: any) => x.count), 'Top Locations', '#1976D2');
      this.renderLine('chartTrend', this.kpis.monthlyProcurementTrend?.map((x: any) => x.label), this.kpis.monthlyProcurementTrend?.map((x: any) => x.count));
    }, 50);
  }

  renderBar(id: string, labels: string[], data: number[], label: string, color: string) {
    const el = document.getElementById(id) as HTMLCanvasElement;
    if (!el) return;
    this.charts.push(new Chart(el, { type: 'bar', data: { labels, datasets: [{ label, data, backgroundColor: color }] }, options: { responsive: true, plugins: { legend: { display: false } } } }));
  }

  renderDoughnut(id: string, labels: string[], data: number[]) {
    const el = document.getElementById(id) as HTMLCanvasElement;
    if (!el) return;
    const colors = ['#5E35B1','#1976D2','#388E3C','#F57C00','#D32F2F','#0097A7','#7B1FA2'];
    this.charts.push(new Chart(el, { type: 'doughnut', data: { labels, datasets: [{ data, backgroundColor: colors }] }, options: { responsive: true } }));
  }

  renderLine(id: string, labels: string[], data: number[]) {
    const el = document.getElementById(id) as HTMLCanvasElement;
    if (!el) return;
    this.charts.push(new Chart(el, { type: 'line', data: { labels, datasets: [{ label: 'Procured', data, borderColor: '#5E35B1', tension: 0.3, fill: false }] }, options: { responsive: true } }));
  }
}
