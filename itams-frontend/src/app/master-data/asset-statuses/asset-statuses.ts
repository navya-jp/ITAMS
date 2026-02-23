import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MasterDataService, AssetStatus, CreateAssetStatusRequest, UpdateAssetStatusRequest } from '../../services/master-data.service';

@Component({
  selector: 'app-asset-statuses',
  imports: [CommonModule, FormsModule],
  templateUrl: './asset-statuses.html',
  styleUrl: './asset-statuses.scss',
})
export class AssetStatuses implements OnInit {
  statuses: AssetStatus[] = [];
  filteredStatuses: AssetStatus[] = [];
  loading = false;
  showForm = false;
  editMode = false;
  includeInactive = false;
  searchTerm = '';

  // Form model
  statusForm: CreateAssetStatusRequest & { id?: number; isActive?: boolean; isPredefined?: boolean } = {
    statusName: '',
    statusCode: '',
    description: '',
    colorCode: '#6c757d',
    icon: '',
    isActive: true
  };

  errorMessage = '';
  successMessage = '';

  // Predefined color options
  colorOptions = [
    { name: 'Primary', value: '#0d6efd' },
    { name: 'Success', value: '#198754' },
    { name: 'Warning', value: '#ffc107' },
    { name: 'Danger', value: '#dc3545' },
    { name: 'Info', value: '#0dcaf0' },
    { name: 'Secondary', value: '#6c757d' },
    { name: 'Dark', value: '#212529' }
  ];

  constructor(private masterDataService: MasterDataService) {}

  ngOnInit() {
    this.loadStatuses();
  }

  loadStatuses() {
    this.loading = true;
    this.masterDataService.getAssetStatuses(this.includeInactive).subscribe({
      next: (data) => {
        this.statuses = data;
        this.filterStatuses();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading asset statuses:', error);
        this.showError('Failed to load asset statuses');
        this.loading = false;
      }
    });
  }

  filterStatuses() {
    if (!this.searchTerm) {
      this.filteredStatuses = this.statuses;
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredStatuses = this.statuses.filter(s =>
        s.statusName.toLowerCase().includes(term) ||
        s.statusCode.toLowerCase().includes(term)
      );
    }
  }

  onSearchChange() {
    this.filterStatuses();
  }

  toggleIncludeInactive() {
    this.includeInactive = !this.includeInactive;
    this.loadStatuses();
  }

  showAddForm() {
    this.resetForm();
    this.editMode = false;
    this.showForm = true;
  }

  editStatus(status: AssetStatus) {
    if (status.isPredefined) {
      this.showError('Cannot edit predefined status');
      return;
    }

    this.statusForm = {
      id: status.id,
      statusName: status.statusName,
      statusCode: status.statusCode,
      description: status.description || '',
      colorCode: status.colorCode,
      icon: status.icon || '',
      isActive: status.isActive,
      isPredefined: status.isPredefined
    };
    this.editMode = true;
    this.showForm = true;
  }

  cancelForm() {
    this.showForm = false;
    this.resetForm();
  }

  resetForm() {
    this.statusForm = {
      statusName: '',
      statusCode: '',
      description: '',
      colorCode: '#6c757d',
      icon: '',
      isActive: true
    };
    this.errorMessage = '';
  }

  saveStatus() {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.editMode && this.statusForm.id) {
      const updateRequest: UpdateAssetStatusRequest = {
        statusName: this.statusForm.statusName,
        description: this.statusForm.description,
        colorCode: this.statusForm.colorCode,
        icon: this.statusForm.icon,
        isActive: this.statusForm.isActive || true
      };

      this.masterDataService.updateAssetStatus(this.statusForm.id, updateRequest).subscribe({
        next: () => {
          this.showSuccess('Asset status updated successfully');
          this.loadStatuses();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error updating asset status:', error);
          this.showError(error.error?.message || 'Failed to update asset status');
          this.loading = false;
        }
      });
    } else {
      const createRequest: CreateAssetStatusRequest = {
        statusName: this.statusForm.statusName,
        statusCode: this.statusForm.statusCode,
        description: this.statusForm.description,
        colorCode: this.statusForm.colorCode,
        icon: this.statusForm.icon
      };

      this.masterDataService.createAssetStatus(createRequest).subscribe({
        next: () => {
          this.showSuccess('Asset status created successfully');
          this.loadStatuses();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error creating asset status:', error);
          this.showError(error.error?.message || 'Failed to create asset status');
          this.loading = false;
        }
      });
    }
  }

  deleteStatus(status: AssetStatus) {
    if (status.isPredefined) {
      this.showError('Cannot delete predefined status');
      return;
    }

    if (!confirm(`Are you sure you want to delete status "${status.statusName}"?`)) {
      return;
    }

    this.loading = true;
    this.masterDataService.deleteAssetStatus(status.id).subscribe({
      next: () => {
        this.showSuccess('Asset status deleted successfully');
        this.loadStatuses();
      },
      error: (error) => {
        console.error('Error deleting asset status:', error);
        this.showError(error.error?.message || 'Failed to delete asset status');
        this.loading = false;
      }
    });
  }

  validateForm(): boolean {
    if (!this.statusForm.statusName.trim()) {
      this.showError('Status name is required');
      return false;
    }
    if (!this.editMode && !this.statusForm.statusCode.trim()) {
      this.showError('Status code is required');
      return false;
    }
    if (!this.statusForm.colorCode.trim()) {
      this.showError('Color code is required');
      return false;
    }
    return true;
  }

  showError(message: string) {
    this.errorMessage = message;
    setTimeout(() => this.errorMessage = '', 5000);
  }

  showSuccess(message: string) {
    this.successMessage = message;
    setTimeout(() => this.successMessage = '', 3000);
  }
}
