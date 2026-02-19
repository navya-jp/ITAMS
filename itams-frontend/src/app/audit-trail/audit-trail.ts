import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
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
  imports: [CommonModule],
  templateUrl: './audit-trail.html',
  styleUrls: ['./audit-trail.scss']
})
export class AuditTrail implements OnInit {
  loginAudits: LoginAudit[] = [];
  loading = false;
  error: string | null = null;

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadLoginAudits();
  }

  loadLoginAudits() {
    this.loading = true;
    this.error = null;

    this.api.getLoginAudits(100)
      .subscribe({
        next: (data: LoginAudit[]) => {
          this.loginAudits = data;
          this.loading = false;
        },
        error: (err: any) => {
          this.error = 'Failed to load login audit data';
          this.loading = false;
          console.error('Error loading login audits:', err);
        }
      });
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
}
