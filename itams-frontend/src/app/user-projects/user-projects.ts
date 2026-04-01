import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Api } from '../services/api';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-user-projects',
  imports: [CommonModule, FormsModule],
  templateUrl: './user-projects.html',
  styleUrl: './user-projects.scss',
})
export class UserProjects implements OnInit {
  projects: any[] = [];
  loading = false;
  error = '';
  success = '';

  // Make Math available in template
  Math = Math;

  // Filter and search
  searchTerm = '';
  statusFilter = 'all';
  sortBy = 'name';
  sortOrder = 'asc';

  // Pagination
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;

  // Current user info (from AuthService)
  get currentUser() {
    const user = this.authService.currentUser;
    if (user) {
      return {
        id: user.id,
        name: `${user.firstName} ${user.lastName}`,
        role: user.roleName
      };
    }
    return {
      id: 0,
      name: 'User',
      role: 'User'
    };
  }

  constructor(
    private api: Api,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadUserProjects();
  }

  loadUserProjects() {
    this.loading = true;
    this.api.getMyProject().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const data = response.data;

          // Auditor gets all projects
          if (data.isAuditor && data.projects) {
            this.projects = data.projects.map((p: any) => ({
              id: p.id,
              name: p.preferredName || p.name,
              code: p.code,
              description: p.description || 'No description available',
              status: p.isActive ? 'Active' : 'Inactive',
              progress: 0,
              role: 'Auditor',
              dueDate: 'N/A',
              locations: p.locations?.map((l: any) => l.name) ?? []
            }));
          } else {
            // Single project for regular users
            this.projects = [{
              id: data.id,
              name: data.name,
              preferredName: data.preferredName,
              code: data.code,
              description: data.description || 'No description available',
              status: data.isActive ? 'Active' : 'Inactive',
              progress: 0,
              role: data.userRole,
              dueDate: 'N/A',
              locations: data.locations?.map((l: any) => l.name || l) ?? []
            }];
          }
          this.totalItems = this.projects.length;
        } else {
          this.projects = [];
          this.totalItems = 0;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading project:', error);
        this.error = 'Failed to load your assigned project';
        this.projects = [];
        this.totalItems = 0;
        this.loading = false;
      }
    });
  }

  get filteredProjects() {
    let filtered = [...this.projects];

    // Apply search filter
    if (this.searchTerm) {
      filtered = filtered.filter(project => 
        project.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        project.code.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        project.description.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    }

    // Apply status filter
    if (this.statusFilter !== 'all') {
      filtered = filtered.filter(project => 
        project.status.toLowerCase() === this.statusFilter.toLowerCase()
      );
    }

    // Apply sorting
    filtered.sort((a, b) => {
      let aValue = a[this.sortBy];
      let bValue = b[this.sortBy];
      
      if (typeof aValue === 'string') {
        aValue = aValue.toLowerCase();
        bValue = bValue.toLowerCase();
      }
      
      if (this.sortOrder === 'asc') {
        return aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
      } else {
        return aValue > bValue ? -1 : aValue < bValue ? 1 : 0;
      }
    });

    return filtered;
  }

  get paginatedProjects() {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredProjects.slice(startIndex, startIndex + this.itemsPerPage);
  }

  get totalPages() {
    return Math.ceil(this.filteredProjects.length / this.itemsPerPage);
  }

  onSearch() {
    this.currentPage = 1; // Reset to first page when searching
  }

  onSort(field: string) {
    if (this.sortBy === field) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = field;
      this.sortOrder = 'asc';
    }
  }

  onPageChange(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  getProgressColor(progress: number): string {
    if (progress >= 80) return 'success';
    if (progress >= 50) return 'warning';
    return 'danger';
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return 'bg-success';
      case 'in progress': return 'bg-warning';
      case 'planning': return 'bg-info';
      case 'completed': return 'bg-primary';
      case 'on hold': return 'bg-secondary';
      case 'cancelled': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }

  getRoleColor(role: string): string {
    switch (role.toLowerCase()) {
      case 'project manager': return 'text-primary';
      case 'technical lead': return 'text-success';
      case 'team member': return 'text-info';
      default: return 'text-secondary';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }

  viewProjectDetails(project: any) {
    // Navigate to My Assets — the assets page will show assets for this project
    this.router.navigate(['/user/assets']);
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }
}