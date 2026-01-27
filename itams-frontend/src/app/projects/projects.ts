import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api, Project, CreateProject, Location, CreateLocation } from '../services/api';
import { ValidationService } from '../shared/validation.service';
import { INDIAN_STATES, SPV_NAMES, DISTRICTS_BY_STATE, PLAZA_NAMES, GOVERNMENT_CODES, OFFICE_NAMES, INTERNAL_LOCATIONS, LANE_OPTIONS } from '../shared/constants';

@Component({
  selector: 'app-projects',
  imports: [CommonModule, FormsModule],
  templateUrl: './projects.html',
  styleUrl: './projects.scss',
})
export class Projects implements OnInit {
  projects: Project[] = [];
  loading = false;
  error = '';
  success = '';

  // Modal and tab states
  showCreateModal = false;
  showEditModal = false;
  showLocationModal = false;
  showAddLocationModal = false;
  selectedProject: Project | null = null;
  currentTab = 1;
  maxTab = 1;
  plazaTab = 1;
  selectedLane = 0;

  // Dropdown states
  showSpvDropdown = false;
  showStateDropdown = false;
  showLocationStateDropdown = false;
  showOfficeDropdown = false;
  showDistrictDropdown = false;
  showPlazaDropdown = false;
  showGovCodeDropdown = false;
  stateSearchTerm = '';
  filteredSpvNames: string[] = [];
  filteredStates: string[] = [];
  filteredLocationStates: string[] = [];
  filteredOfficeNames: string[] = [];
  filteredDistricts: string[] = [];
  filteredPlazaNames: string[] = [];
  filteredGovCodes: string[] = [];

  // Form data
  createForm: CreateProject = {
    name: '',
    preferredName: '',
    code: '',
    spvName: '',
    states: [],
    description: ''
  };

  editForm: Partial<Project> = {};

  // Location form data
  locationForm: CreateLocation = {
    name: '',
    type: 'office',
    projectId: 0,
    internalLocations: [],
    // Plaza specific properties
    plazaCode: '',
    governmentCode: '',
    chainageNumber: '',
    latitude: undefined,
    longitude: undefined,
    numberOfLanes: undefined,
    // Office specific
    district: ''
  };

  generatedLanes: any[] = [];
  customLocation = '';

  // Constants
  spvNames = SPV_NAMES;
  indianStates = INDIAN_STATES;
  districtsByState = DISTRICTS_BY_STATE;
  plazaNames = PLAZA_NAMES;
  governmentCodes = GOVERNMENT_CODES;
  officeNames = OFFICE_NAMES;
  internalLocationOptions = INTERNAL_LOCATIONS;
  laneOptions = LANE_OPTIONS;

  // Validation states
  validationErrors: { [key: string]: string } = {};

  constructor(
    private api: Api,
    private validationService: ValidationService
  ) {
    this.filteredSpvNames = [...this.spvNames];
    this.filteredStates = [...this.indianStates];
    this.filteredLocationStates = [...this.indianStates];
    this.filteredOfficeNames = [...this.officeNames];
    this.filteredPlazaNames = [...this.plazaNames];
    this.filteredGovCodes = [...this.governmentCodes];
    this.filteredDistricts = [];
  }

  ngOnInit() {
    this.loadProjects();
  }

  loadProjects() {
    this.loading = true;
    this.api.getProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load projects';
        this.loading = false;
        console.error('Error loading projects:', error);
      }
    });
  }

  // Modal management
  openCreateModal() {
    this.resetCreateForm();
    this.currentTab = 1;
    this.maxTab = 1;
    this.showCreateModal = true;
    this.clearMessages();
  }

  openEditModal(project: Project) {
    this.selectedProject = project;
    this.editForm = {
      name: project.name,
      preferredName: project.preferredName,
      description: project.description,
      isActive: project.isActive
    };
    this.showEditModal = true;
    this.clearMessages();
  }

  openLocationModal(project: Project) {
    this.selectedProject = project;
    this.showLocationModal = true;
    this.clearMessages();
  }

  closeModals() {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.showLocationModal = false;
    this.showAddLocationModal = false;
    this.selectedProject = null;
    this.currentTab = 1;
    this.maxTab = 1;
    this.clearMessages();
    this.clearValidationErrors();
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
    }
    
    return true;
  }

  validateTab1(): boolean {
    let isValid = true;

    if (!this.createForm.spvName) {
      this.validationErrors['spvName'] = 'SPV Name is required';
      isValid = false;
    }

    if (!this.createForm.states || this.createForm.states.length === 0) {
      this.validationErrors['states'] = 'At least one state is required';
      isValid = false;
    } else if (this.createForm.states.length > 2) {
      this.validationErrors['states'] = 'Maximum 2 states allowed';
      isValid = false;
    }

    if (!this.createForm.code) {
      this.validationErrors['code'] = 'Project code is required';
      isValid = false;
    } else {
      const codeValidation = this.validationService.validateProjectCode(this.createForm.code);
      if (!codeValidation.valid) {
        this.validationErrors['code'] = codeValidation.message || 'Invalid project code';
        isValid = false;
      }
    }

    return isValid;
  }

  // Dropdown management
  toggleSpvDropdown(event: Event) {
    event.stopPropagation();
    this.showSpvDropdown = !this.showSpvDropdown;
    this.showStateDropdown = false;
    
    if (this.showSpvDropdown) {
      this.setupClickOutside(() => this.showSpvDropdown = false);
    }
  }

  toggleStateDropdown(event: Event) {
    event.stopPropagation();
    this.showStateDropdown = !this.showStateDropdown;
    this.showSpvDropdown = false;
    
    if (this.showStateDropdown) {
      this.setupClickOutside(() => this.showStateDropdown = false);
    }
  }

  onSpvInput(event: any) {
    const value = event.target.value;
    this.createForm.spvName = value;
    this.filterSpvNames(value);
    this.generateProjectCode();
    this.clearValidationError('spvName');
    
    if (value && !this.showSpvDropdown) {
      this.showSpvDropdown = true;
      this.setupClickOutside(() => this.showSpvDropdown = false);
    }
  }

  onStateInput(event: any) {
    const value = event.target.value;
    this.stateSearchTerm = value;
    this.filterStates(value);
    this.clearValidationError('states');
    
    if (value && !this.showStateDropdown) {
      this.showStateDropdown = true;
      this.setupClickOutside(() => this.showStateDropdown = false);
    }
  }

  filterSpvNames(searchTerm: string) {
    if (!searchTerm) {
      this.filteredSpvNames = [...this.spvNames];
    } else {
      this.filteredSpvNames = this.spvNames.filter(spv => 
        spv.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
  }

  filterStates(searchTerm: string) {
    if (!searchTerm) {
      this.filteredStates = [...this.indianStates];
    } else {
      this.filteredStates = this.indianStates.filter(state => 
        state.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
  }

  toggleStateSelection(state: string) {
    if (this.isStateSelected(state)) {
      this.removeState(state);
    } else if (this.createForm.states.length < 2) {
      this.createForm.states.push(state);
      this.generateProjectCode();
      this.updateDistrictOptions();
      this.clearValidationError('states');
    }
    this.stateSearchTerm = '';
    this.showStateDropdown = false;
  }

  isStateSelected(state: string): boolean {
    return this.createForm.states.includes(state);
  }

  removeState(state: string) {
    const index = this.createForm.states.indexOf(state);
    if (index > -1) {
      this.createForm.states.splice(index, 1);
      this.generateProjectCode();
      this.updateDistrictOptions();
      this.clearValidationError('states');
    }
  }

  updateDistrictOptions() {
    // Combine districts from all selected states
    this.filteredDistricts = [];
    this.createForm.states.forEach(state => {
      const stateDistricts = this.districtsByState[state] || [];
      this.filteredDistricts = [...this.filteredDistricts, ...stateDistricts];
    });
    // Remove duplicates
    this.filteredDistricts = [...new Set(this.filteredDistricts)];
  }

  selectSpv(spv: string) {
    this.createForm.spvName = spv;
    this.createForm.name = spv; // Set as default name
    this.showSpvDropdown = false;
    this.generateProjectCode();
    this.clearValidationError('spvName');
  }

  // Auto-generation
  generateProjectCode() {
    if (this.createForm.spvName && this.createForm.states.length > 0) {
      this.createForm.code = this.validationService.generateProjectCode(
        this.createForm.spvName, 
        this.createForm.states
      );
      this.clearValidationError('code');
    }
  }

  onProjectCodeChange() {
    this.createForm.code = this.createForm.code.toUpperCase();
    this.clearValidationError('code');
  }

  // CRUD operations
  createProject() {
    if (!this.validateTab1()) {
      return;
    }

    this.loading = true;
    this.api.createProject(this.createForm).subscribe({
      next: (project) => {
        this.projects.push(project);
        this.success = 'Project created successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to create project';
        this.loading = false;
        console.error('Error creating project:', error);
      }
    });
  }

  updateProject() {
    if (!this.selectedProject) return;

    this.loading = true;
    this.api.updateProject(this.selectedProject.id, this.editForm).subscribe({
      next: (updatedProject) => {
        const index = this.projects.findIndex(p => p.id === this.selectedProject!.id);
        if (index !== -1) {
          this.projects[index] = updatedProject;
        }
        this.success = 'Project updated successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to update project';
        this.loading = false;
        console.error('Error updating project:', error);
      }
    });
  }

  deactivateProject(project: Project) {
    if (confirm(`Are you sure you want to deactivate project ${project.name}?`)) {
      this.loading = true;
      this.api.updateProject(project.id, { isActive: false }).subscribe({
        next: (updatedProject) => {
          const index = this.projects.findIndex(p => p.id === project.id);
          if (index !== -1) {
            this.projects[index] = updatedProject;
          }
          this.success = 'Project deactivated successfully';
          this.loading = false;
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to deactivate project';
          this.loading = false;
          console.error('Error deactivating project:', error);
        }
      });
    }
  }

  activateProject(project: Project) {
    this.loading = true;
    this.api.updateProject(project.id, { isActive: true }).subscribe({
      next: (updatedProject) => {
        const index = this.projects.findIndex(p => p.id === project.id);
        if (index !== -1) {
          this.projects[index] = updatedProject;
        }
        this.success = 'Project activated successfully';
        this.loading = false;
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to activate project';
        this.loading = false;
        console.error('Error activating project:', error);
      }
    });
  }

  // Utility methods
  resetCreateForm() {
    this.createForm = {
      name: '',
      preferredName: '',
      code: '',
      spvName: '',
      states: [],
      description: ''
    };
    this.stateSearchTerm = '';
    this.filteredSpvNames = [...this.spvNames];
    this.filteredStates = [...this.indianStates];
    this.filteredDistricts = [];
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
      return !!(this.createForm.spvName && this.createForm.states.length > 0 && this.createForm.code &&
             this.validationService.validateProjectCode(this.createForm.code).valid);
    }
    return true;
  }

  setupClickOutside(callback: () => void) {
    const closeDropdown = (e: Event) => {
      callback();
      document.removeEventListener('click', closeDropdown);
    };
    setTimeout(() => document.addEventListener('click', closeDropdown), 0);
  }

  // Location Management Methods
  openAddLocationModal() {
    this.resetLocationForm();
    this.plazaTab = 1;
    this.showAddLocationModal = true;
  }

  closeAddLocationModal() {
    this.showAddLocationModal = false;
    this.resetLocationForm();
  }

  resetLocationForm() {
    this.locationForm = {
      name: '',
      type: 'office',
      projectId: this.selectedProject?.id || 0,
      internalLocations: [],
      // Plaza specific properties
      plazaCode: '',
      governmentCode: '',
      chainageNumber: '',
      latitude: undefined,
      longitude: undefined,
      numberOfLanes: undefined,
      // Office specific
      district: ''
    };
    this.generatedLanes = [];
    this.selectedLane = 0;
    this.customLocation = '';
  }

  // Office form methods
  onOfficeNameInput(event: any) {
    const value = event.target.value;
    this.locationForm.name = value;
    this.filterOfficeNames(value);
  }

  filterOfficeNames(searchTerm: string) {
    if (!searchTerm) {
      this.filteredOfficeNames = [...this.officeNames];
    } else {
      this.filteredOfficeNames = this.officeNames.filter(office => 
        office.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
  }

  selectOfficeName(office: string) {
    this.locationForm.office = office;
    this.showOfficeDropdown = false;
  }

  // Location state methods
  onLocationStateInput(event: any) {
    const value = event.target.value;
    this.locationForm.state = value;
    this.filterLocationStates(value);
  }

  filterLocationStates(searchTerm: string) {
    if (!searchTerm) {
      this.filteredLocationStates = [...this.indianStates];
    } else {
      this.filteredLocationStates = this.indianStates.filter(state => 
        state.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
  }

  selectLocationState(state: string) {
    this.locationForm.state = state;
    this.showLocationStateDropdown = false;
  }

  onDistrictInput(event: any) {
    const value = event.target.value;
    this.locationForm.district = value;
    this.filterDistricts(value);
  }

  filterDistricts(searchTerm: string) {
    // Get districts from all selected states
    let allDistricts: string[] = [];
    this.createForm.states.forEach(state => {
      const stateDistricts = this.districtsByState[state] || [];
      allDistricts = [...allDistricts, ...stateDistricts];
    });
    // Remove duplicates
    allDistricts = [...new Set(allDistricts)];
    
    if (!searchTerm) {
      this.filteredDistricts = [...allDistricts];
    } else {
      this.filteredDistricts = allDistricts.filter(district => 
        district.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
  }

  selectDistrict(district: string) {
    this.locationForm.district = district;
    this.showDistrictDropdown = false;
  }

  // Plaza form methods
  onPlazaNameInput(event: any) {
    const value = event.target.value;
    this.locationForm.name = value;
    this.filterPlazaNames(value);
  }

  filterPlazaNames(searchTerm: string) {
    if (!searchTerm) {
      this.filteredPlazaNames = [...this.plazaNames];
    } else {
      this.filteredPlazaNames = this.plazaNames.filter(plaza => 
        plaza.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
  }

  selectPlazaName(plaza: string) {
    this.locationForm.plaza = plaza;
    this.showPlazaDropdown = false;
  }

  onPlazaCodeChange() {
    if (this.locationForm.plazaCode) {
      this.locationForm.plazaCode = this.locationForm.plazaCode.toUpperCase();
    }
  }

  onGovCodeInput(event: any) {
    const value = event.target.value.toUpperCase();
    this.locationForm.governmentCode = value;
    this.filterGovCodes(value);
  }

  filterGovCodes(searchTerm: string) {
    if (!searchTerm) {
      this.filteredGovCodes = [...this.governmentCodes];
    } else {
      this.filteredGovCodes = this.governmentCodes.filter(code => 
        code.includes(searchTerm)
      );
    }
  }

  selectGovCode(code: string) {
    this.locationForm.governmentCode = code;
    this.showGovCodeDropdown = false;
  }

  onChainageInput(event: any) {
    const formatted = this.validationService.formatChainageNumber(event.target.value);
    this.locationForm.chainageNumber = formatted;
    event.target.value = formatted;
  }

  generateLanes() {
    const numLanes = this.locationForm.numberOfLanes || 0;
    this.generatedLanes = Array.from({ length: numLanes }, (_, i) => ({
      laneNumber: i + 1,
      // Future expandable properties
    }));
    this.selectedLane = 0;
  }

  toggleInternalLocation(location: string, event: any) {
    if (!this.locationForm.internalLocations) {
      this.locationForm.internalLocations = [];
    }

    if (event.target.checked) {
      this.locationForm.internalLocations.push(location);
    } else {
      const index = this.locationForm.internalLocations.indexOf(location);
      if (index > -1) {
        this.locationForm.internalLocations.splice(index, 1);
      }
    }
  }

  addCustomLocation() {
    if (this.customLocation?.trim()) {
      if (!this.locationForm.internalLocations) {
        this.locationForm.internalLocations = [];
      }
      
      if (!this.locationForm.internalLocations.includes(this.customLocation.trim())) {
        this.locationForm.internalLocations.push(this.customLocation.trim());
        this.internalLocationOptions.push(this.customLocation.trim());
      }
      
      this.customLocation = '';
    }
  }

  getLocationId(location: string): string {
    return 'internal-' + location.replace(/\s+/g, '-').toLowerCase();
  }

  isLocationFormValid(): boolean {
    if (!this.locationForm.name || !this.locationForm.type) {
      return false;
    }

    if (this.locationForm.type === 'office') {
      return !!(this.locationForm.name && this.locationForm.district);
    }

    if (this.locationForm.type === 'plaza') {
      return !!(
        this.locationForm.name &&
        this.locationForm.plazaCode &&
        this.locationForm.governmentCode &&
        this.locationForm.chainageNumber &&
        this.locationForm.latitude &&
        this.locationForm.longitude
      );
    }

    return false;
  }

  getLocationValidationMessage(): string {
    if (!this.locationForm.name) return 'Location name is required';
    if (!this.locationForm.type) return 'Location type is required';

    if (this.locationForm.type === 'office') {
      if (!this.locationForm.district) return 'District is required for office locations';
    }

    if (this.locationForm.type === 'plaza') {
      if (!this.locationForm.plazaCode) return 'Plaza code is required';
      if (!this.locationForm.governmentCode) return 'Government code is required';
      if (!this.locationForm.chainageNumber) return 'Chainage number is required';
      if (!this.locationForm.latitude) return 'Latitude is required';
      if (!this.locationForm.longitude) return 'Longitude is required';
    }

    return '';
  }

  saveLocation() {
    if (!this.isLocationFormValid()) {
      this.error = this.getLocationValidationMessage() || 'Please fill all required fields';
      return;
    }

    if (!this.selectedProject || !this.selectedProject.id) {
      this.error = 'No project selected. Please select a project first.';
      return;
    }

    this.loading = true;
    this.locationForm.projectId = this.selectedProject.id;
    
    // Map frontend data to backend format
    const backendLocation: any = {
      name: this.locationForm.name,
      projectId: this.locationForm.projectId,
      region: 'India', // Default region
      state: this.locationForm.type === 'office' ? 'Unknown' : 'Highway', // Default state
    };

    // Add type-specific fields only if they have values
    if (this.locationForm.type === 'plaza') {
      backendLocation.plaza = this.locationForm.name;
      const address = `${this.locationForm.plazaCode || ''} ${this.locationForm.governmentCode || ''} ${this.locationForm.chainageNumber || ''}`.trim();
      if (address) {
        backendLocation.address = address;
      }
    } else if (this.locationForm.type === 'office') {
      backendLocation.office = this.locationForm.name;
      if (this.locationForm.district) {
        backendLocation.address = this.locationForm.district;
      }
    }

    console.log('Sending location data:', JSON.stringify(backendLocation, null, 2)); // Debug log
    console.log('Selected project:', this.selectedProject); // Debug log
    
    this.api.createLocation(backendLocation).subscribe({
      next: (location) => {
        this.success = `${this.locationForm.type === 'office' ? 'Office' : 'Plaza'} location created successfully`;
        this.loading = false;
        this.closeAddLocationModal();
        // Refresh project data
        this.loadProjects();
      },
      error: (error) => {
        console.error('Location creation error:', error); // Debug log
        this.error = error.error?.message || 'Failed to create location';
        this.loading = false;
      }
    });
  }
}
