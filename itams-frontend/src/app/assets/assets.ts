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
  licensingAssets: any[] = [];
  filteredlicensingAssets: any[] = [];
  projects: Project[] = [];
  locations: Location[] = [];
  projectUsers: any[] = [];
  loading = false;
  error = '';
  success = '';

  // Navigation state
  currentView: 'location-select' | 'category-dashboard' | 'asset-list' = 'location-select';
  selectedLocationType: 'office' | 'site' | null = null;
  selectedCategory: 'hardware' | 'licensing' | 'services' | null = null;

  // Search and filter
  searchTerm = '';
  filterStatus = 'all';
  filterType = 'all';

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  showViewModal = false;
  showBulkUploadModal = false;
  selectedAsset: Asset | any = null;
  currentTab = 1;
  maxTab = 1;

  // Licensing asset tab
  licensingTab = 1;
  licensingMaxTab = 1;

  // View/Edit modal tabs
  viewTab = 1;
  editTab = 1;

  // Bulk upload
  selectedFile: File | null = null;
  uploading = false;
  uploadResult: BulkUploadResult | null = null;
  dragOver = false;
  private baseUrl = '/api';

  // Form data
  selectedAssetType: 'Hardware' | 'Licensing' | 'Services' = 'Hardware';

  createForm: CreateAsset = {
    assetTag: '', projectId: 0, locationId: 0, usageCategory: 'TMS',
    classification: '', assetType: '', subType: '', make: '', model: '',
    serialNumber: '', procurementDate: undefined, procurementCost: undefined,
    vendor: '', warrantyStartDate: undefined, warrantyEndDate: undefined,
    commissioningDate: undefined, status: 'In Use', placing: '',
    assignedUserId: undefined, assignedUserRole: ''
  };

  licensingForm: any = {
    licenseName: '', version: '', licenseKey: '', licenseType: '',
    numberOfLicenses: 1, purchaseDate: undefined, validityStartDate: undefined,
    validityEndDate: undefined, assetTag: '', status: 'Active',
    vendor: '', publisher: '', validityType: '', usageCategory: 'TMS'
  };

  editForm: Partial<CreateAsset> & {
    assetId?: string; licenseName?: string; version?: string;
    licenseKey?: string; licenseType?: string; numberOfLicenses?: number;
    purchaseDate?: Date; validityStartDate?: Date; validityEndDate?: Date;
    validityType?: string; vendor?: string; publisher?: string;
    region?: string; state?: string; plazaName?: string;
    osType?: string; osVersion?: string; dbType?: string; dbVersion?: string;
    ipAddress?: string; assignedUserText?: string; userRole?: string;
    patchStatus?: string; usbBlockingStatus?: string; procuredBy?: string; remarks?: string;
  } = {};

  // Category definitions
  assetCategories = [
    { value: 'hardware', label: 'Hardware', icon: 'fas fa-server', color: '#0d6efd', description: 'Physical devices, computers, networking equipment' },
    { value: 'licensing', label: 'Software & Licensing', icon: 'fas fa-file-code', color: '#198754', description: 'Software installations, license keys, subscriptions' },
    { value: 'services', label: 'Services', icon: 'fas fa-concierge-bell', color: '#fd7e14', description: 'AMC, support contracts, SLAs' }
  ];

  hardwareSubTypes = ['Desktop', 'Laptop', 'Server', 'Switch', 'Router', 'UPS', 'Printer', 'Camera', 'Other Hardware'];
  softwareSubTypes = ['Operating System', 'Application Software', 'Antivirus', 'License Key', 'Subscription', 'Other Software'];
  serviceSubTypes = ['AMC', 'Support Contract', 'SLA Agreement', 'Internet Service', 'Cloud Service', 'Other Service'];

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

  licensingStatuses = [
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

  // Services
  serviceAssets: any[] = [];
  filteredServiceAssets: any[] = [];
  serviceTypes: any[] = [];
  showServiceViewModal = false;
  showServiceEditModal = false;
  showRenewModal = false;
  selectedServiceAsset: any = null;
  serviceViewTab = 1;
  serviceEditTab = 1;
  serviceTab = 1;

  serviceForm: any = {
    serviceName: '', serviceTypeId: 0, vendorName: '', contractNumber: '',
    contractStartDate: undefined, contractEndDate: undefined,
    renewalCycleMonths: 12, renewalReminderDays: 30,
    contractCost: undefined, billingCycle: '', currency: 'INR',
    slaType: '', responseTime: '', coverageDetails: '',
    contactPerson: '', supportContactNumber: '',
    description: '', remarks: '', usageCategory: 'TMS', autoRenewEnabled: false
  };

  renewForm: any = {
    newEndDate: undefined, renewalCost: undefined, remarks: ''
  };

  serviceStatuses = [
    { value: 'Active', label: 'Active' },
    { value: 'Expiring', label: 'Expiring' },
    { value: 'Expired', label: 'Expired' },
    { value: 'Cancelled', label: 'Cancelled' }
  ];

  billingCycles = ['Monthly', 'Quarterly', 'Annually'];
  currencies = ['INR', 'USD', 'EUR'];

  validationErrors: { [key: string]: string } = {};

  constructor(private api: Api, private http: HttpClient) {}

  ngOnInit() {
    this.loadAssets();
    this.loadLocations();
    this.loadServiceTypes();
  }

  selectLocationType(type: 'office' | 'site') {
    this.selectedLocationType = type;
    this.currentView = 'category-dashboard';
  }

  selectCategory(category: 'hardware' | 'licensing' | 'services') {
    this.selectedCategory = category;
    this.currentView = 'asset-list';
    this.applyFilters();
  }

  goBack(level: 'location-select' | 'category-dashboard') {
    if (level === 'location-select') {
      this.selectedLocationType = null;
      this.selectedCategory = null;
      this.currentView = 'location-select';
    } else {
      this.selectedCategory = null;
      this.currentView = 'category-dashboard';
    }
  }

  get locationTypeLabel(): string {
    return this.selectedLocationType === 'office' ? 'Head Office' : 'Site';
  }

  get categoryLabel(): string {
    return this.assetCategories.find(c => c.value === this.selectedCategory)?.label ?? '';
  }

  // Count helpers for dashboard cards
  getHardwareCount(): number {
    return this.assets.filter(a => this.matchesLocationType(a, this.selectedLocationType!)).length;
  }

  getLicensingCount(): number {
    return this.licensingAssets.length;
  }

  getServicesCount(): number {
    return this.serviceAssets.length;
  }

  getCategoryCount(category: string): number {
    if (category === 'hardware') return this.getHardwareCount();
    if (category === 'licensing') return this.getLicensingCount();
    return this.getServicesCount();
  }

  // ── Filtering ────────────────────────────────────────────────────────────────

  matchesLocationType(asset: Asset, locationType: 'office' | 'site'): boolean {
    const loc = this.locations.find(l => l.id === asset.locationId);
    if (loc) {
      if (locationType === 'office') return loc.type === 'office';
      return loc.type === 'plaza' || loc.type !== 'office';
    }
    // Fallback: use placing field
    const officePlacings = ['Server Room', 'Control Room', 'Admin Building'];
    const sitePlacings = ['Lane Area', 'Booth Area', 'Plaza Area'];
    if (locationType === 'office') return officePlacings.includes(asset.placing);
    return sitePlacings.includes(asset.placing);
  }

  applyFilters() {
    this.filteredAssets = this.assets.filter(asset => {
      const matchesSearch = !this.searchTerm ||
        asset.assetId.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        asset.assetTag.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        (asset.assetType && asset.assetType.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.make && asset.make.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.model && asset.model.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.serialNumber && asset.serialNumber.toLowerCase().includes(this.searchTerm.toLowerCase()));

      const matchesStatus = this.filterStatus === 'all' || asset.status === this.filterStatus;
      const matchesType = this.filterType === 'all' || asset.assetType === this.filterType;

      const matchesLocation = !this.selectedLocationType || this.matchesLocationType(asset, this.selectedLocationType);

      return matchesSearch && matchesStatus && matchesType && matchesLocation;
    });

    if (this.selectedCategory === 'licensing') {
      this.filteredlicensingAssets = this.licensingAssets.filter(asset =>
        !this.searchTerm ||
        (asset.licenseName && asset.licenseName.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.version && asset.version.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.vendor && asset.vendor.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (asset.assetId && asset.assetId.toLowerCase().includes(this.searchTerm.toLowerCase()))
      );
    }

    if (this.selectedCategory === 'services') {
      this.filteredServiceAssets = this.serviceAssets.filter(a =>
        !this.searchTerm ||
        (a.serviceName && a.serviceName.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (a.vendorName && a.vendorName.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (a.assetId && a.assetId.toLowerCase().includes(this.searchTerm.toLowerCase())) ||
        (a.serviceTypeName && a.serviceTypeName.toLowerCase().includes(this.searchTerm.toLowerCase()))
      );
    }
  }

  onSearchChange() { this.applyFilters(); }
  onStatusFilterChange() { this.applyFilters(); }
  onTypeFilterChange() { this.applyFilters(); }

  clearFilters() {
    this.searchTerm = '';
    this.filterStatus = 'all';
    this.filterType = 'all';
    this.applyFilters();
  }

  // ── Data Loading ─────────────────────────────────────────────────────────────

  loadAssets() {
    this.loading = true;
    this.api.getAssets().subscribe({
      next: (assets) => {
        this.assets = assets;
        this.filteredAssets = assets;
        this.applyFilters();
        this.loading = false;
      },
      error: () => { this.error = 'Failed to load assets'; this.loading = false; }
    });

    this.api.getLicensingAssets().subscribe({
      next: (assets) => {
        this.licensingAssets = assets;
        this.filteredlicensingAssets = assets;
      },
      error: (err) => console.error('Error loading licensing assets:', err)
    });

    this.api.getServiceAssets().subscribe({
      next: (assets) => {
        this.serviceAssets = assets;
        this.filteredServiceAssets = assets;
      },
      error: (err) => console.error('Error loading service assets:', err)
    });
  }

  loadLocations() {
    this.api.getLocations().subscribe({
      next: (locations) => { this.locations = locations; },
      error: (err) => console.error('Error loading locations:', err)
    });
  }

  loadServiceTypes() {
    this.api.getServiceTypes().subscribe({
      next: (types) => { this.serviceTypes = types; },
      error: (err) => console.error('Error loading service types:', err)
    });
  }

  // ── Modals ───────────────────────────────────────────────────────────────────

  openCreateModal() {
    this.resetCreateForm();
    if (this.selectedCategory === 'licensing') this.selectedAssetType = 'Licensing';
    else if (this.selectedCategory === 'services') this.selectedAssetType = 'Services';
    else this.selectedAssetType = 'Hardware';
    this.showCreateModal = true;
    this.currentTab = 1;
    this.maxTab = 1;
    this.licensingTab = 1;
    this.licensingMaxTab = 1;
    this.serviceTab = 1;
    this.clearMessages();
  }

  openLicensingAssetViewModal(asset: any) {
    this.selectedAsset = asset;
    this.showViewModal = true;
    this.viewTab = 1;
    this.clearMessages();
  }

  openLicensingAssetEditModal(asset: any) {
    this.selectedAsset = asset;
    this.editForm = { ...asset };
    this.showEditModal = true;
    this.editTab = 1;
    this.clearMessages();
  }

  openEditModal(asset: Asset) {
    this.selectedAsset = asset;
    this.editForm = {
      assetTag: asset.assetTag, locationId: asset.locationId,
      classification: asset.classification, region: asset.region,
      state: asset.state, plazaName: asset.plazaName,
      assetType: asset.assetType, subType: asset.subType,
      make: asset.make, model: asset.model, serialNumber: asset.serialNumber,
      osType: asset.osType, osVersion: asset.osVersion,
      dbType: asset.dbType, dbVersion: asset.dbVersion, ipAddress: asset.ipAddress,
      assignedUserText: asset.assignedUserText, userRole: asset.userRole,
      procurementDate: asset.procurementDate, procurementCost: asset.procurementCost,
      vendor: asset.vendor, warrantyStartDate: asset.warrantyStartDate,
      warrantyEndDate: asset.warrantyEndDate, commissioningDate: asset.commissioningDate,
      status: asset.status, placing: asset.placing, patchStatus: asset.patchStatus,
      usbBlockingStatus: asset.usbBlockingStatus, procuredBy: asset.procuredBy,
      remarks: asset.remarks, assignedUserId: asset.assignedUserId,
      assignedUserRole: asset.assignedUserRole
    };
    this.loadProjectUsers(asset.projectId);
    this.showEditModal = true;
    this.editTab = 1;
    this.clearMessages();
  }

  loadProjectUsers(projectId: number) {
    this.api.getUsers().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.projectUsers = response.data.filter((user: any) => user.projectId === projectId);
        }
      },
      error: () => { this.projectUsers = []; }
    });
  }

  openViewModal(asset: Asset) {
    this.selectedAsset = asset;
    this.showViewModal = true;
    this.viewTab = 1;
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

  // ── Bulk Upload ───────────────────────────────────────────────────────────────

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
      if (!file.name.toLowerCase().endsWith('.xlsx')) {
        this.error = 'Invalid file format. Please upload an Excel file (.xlsx)';
        this.selectedFile = null;
        return;
      }
      if (file.size > 50 * 1024 * 1024) {
        this.error = 'File size exceeds maximum limit of 50MB';
        this.selectedFile = null;
        return;
      }
      this.selectedFile = file;
      this.uploadResult = null;
      this.clearMessages();
    }
  }

  onDragOver(event: DragEvent) { event.preventDefault(); event.stopPropagation(); this.dragOver = true; }
  onDragLeave(event: DragEvent) { event.preventDefault(); event.stopPropagation(); this.dragOver = false; }

  onDrop(event: DragEvent) {
    event.preventDefault(); event.stopPropagation(); this.dragOver = false;
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      if (!file.name.toLowerCase().endsWith('.xlsx')) { this.error = 'Invalid file format. Please upload an Excel file (.xlsx)'; return; }
      if (file.size > 50 * 1024 * 1024) { this.error = 'File size exceeds maximum limit of 50MB'; return; }
      this.selectedFile = file;
      this.uploadResult = null;
      this.clearMessages();
    }
  }

  clearFile() { this.selectedFile = null; this.uploadResult = null; }

  async uploadFile() {
    if (!this.selectedFile) { this.error = 'Please select a file first'; return; }
    this.uploading = true;
    this.uploadResult = null;
    this.clearMessages();
    try {
      const formData = new FormData();
      formData.append('file', this.selectedFile);
      const token = localStorage.getItem('auth_token');
      const headers = new HttpHeaders({ 'Authorization': `Bearer ${token}` });

      let endpoint = `${this.baseUrl}/assets/bulk-upload`;
      if (this.selectedCategory === 'licensing') endpoint = `${this.baseUrl}/licensingassets/bulk-upload`;
      else if (this.selectedCategory === 'services') endpoint = `${this.baseUrl}/serviceassets/bulk-upload`;

      const result = await this.http.post<BulkUploadResult>(endpoint, formData, { headers }).toPromise();
      if (result) {
        this.uploadResult = result;
        if (result.message && result.totalRows === 0 && result.successCount === 0) { this.error = result.message; return; }
        if (result.successCount > 0) { this.success = `Successfully uploaded ${result.successCount} records!`; this.loadAssets(); }
        if (result.failedCount > 0) { this.error = `${result.failedCount} rows failed. Check the error details below.`; }
      }
    } catch (error: any) {
      if (error.status === 400) this.error = error.error?.message || 'Invalid file or data format';
      else if (error.status === 413) this.error = 'File size exceeds maximum limit of 50MB';
      else this.error = error.error?.message || 'An error occurred during upload. Please try again.';
    } finally { this.uploading = false; }
  }

  downloadTemplate() {
    if (this.selectedCategory === 'licensing') window.open(`${this.baseUrl}/licensingassets/download-template`, '_blank');
    else if (this.selectedCategory === 'services') window.open(`${this.baseUrl}/serviceassets/download-template`, '_blank');
    else window.open(`${this.baseUrl}/assets/download-template`, '_blank');
  }
  getFileName(): string { return this.selectedFile?.name || ''; }
  getFileSize(): string {
    if (!this.selectedFile) return '';
    return `${(this.selectedFile.size / (1024 * 1024)).toFixed(2)} MB`;
  }

  // ── Tab Navigation ────────────────────────────────────────────────────────────

  goToTab(tabNumber: number) { if (tabNumber <= this.maxTab) this.currentTab = tabNumber; }

  nextTab() {
    if (this.validateCurrentTab()) {
      this.currentTab++;
      this.maxTab = Math.max(this.maxTab, this.currentTab);
    }
  }

  previousTab() { if (this.currentTab > 1) this.currentTab--; }

  validateCurrentTab(): boolean {
    this.clearValidationErrors();
    if (this.currentTab === 1) return this.validateTab1();
    if (this.currentTab === 2) return this.validateTab2();
    return true;
  }

  validateTab1(): boolean {
    let isValid = true;
    if (!this.createForm.assetTag) { this.validationErrors['assetTag'] = 'Asset tag is required'; isValid = false; }
    if (this.selectedLocationType === 'office') {
      if (!this.createForm.locationId) { this.validationErrors['placing'] = 'Location is required'; isValid = false; }
    } else {
      if (!this.createForm.placing) { this.validationErrors['placing'] = 'Placing is required'; isValid = false; }
    }
    return isValid;
  }

  validateTab2(): boolean {
    let isValid = true;
    if (!this.createForm.assetType) { this.validationErrors['assetType'] = 'Asset type is required'; isValid = false; }
    if (!this.createForm.make) { this.validationErrors['make'] = 'Make is required'; isValid = false; }
    if (!this.createForm.model) { this.validationErrors['model'] = 'Model is required'; isValid = false; }
    return isValid;
  }

  goTolicensingTab(tabNumber: number) { if (tabNumber <= this.licensingMaxTab) this.licensingTab = tabNumber; }

  nextlicensingTab() {
    if (this.validatelicensingTab(this.licensingTab)) {
      this.licensingTab++;
      this.licensingMaxTab = Math.max(this.licensingMaxTab, this.licensingTab);
    }
  }

  previouslicensingTab() { if (this.licensingTab > 1) this.licensingTab--; }

  validatelicensingTab(tabNumber: number): boolean {
    this.clearValidationErrors();
    if (tabNumber === 1) return this.validatelicensingTab1();
    if (tabNumber === 2) return this.validatelicensingTab2();
    return true;
  }

  validatelicensingTab1(): boolean {
    let isValid = true;
    if (!this.licensingForm.licenseName) { this.validationErrors['licenseName'] = 'License name is required'; isValid = false; }
    if (!this.licensingForm.version) { this.validationErrors['version'] = 'Version is required'; isValid = false; }
    if (!this.licensingForm.licenseKey) { this.validationErrors['licenseKey'] = 'License key is required'; isValid = false; }
    if (!this.licensingForm.licenseType) { this.validationErrors['licenseType'] = 'License type is required'; isValid = false; }
    return isValid;
  }

  validatelicensingTab2(): boolean {
    let isValid = true;
    if (!this.licensingForm.numberOfLicenses || this.licensingForm.numberOfLicenses <= 0) { this.validationErrors['numberOfLicenses'] = 'Number of licenses must be greater than 0'; isValid = false; }
    if (!this.licensingForm.purchaseDate) { this.validationErrors['purchaseDate'] = 'Purchase date is required'; isValid = false; }
    if (!this.licensingForm.validityStartDate) { this.validationErrors['validityStartDate'] = 'Validity start date is required'; isValid = false; }
    if (!this.licensingForm.validityEndDate) { this.validationErrors['validityEndDate'] = 'Validity end date is required'; isValid = false; }
    if (this.licensingForm.validityStartDate && this.licensingForm.validityEndDate) {
      if (new Date(this.licensingForm.validityEndDate) <= new Date(this.licensingForm.validityStartDate)) {
        this.validationErrors['validityEndDate'] = 'Validity end date must be after start date'; isValid = false;
      }
    }
    return isValid;
  }

  validatelicensingForm(): boolean {
    return this.validatelicensingTab1() && this.validatelicensingTab2() &&
      !!this.licensingForm.status && !!this.licensingForm.vendor &&
      !!this.licensingForm.publisher && !!this.licensingForm.validityType;
  }

  islicensingTabValid(tabNumber: number): boolean {
    if (tabNumber === 1) return !!(this.licensingForm.licenseName && this.licensingForm.version && this.licensingForm.licenseKey && this.licensingForm.licenseType);
    if (tabNumber === 2) return !!(this.licensingForm.numberOfLicenses && this.licensingForm.purchaseDate && this.licensingForm.validityStartDate && this.licensingForm.validityEndDate);
    if (tabNumber === 3) return !!(this.licensingForm.status && this.licensingForm.vendor && this.licensingForm.publisher && this.licensingForm.validityType);
    return true;
  }

  setViewTab(tab: number) { this.viewTab = tab; }
  setEditTab(tab: number) { this.editTab = tab; }

  isTabValid(tabNumber: number): boolean {
    if (tabNumber === 1) {
      const locationOk = this.selectedLocationType === 'office' ? !!this.createForm.locationId : !!this.createForm.placing;
      return !!(this.createForm.assetTag && locationOk);
    }
    if (tabNumber === 2) return !!(this.createForm.assetType && this.createForm.make && this.createForm.model);
    return true;
  }

  // ── CRUD ──────────────────────────────────────────────────────────────────────

  createAsset() {
    if (this.selectedAssetType === 'Hardware') {
      if (!this.validateTab1() || !this.validateTab2()) return;
      this.createHardwareAsset();
    } else {
      if (!this.validatelicensingTab1() || !this.validatelicensingTab2() || !this.validatelicensingForm()) return;
      this.createLicensingAsset();
    }
  }

  createHardwareAsset() {
    this.loading = true;
    this.api.createAsset(this.createForm).subscribe({
      next: (asset) => { this.assets.push(asset); this.success = 'Hardware asset created successfully'; this.loading = false; this.closeModals(); this.applyFilters(); },
      error: (error) => { this.error = error.error?.message || 'Failed to create asset'; this.loading = false; }
    });
  }

  createLicensingAsset() {
    this.loading = true;
    this.api.createLicensingAsset(this.licensingForm).subscribe({
      next: () => {
        this.success = 'Licensing asset created successfully';
        this.loading = false;
        this.closeModals();
        setTimeout(() => this.loadAssets(), 500);
      },
      error: (error) => { this.error = error.error?.message || error.message || 'Failed to create licensing asset'; this.loading = false; }
    });
  }

  updateAsset() {
    if (!this.selectedAsset) return;
    this.loading = true;
    this.api.updateAsset(this.selectedAsset.id, this.editForm).subscribe({
      next: () => { this.success = 'Asset updated successfully'; this.loading = false; this.closeModals(); this.loadAssets(); },
      error: (error) => { this.error = error.error?.message || 'Failed to update asset'; this.loading = false; }
    });
  }

  updateLicensingAsset() {
    if (!this.selectedAsset) return;
    this.loading = true;
    this.api.updateLicensingAsset(this.selectedAsset.id, { status: this.editForm.status }).subscribe({
      next: () => { this.success = 'Licensing asset updated successfully'; this.loading = false; this.closeModals(); this.loadAssets(); },
      error: (error) => { this.error = error.error?.message || 'Failed to update licensing asset'; this.loading = false; }
    });
  }

  deleteAsset(asset: Asset) {
    if (confirm(`Are you sure you want to delete asset ${asset.assetId}?`)) {
      this.loading = true;
      this.api.deleteAsset(asset.id).subscribe({
        next: () => { this.success = 'Asset deleted successfully'; this.loading = false; this.loadAssets(); },
        error: (error) => { this.error = error.error?.message || 'Failed to delete asset'; this.loading = false; }
      });
    }
  }

  // ── Utilities ─────────────────────────────────────────────────────────────────

  resetCreateForm() {
    this.selectedAssetType = 'Hardware';
    this.createForm = {
      assetTag: '', projectId: 0, locationId: 0, usageCategory: 'TMS',
      classification: '', assetType: '', subType: '', make: '', model: '',
      serialNumber: '', procurementDate: undefined, procurementCost: undefined,
      vendor: '', warrantyStartDate: undefined, warrantyEndDate: undefined,
      commissioningDate: undefined, status: 'In Use', placing: '',
      assignedUserId: undefined, assignedUserRole: ''
    };
    this.licensingForm = {
      licenseName: '', version: '', licenseKey: '', licenseType: '',
      numberOfLicenses: 1, purchaseDate: undefined, validityStartDate: undefined,
      validityEndDate: undefined, assetTag: '', status: 'Active',
      vendor: '', publisher: '', validityType: '', usageCategory: 'TMS'
    };
  }

  searchlicensingAssets(term: string) {
    this.filteredlicensingAssets = this.licensingAssets.filter(asset =>
      !term ||
      (asset.licenseName && asset.licenseName.toLowerCase().includes(term.toLowerCase())) ||
      (asset.version && asset.version.toLowerCase().includes(term.toLowerCase())) ||
      (asset.vendor && asset.vendor.toLowerCase().includes(term.toLowerCase())) ||
      (asset.assetId && asset.assetId.toLowerCase().includes(term.toLowerCase()))
    );
  }

  clearMessages() { this.error = ''; this.success = ''; }
  clearValidationErrors() { this.validationErrors = {}; }
  clearValidationError(field: string) { delete this.validationErrors[field]; }
  hasValidationError(field: string): boolean { return !!this.validationErrors[field]; }
  getValidationError(field: string): string { return this.validationErrors[field] || ''; }
  formatDate(date: Date | undefined): string { if (!date) return 'N/A'; return new Date(date).toLocaleDateString(); }
  formatCurrency(amount: number | undefined): string { if (!amount) return 'N/A'; return `₹${amount.toLocaleString()}`; }
}
