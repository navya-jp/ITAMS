import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Api, ApiResponse, User } from '../services/api';
import { ReportService } from '../reports/report.service';

declare var Chart: any;

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit, AfterViewInit {
  totalUsers = 0;
  activeUsers = 0;
  totalProjects = 0;
  totalLocations = 0;

  kpis: any = null;
  charts: any[] = [];

  constructor(private api: Api, private reportSvc: ReportService) {}

  ngOnInit() {
    this.loadStats();
    this.loadKpis();
  }

  ngAfterViewInit() {}

  private loadStats() {
    this.api.getUsers().subscribe({
      next: (response: ApiResponse<User[]>) => {
        if (response.success && response.data) {
          this.totalUsers = response.data.length;
          this.activeUsers = response.data.filter((u: User) => u.isActive && !u.isLocked).length;
        }
      }
    });
    this.api.getProjects().subscribe({ next: (p) => this.totalProjects = p.length });
    this.api.getLocations().subscribe({ next: (l) => this.totalLocations = l.length });
  }

  private loadKpis() {
    this.reportSvc.getDashboardKpis().subscribe({
      next: (data) => {
        this.kpis = data;
        setTimeout(() => this.renderCharts(), 150);
      },
      error: (err) => {
        console.error('KPI load failed:', err);
        // Set empty kpis so spinner stops
        this.kpis = { totalHardwareAssets: 0, assetsInUse: 0, assetsInRepair: 0, unacknowledgedAlerts: 0, assetsByType: [], assetsByStatus: [], assetsByLocation: [], monthlyProcurementTrend: [] };
      }
    });
  }

  private destroyCharts() {
    this.charts.forEach(c => { try { c.destroy(); } catch {} });
    this.charts = [];
  }

  private renderCharts() {
    if (!this.kpis) return;
    this.destroyCharts();

    // Assets by Type — horizontal bar
    this.renderHBar('chartByType',
      this.kpis.assetsByType?.map((x: any) => x.label) ?? [],
      this.kpis.assetsByType?.map((x: any) => x.count) ?? [],
      'Assets by Type');

    // Assets by Status — doughnut
    this.renderDoughnut('chartByStatus',
      this.kpis.assetsByStatus?.map((x: any) => x.label) ?? [],
      this.kpis.assetsByStatus?.map((x: any) => x.count) ?? []);

    // Top locations — bar
    this.renderBar('chartByLocation',
      this.kpis.assetsByLocation?.map((x: any) => x.label) ?? [],
      this.kpis.assetsByLocation?.map((x: any) => x.count) ?? [],
      'Assets', '#1976D2');

    // Monthly trend — line
    this.renderLine('chartTrend',
      this.kpis.monthlyProcurementTrend?.map((x: any) => x.label) ?? [],
      this.kpis.monthlyProcurementTrend?.map((x: any) => x.count) ?? []);
  }

  private renderHBar(id: string, labels: string[], data: number[], label: string) {
    const el = document.getElementById(id) as HTMLCanvasElement;
    if (!el) return;
    this.charts.push(new Chart(el, {
      type: 'bar',
      data: { labels, datasets: [{ label, data, backgroundColor: '#5E35B1' }] },
      options: { indexAxis: 'y', responsive: true, plugins: { legend: { display: false } } }
    }));
  }

  private renderBar(id: string, labels: string[], data: number[], label: string, color: string) {
    const el = document.getElementById(id) as HTMLCanvasElement;
    if (!el) return;
    this.charts.push(new Chart(el, {
      type: 'bar',
      data: { labels, datasets: [{ label, data, backgroundColor: color }] },
      options: { responsive: true, plugins: { legend: { display: false } } }
    }));
  }

  private renderDoughnut(id: string, labels: string[], data: number[]) {
    const el = document.getElementById(id) as HTMLCanvasElement;
    if (!el) return;
    const colors = ['#388E3C', '#F57C00', '#D32F2F', '#1976D2', '#7B1FA2', '#0097A7'];
    this.charts.push(new Chart(el, {
      type: 'doughnut',
      data: { labels, datasets: [{ data, backgroundColor: colors }] },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'top' } } }
    }));
  }

  private renderLine(id: string, labels: string[], data: number[]) {
    const el = document.getElementById(id) as HTMLCanvasElement;
    if (!el) return;
    this.charts.push(new Chart(el, {
      type: 'line',
      data: { labels, datasets: [{ label: 'Procured', data, borderColor: '#5E35B1', tension: 0.3, fill: true, backgroundColor: 'rgba(94,53,177,0.1)' }] },
      options: { responsive: true }
    }));
  }
}
