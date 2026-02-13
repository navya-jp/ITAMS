import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadUserProjects();
  }

  loadUserProjects() {
    this.loading = true;
    
    // Get user's assigned project from API
    this.api.getMyProject().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const project = response.data;
          this.projects = [{
            id: project.id,
            name: project.name,
            preferredName: project.preferredName,
            code: project.code,
            description: project.description || 'No description available',
            status: project.isActive ? 'Active' : 'Inactive',
            progress: 0, // TODO: Calculate actual progress
            role: project.userRole,
            dueDate: 'N/A', // TODO: Add due date field
            budget: 0, // TODO: Add budget field
            spent: 0, // TODO: Add spent field
            team: [], // TODO: Get team members
            locations: Array(project.locationCount).fill('Location') // Placeholder
          }];
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
    // TODO: Navigate to project details page or open modal
    console.log('View project details:', project);
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }
}