import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MasterDataService, Vendor, CreateVendorRequest, UpdateVendorRequest } from '../../services/master-data.service';

@Component({
  selector: 'app-vendors',
  imports: [CommonModule, FormsModule],
  templateUrl: './vendors.html',
  styleUrl: './vendors.scss',
})
export class Vendors implements OnInit {
  vendors: Vendor[] = [];
  filteredVendors: Vendor[] = [];
  loading = false;
  showForm = false;
  editMode = false;
  includeInactive = false;
  searchTerm = '';

  // Form model
  vendorForm: CreateVendorRequest & { id?: number; isActive?: boolean } = {
    vendorName: '',
    vendorCode: '',
    email: '',
    contactPerson: '',
    phoneNumber: '',
    address: '',
    website: '',
    isActive: true
  };

  errorMessage = '';
  successMessage = '';

  constructor(private masterDataService: MasterDataService) {}

  ngOnInit() {
    this.loadVendors();
  }

  loadVendors() {
    this.loading = true;
    this.masterDataService.getVendors(this.includeInactive).subscribe({
      next: (data) => {
        this.vendors = data;
        this.filterVendors();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading vendors:', error);
        this.showError('Failed to load vendors');
        this.loading = false;
      }
    });
  }

  filterVendors() {
    if (!this.searchTerm) {
      this.filteredVendors = this.vendors;
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredVendors = this.vendors.filter(v =>
        v.vendorName.toLowerCase().includes(term) ||
        v.vendorCode.toLowerCase().includes(term) ||
        (v.email && v.email.toLowerCase().includes(term))
      );
    }
  }

  onSearchChange() {
    this.filterVendors();
  }

  toggleIncludeInactive() {
    this.includeInactive = !this.includeInactive;
    this.loadVendors();
  }

  showAddForm() {
    this.resetForm();
    this.editMode = false;
    this.showForm = true;
  }

  editVendor(vendor: Vendor) {
    this.vendorForm = {
      id: vendor.id,
      vendorName: vendor.vendorName,
      vendorCode: vendor.vendorCode,
      email: vendor.email,
      contactPerson: vendor.contactPerson || '',
      phoneNumber: vendor.phoneNumber || '',
      address: vendor.address || '',
      website: vendor.website || '',
      isActive: vendor.isActive
    };
    this.editMode = true;
    this.showForm = true;
  }

  cancelForm() {
    this.showForm = false;
    this.resetForm();
  }

  resetForm() {
    this.vendorForm = {
      vendorName: '',
      vendorCode: '',
      email: '',
      contactPerson: '',
      phoneNumber: '',
      address: '',
      website: '',
      isActive: true
    };
    this.errorMessage = '';
  }

  saveVendor() {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.editMode && this.vendorForm.id) {
      const updateRequest: UpdateVendorRequest = {
        vendorName: this.vendorForm.vendorName,
        email: this.vendorForm.email,
        contactPerson: this.vendorForm.contactPerson,
        phoneNumber: this.vendorForm.phoneNumber,
        address: this.vendorForm.address,
        website: this.vendorForm.website,
        isActive: this.vendorForm.isActive || true
      };

      this.masterDataService.updateVendor(this.vendorForm.id, updateRequest).subscribe({
        next: () => {
          this.showSuccess('Vendor updated successfully');
          this.loadVendors();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error updating vendor:', error);
          this.showError(error.error?.message || 'Failed to update vendor');
          this.loading = false;
        }
      });
    } else {
      const createRequest: CreateVendorRequest = {
        vendorName: this.vendorForm.vendorName,
        vendorCode: this.vendorForm.vendorCode,
        email: this.vendorForm.email,
        contactPerson: this.vendorForm.contactPerson,
        phoneNumber: this.vendorForm.phoneNumber,
        address: this.vendorForm.address,
        website: this.vendorForm.website
      };

      this.masterDataService.createVendor(createRequest).subscribe({
        next: () => {
          this.showSuccess('Vendor created successfully');
          this.loadVendors();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error creating vendor:', error);
          this.showError(error.error?.message || 'Failed to create vendor');
          this.loading = false;
        }
      });
    }
  }

  deleteVendor(vendor: Vendor) {
    if (!confirm(`Are you sure you want to delete vendor "${vendor.vendorName}"?`)) {
      return;
    }

    this.loading = true;
    this.masterDataService.deleteVendor(vendor.id).subscribe({
      next: () => {
        this.showSuccess('Vendor deleted successfully');
        this.loadVendors();
      },
      error: (error) => {
        console.error('Error deleting vendor:', error);
        this.showError(error.error?.message || 'Failed to delete vendor');
        this.loading = false;
      }
    });
  }

  validateForm(): boolean {
    if (!this.vendorForm.vendorName.trim()) {
      this.showError('Vendor name is required');
      return false;
    }
    if (!this.editMode && !this.vendorForm.vendorCode.trim()) {
      this.showError('Vendor code is required');
      return false;
    }
    if (!this.vendorForm.email.trim()) {
      this.showError('Email is required');
      return false;
    }
    if (!this.isValidEmail(this.vendorForm.email)) {
      this.showError('Please enter a valid email address');
      return false;
    }
    return true;
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
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
