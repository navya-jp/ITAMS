import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MasterDataService, CriticalityLevel, CreateCriticalityLevelRequest, UpdateCriticalityLevelRequest } from '../../services/master-data.service';

@Component({
  selector: 'app-criticality-levels',
  imports: [CommonModule, FormsModule],
  templateUrl: './criticality-levels.html',
  styleUrl: './criticality-levels.scss',
})
export class CriticalityLevels implements OnInit {
  levels: CriticalityLevel[] = [];
  filteredLevels: CriticalityLevel[] = [];
  loading = false;
  showForm = false;
  editMode = false;
  includeInactive = false;
  searchTerm = '';

  // Form model
  levelForm: CreateCriticalityLevelRequest & { id?: number; isActive?: boolean; isPredefined?: boolean } = {
    levelName: '',
    levelCode: '',
    description: '',
    priorityOrder: 1,
    slaHours: 24,
    priorityLevel: 'Medium',
    notificationThresholdDays: 7,
    isActive: true
  };

  errorMessage = '';
  successMessage = '';

  // Priority level options
  priorityLevelOptions = ['Critical', 'High', 'Medium', 'Low'];

  constructor(private masterDataService: MasterDataService) {}

  ngOnInit() {
    this.loadLevels();
  }

  loadLevels() {
    this.loading = true;
    this.masterDataService.getCriticalityLevels(this.includeInactive).subscribe({
      next: (data) => {
        this.levels = data.sort((a, b) => a.priorityOrder - b.priorityOrder);
        this.filterLevels();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading criticality levels:', error);
        this.showError('Failed to load criticality levels');
        this.loading = false;
      }
    });
  }

  filterLevels() {
    if (!this.searchTerm) {
      this.filteredLevels = this.levels;
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredLevels = this.levels.filter(l =>
        l.levelName.toLowerCase().includes(term) ||
        l.levelCode.toLowerCase().includes(term) ||
        l.priorityLevel.toLowerCase().includes(term)
      );
    }
  }

  onSearchChange() {
    this.filterLevels();
  }

  toggleIncludeInactive() {
    this.includeInactive = !this.includeInactive;
    this.loadLevels();
  }

  showAddForm() {
    this.resetForm();
    this.editMode = false;
    this.showForm = true;
  }

  editLevel(level: CriticalityLevel) {
    if (level.isPredefined) {
      this.showError('Cannot edit predefined criticality level');
      return;
    }

    this.levelForm = {
      id: level.id,
      levelName: level.levelName,
      levelCode: level.levelCode,
      description: level.description || '',
      priorityOrder: level.priorityOrder,
      slaHours: level.slaHours,
      priorityLevel: level.priorityLevel,
      notificationThresholdDays: level.notificationThresholdDays,
      isActive: level.isActive,
      isPredefined: level.isPredefined
    };
    this.editMode = true;
    this.showForm = true;
  }

  cancelForm() {
    this.showForm = false;
    this.resetForm();
  }

  resetForm() {
    this.levelForm = {
      levelName: '',
      levelCode: '',
      description: '',
      priorityOrder: 1,
      slaHours: 24,
      priorityLevel: 'Medium',
      notificationThresholdDays: 7,
      isActive: true
    };
    this.errorMessage = '';
  }

  saveLevel() {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.editMode && this.levelForm.id) {
      const updateRequest: UpdateCriticalityLevelRequest = {
        levelName: this.levelForm.levelName,
        description: this.levelForm.description,
        priorityOrder: this.levelForm.priorityOrder,
        slaHours: this.levelForm.slaHours,
        priorityLevel: this.levelForm.priorityLevel,
        notificationThresholdDays: this.levelForm.notificationThresholdDays,
        isActive: this.levelForm.isActive || true
      };

      this.masterDataService.updateCriticalityLevel(this.levelForm.id, updateRequest).subscribe({
        next: () => {
          this.showSuccess('Criticality level updated successfully');
          this.loadLevels();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error updating criticality level:', error);
          this.showError(error.error?.message || 'Failed to update criticality level');
          this.loading = false;
        }
      });
    } else {
      const createRequest: CreateCriticalityLevelRequest = {
        levelName: this.levelForm.levelName,
        levelCode: this.levelForm.levelCode,
        description: this.levelForm.description,
        priorityOrder: this.levelForm.priorityOrder,
        slaHours: this.levelForm.slaHours,
        priorityLevel: this.levelForm.priorityLevel,
        notificationThresholdDays: this.levelForm.notificationThresholdDays
      };

      this.masterDataService.createCriticalityLevel(createRequest).subscribe({
        next: () => {
          this.showSuccess('Criticality level created successfully');
          this.loadLevels();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error creating criticality level:', error);
          this.showError(error.error?.message || 'Failed to create criticality level');
          this.loading = false;
        }
      });
    }
  }

  deleteLevel(level: CriticalityLevel) {
    if (level.isPredefined) {
      this.showError('Cannot delete predefined criticality level');
      return;
    }

    if (!confirm(`Are you sure you want to delete criticality level "${level.levelName}"?`)) {
      return;
    }

    this.loading = true;
    this.masterDataService.deleteCriticalityLevel(level.id).subscribe({
      next: () => {
        this.showSuccess('Criticality level deleted successfully');
        this.loadLevels();
      },
      error: (error) => {
        console.error('Error deleting criticality level:', error);
        this.showError(error.error?.message || 'Failed to delete criticality level');
        this.loading = false;
      }
    });
  }

  validateForm(): boolean {
    if (!this.levelForm.levelName.trim()) {
      this.showError('Level name is required');
      return false;
    }
    if (!this.editMode && !this.levelForm.levelCode.trim()) {
      this.showError('Level code is required');
      return false;
    }
    if (this.levelForm.priorityOrder < 1) {
      this.showError('Priority order must be at least 1');
      return false;
    }
    if (this.levelForm.slaHours < 1) {
      this.showError('SLA hours must be at least 1');
      return false;
    }
    if (this.levelForm.notificationThresholdDays < 0) {
      this.showError('Notification threshold cannot be negative');
      return false;
    }
    return true;
  }

  getPriorityBadgeClass(priority: string): string {
    switch (priority.toLowerCase()) {
      case 'critical': return 'bg-danger';
      case 'high': return 'bg-warning';
      case 'medium': return 'bg-info';
      case 'low': return 'bg-secondary';
      default: return 'bg-secondary';
    }
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
