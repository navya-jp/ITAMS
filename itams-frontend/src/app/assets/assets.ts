import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Api, Asset, CreateAsset, Project, Location } from '../services/api';

interface BulkUploadResult {
  totalRows: number;
  successCount: number;
  failedCount: number;
  errors: BulkUploadError[];
  message: string;
}

interface BulkUploadError {
  rowNumber: number;
  assetTag: string;
  errorMessage: string;
}

@Component({
  selector: 'app-assets',
  imports: [CommonModule, FormsModule],
  templateUrl: './assets.html',
  styleUrl: './assets.scss',
})
export class Assets implements OnInit {
  assets: Asset[] = [];
  projects: Project[] = [];
  locations: Location[] = [];
  projectUsers: any[] = []; // Users in the selected project
  loading = false;
  error = '';
  success = '';

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  showViewModal = false;
  showBulkUploadModal = false;
  selectedAsset: Asset | null = null;
  currentTab = 1;
  maxTab = 1;

  // Bulk upload
  selectedFile: File | null = null;
  uploading = false;
  uploadResult: BulkUploadResult | null = null;
  dragOver = false;
  private baseUrl = '/api'; // Use relative URL with proxy

  // Form data
  createForm: CreateAsset = {
    assetTag: '',
    projectId: 0,
    locationId: 0,
    usageCategory: 'TMS',
    criticality: 'TMSCritical',
    assetType: '',
    subType: '',
    make: '',
    model: '',
    serialNumber: '',
    procurementDate: undefined,
    procurementCost: undefined,
    vendor: '',
    warrantyStartDate: undefined,
    warrantyEndDate: undefined,
    commissioningDate: undefined,
    status: 'InUse',
    assignedUserId: undefined,
    assignedUserRole: ''
  };

  editForm: Partial<CreateAsset> = {};

  // Constants
  usageCategories = ['TMS', 'ITNonTMS'];
  criticalities = ['TMSCritical', 'TMSGeneral', 'ITCritical', 'ITGeneral'];
  statuses = ['InUse', 'Spare', 'Repair', 'Decommissioned', 'Unknown'];
  assetTypes = ['Hardware', 'Software', 'Digital'];

  // Validation
  validationErrors: { [key: string]: string } = {};

  constructor(private api: Api, private http: HttpClient) {}

  ngOnInit() {
    this.loadAssets();
    this.loadLocations();
  }

  loadAssets() {
    this.loading = true;
    this.api.getAssets().subscribe({
      next: (assets) => {
        this.assets = assets;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load assets';
        this.loading = false;
        console.error('Error loading assets:', error);
      }
    });
  }

  loadLocations() {
    this.api.getLocations().subscribe({
      next: (locations) => {
        this.locations = locations;
      },
      error: (error) => {
        console.error('Error loading locations:', error);
      }
    });
  }

  // Modal management
  openCreateModal() {
    this.resetCreateForm();
    this.showCreateModal = true;
    this.currentTab = 1;
    this.maxTab = 1;
    this.clearMessages();
  }

  openEditModal(asset: Asset) {
    this.selectedAsset = asset;
    this.editForm = {
      assetTag: asset.assetTag,
      locationId: asset.locationId,
      assetType: asset.assetType,
      subType: asset.subType,
      make: asset.make,
      model: asset.model,
      serialNumber: asset.serialNumber,
      procurementDate: asset.procurementDate,
      procurementCost: asset.procurementCost,
      vendor: asset.vendor,
      warrantyStartDate: asset.warrantyStartDate,
      warrantyEndDate: asset.warrantyEndDate,
      commissioningDate: asset.commissioningDate,
      status: asset.status,
      assignedUserId: asset.assignedUserId,
      assignedUserRole: asset.assignedUserRole
    };
    
    // Load users from the asset's project
    this.loadProjectUsers(asset.projectId);
    
    this.showEditModal = true;
    this.clearMessages();
  }

  loadProjectUsers(projectId: number) {
    this.api.getUsers().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Filter users by project
          this.projectUsers = response.data.filter((user: any) => user.projectId === projectId);
        }
      },
      error: (error) => {
        console.error('Error loading project users:', error);
        this.projectUsers = [];
      }
    });
  }

  openViewModal(asset: Asset) {
    this.selectedAsset = asset;
    this.showViewModal = true;
    this.clearMessages();
  }

  closeModals() {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.showViewModal = false;
    this.showBulkUploadModal = false;
    this.selectedAsset = null;
    this.selectedFile = null;
    this.uploadResult = null;
    this.clearMessages();
    this.clearValidationErrors();
  }

  // Bulk Upload Methods
  openBulkUploadModal() {
    this.showBulkUploadModal = true;
    this.selectedFile = null;
    this.uploadResult = null;
    this.clearMessages();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.uploadResult = null;
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;

    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      if (file.name.endsWith('.xlsx')) {
        this.selectedFile = file;
        this.uploadResult = null;
      } else {
        alert('Please select an Excel file (.xlsx)');
      }
    }
  }

  clearFile() {
    this.selectedFile = null;
    this.uploadResult = null;
  }

  async uploadFile() {
    if (!this.selectedFile) {
      alert('Please select a file first');
      return;
    }

    this.uploading = true;
    this.uploadResult = null;

    try {
      const formData = new FormData();
      formData.append('file', this.selectedFile);

      const token = localStorage.getItem('auth_token'); // Fixed: use 'auth_token' not 'token'
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });

      const result = await this.http.post<BulkUploadResult>(
        `${this.baseUrl}/assets/bulk-upload`, 
        formData,
        { headers }
      ).toPromise();
      
      if (result) {
        this.uploadResult = result;
        
        if (result.successCount > 0) {
          this.success = `Successfully uploaded ${result.successCount} assets!`;
          // Reload assets to show new ones
          this.loadAssets();
        }
        
        if (result.failedCount > 0) {
          this.error = `${result.failedCount} rows failed. Please check the error details below.`;
        }
      }
    } catch (error: any) {
      console.error('Upload error:', error);
      this.error = error.error?.message || 'An error occurred during upload';
    } finally {
      this.uploading = false;
    }
  }

  downloadTemplate() {
    window.open(`${this.baseUrl}/assets/download-template`, '_blank');
  }

  getFileName(): string {
    return this.selectedFile?.name || '';
  }

  getFileSize(): string {
    if (!this.selectedFile) return '';
    const sizeInMB = (this.selectedFile.size / (1024 * 1024)).toFixed(2);
    return `${sizeInMB} MB`;
  }

  // Tab navigation
  goToTab(tabNumber: number) {
    if (tabNumber <= this.maxTab) {
      this.currentTab = tabNumber;
    }
  }

  nextTab() {
    if (this.validateCurrentTab()) {
      this.currentTab++;
      this.maxTab = Math.max(this.maxTab, this.currentTab);
    }
  }

  previousTab() {
    if (this.currentTab > 1) {
      this.currentTab--;
    }
  }

  // Validation
  validateCurrentTab(): boolean {
    this.clearValidationErrors();
    
    if (this.currentTab === 1) {
      return this.validateTab1();
    } else if (this.currentTab === 2) {
      return this.validateTab2();
    }
    
    return true;
  }

  validateTab1(): boolean {
    let isValid = true;

    if (!this.createForm.locationId) {
      this.validationErrors['locationId'] = 'Location is required';
      isValid = false;
    }

    if (!this.createForm.assetTag) {
      this.validationErrors['assetTag'] = 'Asset tag is required';
      isValid = false;
    }

    return isValid;
  }

  validateTab2(): boolean {
    let isValid = true;

    if (!this.createForm.assetType) {
      this.validationErrors['assetType'] = 'Asset type is required';
      isValid = false;
    }

    if (!this.createForm.make) {
      this.validationErrors['make'] = 'Make is required';
      isValid = false;
    }

    if (!this.createForm.model) {
      this.validationErrors['model'] = 'Model is required';
      isValid = false;
    }

    return isValid;
  }

  // CRUD operations
  createAsset() {
    if (!this.validateTab1() || !this.validateTab2()) {
      return;
    }

    this.loading = true;
    this.api.createAsset(this.createForm).subscribe({
      next: (asset) => {
        this.assets.push(asset);
        this.success = 'Asset created successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to create asset';
        this.loading = false;
        console.error('Error creating asset:', error);
      }
    });
  }

  updateAsset() {
    if (!this.selectedAsset) return;

    this.loading = true;
    this.api.updateAsset(this.selectedAsset.id, this.editForm).subscribe({
      next: () => {
        this.success = 'Asset updated successfully';
        this.loading = false;
        this.closeModals();
        this.loadAssets();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to update asset';
        this.loading = false;
        console.error('Error updating asset:', error);
      }
    });
  }

  deleteAsset(asset: Asset) {
    if (confirm(`Are you sure you want to delete asset ${asset.assetId}?`)) {
      this.loading = true;
      this.api.deleteAsset(asset.id).subscribe({
        next: () => {
          this.success = 'Asset deleted successfully';
          this.loading = false;
          this.loadAssets();
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to delete asset';
          this.loading = false;
          console.error('Error deleting asset:', error);
        }
      });
    }
  }

  // Utility methods
  resetCreateForm() {
    this.createForm = {
      assetTag: '',
      projectId: 0,
      locationId: 0,
      usageCategory: 'TMS',
      criticality: 'TMSCritical',
      assetType: '',
      subType: '',
      make: '',
      model: '',
      serialNumber: '',
      procurementDate: undefined,
      procurementCost: undefined,
      vendor: '',
      warrantyStartDate: undefined,
      warrantyEndDate: undefined,
      commissioningDate: undefined,
      status: 'InUse',
      assignedUserId: undefined,
      assignedUserRole: ''
    };
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }

  clearValidationErrors() {
    this.validationErrors = {};
  }

  clearValidationError(field: string) {
    delete this.validationErrors[field];
  }

  hasValidationError(field: string): boolean {
    return !!this.validationErrors[field];
  }

  getValidationError(field: string): string {
    return this.validationErrors[field] || '';
  }

  isTabValid(tabNumber: number): boolean {
    if (tabNumber === 1) {
      return !!(this.createForm.locationId && this.createForm.assetTag);
    } else if (tabNumber === 2) {
      return !!(this.createForm.assetType && this.createForm.make && this.createForm.model);
    }
    return true;
  }

  formatDate(date: Date | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString();
  }

  formatCurrency(amount: number | undefined): string {
    if (!amount) return 'N/A';
    return `₹${amount.toLocaleString()}`;
  }
}
