import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../services/api';

interface LoginAudit {
  id: number;
  userId: number;
  username: string;
  loginTime: string;
  logoutTime?: string;
  ipAddress?: string;
  browserType?: string;
  operatingSystem?: string;
  sessionId?: string;
  status: string;
}

@Component({
  selector: 'app-audit-trail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-trail.html',
  styleUrls: ['./audit-trail.scss']
})
export class AuditTrail implements OnInit {
  loginAudits: LoginAudit[] = [];
  filteredAudits: LoginAudit[] = [];
  loading = false;
  error: string | null = null;
  
  // Time range filter
  selectedTimeRange: 'today' | 'week' | 'month' | 'year' | 'custom' = 'today';
  timeRanges = [
    { value: 'today', label: 'Today', days: 1 },
    { value: 'week', label: 'Last Week', days: 7 },
    { value: 'month', label: 'Last Month', days: 30 },
    { value: 'year', label: 'Last Year', days: 365 },
    { value: 'custom', label: 'Custom Range', days: 0 }
  ];
  
  // Custom date range
  customStartDate: string = '';
  customEndDate: string = '';
  
  // Username filter
  usernameFilter: string = '';

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadLoginAudits();
  }

  onTimeRangeChange() {
    if (this.selectedTimeRange !== 'custom') {
      this.loadLoginAudits();
    }
  }
  
  onCustomDateChange() {
    if (this.customStartDate && this.customEndDate) {
      this.loadLoginAudits();
    }
  }
  
  onUsernameFilterChange() {
    this.applyUsernameFilter();
  }
  
  clearFilters() {
    this.selectedTimeRange = 'today';
    this.customStartDate = '';
    this.customEndDate = '';
    this.usernameFilter = '';
    this.loadLoginAudits();
  }

  loadLoginAudits() {
    this.loading = true;
    this.error = null;

    // Calculate date range based on selected time range
    let startDate: Date;
    let endDate = new Date();
    
    if (this.selectedTimeRange === 'custom') {
      if (!this.customStartDate || !this.customEndDate) {
        this.loading = false;
        return;
      }
      startDate = new Date(this.customStartDate);
      endDate = new Date(this.customEndDate);
      endDate.setHours(23, 59, 59, 999); // End of day
    } else {
      const selectedRange = this.timeRanges.find(r => r.value === this.selectedTimeRange);
      
      if (this.selectedTimeRange === 'today') {
        // For today, start from beginning of today
        startDate = new Date();
        startDate.setHours(0, 0, 0, 0);
      } else if (selectedRange && selectedRange.days > 0) {
        // For other ranges, go back N days from today
        startDate = new Date();
        startDate.setDate(startDate.getDate() - selectedRange.days);
        startDate.setHours(0, 0, 0, 0);
      } else {
        startDate = new Date();
        startDate.setHours(0, 0, 0, 0);
      }
    }

    this.api.getLoginAudits(1000, startDate, endDate)
      .subscribe({
        next: (data: LoginAudit[]) => {
          this.loginAudits = data;
          this.applyUsernameFilter();
          this.loading = false;
        },
        error: (err: any) => {
          this.error = 'Failed to load login audit data';
          this.loading = false;
          console.error('Error loading login audits:', err);
        }
      });
  }
  
  applyUsernameFilter() {
    if (!this.usernameFilter.trim()) {
      this.filteredAudits = this.loginAudits;
    } else {
      const filter = this.usernameFilter.toLowerCase().trim();
      this.filteredAudits = this.loginAudits.filter(audit => 
        audit.username.toLowerCase().includes(filter)
      );
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'ACTIVE':
        return 'badge bg-success';
      case 'LOGGED_OUT':
        return 'badge bg-secondary';
      case 'SESSION_TIMEOUT':
        return 'badge bg-warning';
      case 'FORCED_LOGOUT':
        return 'badge bg-danger';
      default:
        return 'badge bg-secondary';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'ACTIVE':
        return 'Active';
      case 'LOGGED_OUT':
        return 'Logged Out';
      case 'SESSION_TIMEOUT':
        return 'Session Timeout';
      case 'FORCED_LOGOUT':
        return 'Forced Logout';
      default:
        return status;
    }
  }

  formatDateTime(dateTime: string | undefined): string {
    if (!dateTime) return '-';
    return new Date(dateTime).toLocaleString();
  }

  calculateSessionDuration(loginTime: string, logoutTime?: string): string {
    if (!logoutTime) return 'Active';
    
    const login = new Date(loginTime);
    const logout = new Date(logoutTime);
    const diff = logout.getTime() - login.getTime();
    
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    
    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    }
    return `${minutes}m`;
  }

  formatIpAddress(ipAddress: string | undefined): string {
    if (!ipAddress) return 'Unknown';
    // Remove (localhost) suffix for display
    return ipAddress.replace(/\s*\(localhost\)\s*$/i, '').trim();
  }
  
  getSelectedRangeLabel(): string {
    const range = this.timeRanges.find(r => r.value === this.selectedTimeRange);
    return range ? range.label : 'Today';
  }
}
