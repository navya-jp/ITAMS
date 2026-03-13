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
  filteredAssets: Asset[] = [];
  projects: Project[] = [];
  locations: Location[] = [];
  projectUsers: any[] = []; // Users in the selected project
  loading = false;
  error = '';
  success = '';

  // Search and filter
  searchTerm = '';
  filterStatus = 'all';
  filterType = 'all';

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  showViewModal = false;
  showBulkUploadModal = false;
  selectedAsset: Asset | null = null;
  currentTab = 1;
  maxTab = 1;
  
  // Software asset tab
  softwareTab = 1;
  softwareMaxTab = 1;
  
  // View/Edit modal tabs
  viewTab = 1;
  editTab = 1;

  // Bulk upload
  selectedFile: File | null = null;
  uploading = false;
  uploadResult: BulkUploadResult | null = null;
  dragOver = false;
  private baseUrl = '/api'; // Use relative URL with proxy

  // Form data
  selectedAssetType: 'Hardware' | 'Software' = 'Hardware';
  
  createForm: CreateAsset = {
    assetTag: '',
    projectId: 0,
    locationId: 0,
    usageCategory: 'TMS',
    classification: '',
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
    status: 'In Use',
    placing: '',
    assignedUserId: undefined,
    assignedUserRole: ''
  };

  // Software asset form
  softwareForm: any = {
    softwareName: '',
    version: '',
    licenseKey: '',
    licenseType: '',
    numberOfLicenses: 1,
    purchaseDate: undefined,
    validityStartDate: undefined,
    validityEndDate: undefined,
    assetTag: '',
    status: 'Active',
    vendor: '',
    publisher: '',
    validityType: ''
  };

  editForm: Partial<CreateAsset> & {
    region?: string;
    state?: string;
    plazaName?: string;
    osType?: string;
    osVersion?: string;
    dbType?: string;
    dbVersion?: string;
    ipAddress?: string;
    assignedUserText?: string;
    userRole?: string;
    patchStatus?: string;
    usbBlockingStatus?: string;
    procuredBy?: string;
    remarks?: string;
  } = {};

  // Constants
  usageCategories = [
    { value: 'TMS', label: 'TMS' },
    { value: 'ITNonTMS', label: 'IT (Non-TMS)' }
  ];
  
  statuses = [
    { value: 'In Use', label: 'In Use' },
    { value: 'Spare', label: 'Spare' },
    { value: 'Repair', label: 'Repair' },
    { value: 'Decommissioned', label: 'Decommissioned' }
  ];
  
  softwareStatuses = [
    { value: 'Active', label: 'Active' },
    { value: 'Expired', label: 'Expired' },
    { value: 'Available', label: 'Available' }
  ];

  licenseTypes = [
    { value: 'Subscription', label: 'Subscription' },
    { value: 'Open Source', label: 'Open Source' },
    { value: 'Per User', label: 'Per User' },
    { value: 'Per Core', label: 'Per Core' },
    { value: 'Per Device', label: 'Per Device' }
  ];

  validityTypes = [
    { value: 'Renewable', label: 'Renewable' },
    { value: 'Perennial', label: 'Perennial' }
  ];
  
  placingOptions = ['Lane Area', 'Booth Area', 'Plaza Area', 'Server Room', 'Control Room', 'Admin Building'];
  assetTypes = ['Hardware', 'Software'];

  // Validation
  validationErrors: { [key: string]: string } = {};

  constructor(private api: Api, private http: HttpClient) {}

  ngOnInit() {
    this.loadAssets();
    this.loadLocations();
  }

  loadAssets() {
    this.loading = true;
    // Load only hardware assets for the main list
    this.api.getAssets().subscribe({
      next: (assets) => {
        this.assets = assets;
        this.filteredAssets = assets;
        this.applyFilters();
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
    this.softwareTab = 1;
    this.softwareMaxTab = 1;
    this.clearMessages();
  }

  openEditModal(asset: Asset) {
    this.selectedAsset = asset;
    this.editForm = {
      assetTag: asset.assetTag,
      locationId: asset.locationId,
      classification: asset.classification,
      region: asset.region,
      state: asset.state,
      plazaName: asset.plazaName,
      assetType: asset.assetType,
      subType: asset.subType,
      make: asset.make,
      model: asset.model,
      serialNumber: asset.serialNumber,
      osType: asset.osType,
      osVersion: asset.osVersion,
      dbType: asset.dbType,
      dbVersion: asset.dbVersion,
      ipAddress: asset.ipAddress,
      assignedUserText: asset.assignedUserText,
      userRole: asset.userRole,
      procurementDate: asset.procurementDate,
      procurementCost: asset.procurementCost,
      vendor: asset.vendor,
      warrantyStartDate: asset.warrantyStartDate,
      warrantyEndDate: asset.warrantyEndDate,
      commissioningDate: asset.commissioningDate,
      status: asset.status,
      placing: asset.placing,
      patchStatus: asset.patchStatus,
      usbBlockingStatus: asset.usbBlockingStatus,
      procuredBy: asset.procuredBy,
      remarks: asset.remarks,
      assignedUserId: asset.assignedUserId,
      assignedUserRole: asset.assignedUserRole
    };
    
    // Load users from the asset's project
    this.loadProjectUsers(asset.projectId);
    
    this.showEditModal = true;
    this.editTab = 1; // Reset to first tab
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
    this.viewTab = 1; // Reset to first tab
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
      const file = input.files[0];
      
      // Validate file extension
      if (!file.name.toLowerCase().endsWith('.xlsx')) {
        this.error = 'Invalid file format. Please upload an Excel file (.xlsx)';
        this.selectedFile = null;
        return;
      }
      
      // Validate file size (50MB)
      const maxSize = 50 * 1024 * 1024; // 50MB in bytes
      if (file.size > maxSize) {
        this.error = 'File size exceeds maximum limit of 50MB';
        this.selectedFile = null;
        return;
      }
      
      this.selectedFile = file;
      this.uploadResult = null;
      this.clearMessages();
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
      
      // Validate file extension
      if (!file.name.toLowerCase().endsWith('.xlsx')) {
        this.error = 'Invalid file format. Please upload an Excel file (.xlsx)';
        return;
      }
      
      // Validate file size (50MB)
      const maxSize = 50 * 1024 * 1024; // 50MB in bytes
      if (file.size > maxSize) {
        this.error = 'File size exceeds maximum limit of 50MB';
        return;
      }
      
      this.selectedFile = file;
      this.uploadResult = null;
      this.clearMessages();
    }
  }

  clearFile() {
    this.selectedFile = null;
    this.uploadResult = null;
  }

  async uploadFile() {
    if (!this.selectedFile) {
      this.error = 'Please select a file first';
      return;
    }

    this.uploading = true;
    this.uploadResult = null;
    this.clearMessages();

    try {
      const formData = new FormData();
      formData.append('file', this.selectedFile);

      const token = localStorage.getItem('auth_token');
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
        
        // Check if there's an error message from backend (like missing columns)
        if (result.message && result.totalRows === 0 && result.successCount === 0) {
          this.error = result.message;
          return;
        }
        
        if (result.successCount > 0) {
          this.success = `Successfully uploaded ${result.successCount} assets!`;
          this.loadAssets();
        }
        
        if (result.failedCount > 0) {
          this.error = `${result.failedCount} rows failed. Please check the error details below.`;
        }
      }
    } catch (error: any) {
      console.error('Upload error:', error);
      
      // Handle different error types
      if (error.status === 400) {
        this.error = error.error?.message || 'Invalid file or data format';
      } else if (error.status === 413) {
        this.error = 'File size exceeds maximum limit of 50MB';
      } else if (error.status === 415) {
        this.error = 'Invalid file format. Please upload an Excel file (.xlsx)';
      } else {
        this.error = error.error?.message || 'An error occurred during upload. Please try again.';
      }
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

    if (!this.createForm.assetTag) {
      this.validationErrors['assetTag'] = 'Asset tag is required';
      isValid = false;
    }

    if (!this.createForm.placing) {
      this.validationErrors['placing'] = 'Placing is required';
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

  validateSoftwareForm(): boolean {
    let isValid = true;
    this.clearValidationErrors();

    if (!this.softwareForm.softwareName) {
      this.validationErrors['softwareName'] = 'Software name is required';
      isValid = false;
    }

    if (!this.softwareForm.version) {
      this.validationErrors['version'] = 'Version is required';
      isValid = false;
    }

    if (!this.softwareForm.licenseKey) {
      this.validationErrors['licenseKey'] = 'License key is required';
      isValid = false;
    }

    if (!this.softwareForm.licenseType) {
      this.validationErrors['licenseType'] = 'License type is required';
      isValid = false;
    }

    if (!this.softwareForm.numberOfLicenses || this.softwareForm.numberOfLicenses <= 0) {
      this.validationErrors['numberOfLicenses'] = 'Number of licenses must be greater than 0';
      isValid = false;
    }

    if (!this.softwareForm.purchaseDate) {
      this.validationErrors['purchaseDate'] = 'Purchase date is required';
      isValid = false;
    }

    if (!this.softwareForm.validityStartDate) {
      this.validationErrors['validityStartDate'] = 'Validity start date is required';
      isValid = false;
    }

    if (!this.softwareForm.validityEndDate) {
      this.validationErrors['validityEndDate'] = 'Validity end date is required';
      isValid = false;
    }

    if (this.softwareForm.validityStartDate && this.softwareForm.validityEndDate) {
      const startDate = new Date(this.softwareForm.validityStartDate);
      const endDate = new Date(this.softwareForm.validityEndDate);
      if (endDate <= startDate) {
        this.validationErrors['validityEndDate'] = 'Validity end date must be greater than start date';
        isValid = false;
      }
    }

    if (!this.softwareForm.assetTag) {
      this.validationErrors['assetTag'] = 'Asset tag is required';
      isValid = false;
    }

    if (!this.softwareForm.status) {
      this.validationErrors['status'] = 'Status is required';
      isValid = false;
    }

    if (!this.softwareForm.vendor) {
      this.validationErrors['vendor'] = 'Vendor is required';
      isValid = false;
    }

    if (!this.softwareForm.publisher) {
      this.validationErrors['publisher'] = 'Publisher is required';
      isValid = false;
    }

    if (!this.softwareForm.validityType) {
      this.validationErrors['validityType'] = 'Validity type is required';
      isValid = false;
    }

    return isValid;
  }

  // CRUD operations
  createAsset() {
    if (this.selectedAssetType === 'Hardware') {
      if (!this.validateTab1() || !this.validateTab2()) {
        return;
      }
      this.createHardwareAsset();
    } else {
      if (!this.validateSoftwareTab1() || !this.validateSoftwareTab2() || !this.validateSoftwareForm()) {
        return;
      }
      this.createSoftwareAsset();
    }
  }

  createHardwareAsset() {
    this.loading = true;
    this.api.createAsset(this.createForm).subscribe({
      next: (asset) => {
        this.assets.push(asset);
        this.success = 'Hardware asset created successfully';
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

  createSoftwareAsset() {
    this.loading = true;
    console.log('Sending software asset data:', this.softwareForm);
    this.api.createSoftwareAsset(this.softwareForm).subscribe({
      next: (asset) => {
        this.success = 'Software asset created successfully';
        this.loading = false;
        this.closeModals();
        // Reload assets after a short delay to ensure database is updated
        setTimeout(() => {
          this.loadAssets();
        }, 500);
      },
      error: (error) => {
        console.error('Full error response:', error);
        console.error('Error details:', error.error);
        const errorMessage = error.error?.message || error.message || 'Failed to create software asset';
        this.error = errorMessage;
        this.loading = false;
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
    this.selectedAssetType = 'Hardware';
    this.createForm = {
      assetTag: '',
      projectId: 0,
      locationId: 0,
      usageCategory: 'TMS',
      classification: '',
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
      status: 'In Use',
      placing: '',
      assignedUserId: undefined,
      assignedUserRole: ''
    };
    this.softwareForm = {
      softwareName: '',
      version: '',
      licenseKey: '',
      licenseType: '',
      numberOfLicenses: 1,
      purchaseDate: undefined,
      validityStartDate: undefined,
      validityEndDate: undefined,
      assetTag: '',
      status: 'Active',
      vendor: '',
      publisher: '',
      validityType: ''
    };
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }

  // Search and Filter Methods
  applyFilters() {
    this.filteredAssets = this.assets.filter(asset => {
      // Search filter
      const matchesSearch = !this.searchTerm || 
        asset.assetId.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        asset.assetTag.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        (asset.assetType && asset.assetType.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.make && asset.make.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.model && asset.model.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.serialNumber && asset.serialNumber.toLowerCase().includes(this.searchTerm.toLowerCase()));

      // Status filter
      const matchesStatus = this.filterStatus === 'all' || asset.status === this.filterStatus;

      // Type filter
      const matchesType = this.filterType === 'all' || asset.assetType === this.filterType;

      return matchesSearch && matchesStatus && matchesType;
    });
  }

  onSearchChange() {
    this.applyFilters();
  }

  onStatusFilterChange() {
    this.applyFilters();
  }

  onTypeFilterChange() {
    this.applyFilters();
  }

  clearFilters() {
    this.searchTerm = '';
    this.filterStatus = 'all';
    this.filterType = 'all';
    this.applyFilters();
  }

  // View/Edit modal tab navigation
  setViewTab(tab: number) {
    this.viewTab = tab;
  }

  setEditTab(tab: number) {
    this.editTab = tab;
  }

  // Software asset tab navigation
  goToSoftwareTab(tabNumber: number) {
    if (tabNumber <= this.softwareMaxTab) {
      this.softwareTab = tabNumber;
    }
  }

  nextSoftwareTab() {
    if (this.validateSoftwareTab(this.softwareTab)) {
      this.softwareTab++;
      this.softwareMaxTab = Math.max(this.softwareMaxTab, this.softwareTab);
    }
  }

  previousSoftwareTab() {
    if (this.softwareTab > 1) {
      this.softwareTab--;
    }
  }

  validateSoftwareTab(tabNumber: number): boolean {
    this.clearValidationErrors();
    
    if (tabNumber === 1) {
      return this.validateSoftwareTab1();
    } else if (tabNumber === 2) {
      return this.validateSoftwareTab2();
    }
    
    return true;
  }

  validateSoftwareTab1(): boolean {
    let isValid = true;

    if (!this.softwareForm.softwareName) {
      this.validationErrors['softwareName'] = 'Software name is required';
      isValid = false;
    }

    if (!this.softwareForm.version) {
      this.validationErrors['version'] = 'Version is required';
      isValid = false;
    }

    if (!this.softwareForm.licenseKey) {
      this.validationErrors['licenseKey'] = 'License key is required';
      isValid = false;
    }

    if (!this.softwareForm.licenseType) {
      this.validationErrors['licenseType'] = 'License type is required';
      isValid = false;
    }

    return isValid;
  }

  validateSoftwareTab2(): boolean {
    let isValid = true;

    if (!this.softwareForm.numberOfLicenses || this.softwareForm.numberOfLicenses <= 0) {
      this.validationErrors['numberOfLicenses'] = 'Number of licenses must be greater than 0';
      isValid = false;
    }

    if (!this.softwareForm.purchaseDate) {
      this.validationErrors['purchaseDate'] = 'Purchase date is required';
      isValid = false;
    }

    if (!this.softwareForm.validityStartDate) {
      this.validationErrors['validityStartDate'] = 'Validity start date is required';
      isValid = false;
    }

    if (!this.softwareForm.validityEndDate) {
      this.validationErrors['validityEndDate'] = 'Validity end date is required';
      isValid = false;
    }

    if (this.softwareForm.validityStartDate && this.softwareForm.validityEndDate) {
      const startDate = new Date(this.softwareForm.validityStartDate);
      const endDate = new Date(this.softwareForm.validityEndDate);
      if (endDate <= startDate) {
        this.validationErrors['validityEndDate'] = 'Validity end date must be greater than start date';
        isValid = false;
      }
    }

    return isValid;
  }

  isSoftwareTabValid(tabNumber: number): boolean {
    if (tabNumber === 1) {
      return !!(this.softwareForm.softwareName && this.softwareForm.version && this.softwareForm.licenseKey && this.softwareForm.licenseType);
    } else if (tabNumber === 2) {
      return !!(this.softwareForm.numberOfLicenses && this.softwareForm.purchaseDate && this.softwareForm.validityStartDate && this.softwareForm.validityEndDate);
    } else if (tabNumber === 3) {
      return !!(this.softwareForm.assetTag && this.softwareForm.status && this.softwareForm.vendor && this.softwareForm.publisher && this.softwareForm.validityType);
    }
    return true;
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
      return !!(this.createForm.locationId && this.createForm.assetTag && this.createForm.placing);
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
