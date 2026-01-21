import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api, Location, CreateLocation, Project } from '../services/api';

@Component({
  selector: 'app-locations',
  imports: [CommonModule, FormsModule],
  templateUrl: './locations.html',
  styleUrl: './locations.scss',
})
export class Locations implements OnInit {
  locations: Location[] = [];
  projects: Project[] = [];
  loading = false;
  error = '';
  success = '';

  // Modal states
  showCreateModal = false;
  selectedLocation: Location | null = null;

  // Form data
  createForm: CreateLocation = {
    name: '',
    projectId: 0,
    region: '',
    state: '',
    plaza: '',
    lane: '',
    office: '',
    address: ''
  };

  // Filter
  selectedProjectFilter = 0;

  // Automated dropdown options
  regions = [
    'North America', 'South America', 'Europe', 'Asia Pacific', 'Middle East', 'Africa',
    'North Region', 'South Region', 'East Region', 'West Region', 'Central Region'
  ];

  states = [
    'Alabama', 'Alaska', 'Arizona', 'Arkansas', 'California', 'Colorado', 'Connecticut',
    'Delaware', 'Florida', 'Georgia', 'Hawaii', 'Idaho', 'Illinois', 'Indiana', 'Iowa',
    'Kansas', 'Kentucky', 'Louisiana', 'Maine', 'Maryland', 'Massachusetts', 'Michigan',
    'Minnesota', 'Mississippi', 'Missouri', 'Montana', 'Nebraska', 'Nevada', 'New Hampshire',
    'New Jersey', 'New Mexico', 'New York', 'North Carolina', 'North Dakota', 'Ohio',
    'Oklahoma', 'Oregon', 'Pennsylvania', 'Rhode Island', 'South Carolina', 'South Dakota',
    'Tennessee', 'Texas', 'Utah', 'Vermont', 'Virginia', 'Washington', 'West Virginia',
    'Wisconsin', 'Wyoming'
  ];

  plazas = [
    'Main Plaza', 'Business Plaza', 'Corporate Plaza', 'Tech Plaza', 'Innovation Plaza',
    'Central Plaza', 'Executive Plaza', 'Commerce Plaza', 'Trade Plaza', 'Professional Plaza'
  ];

  officeTypes = [
    'Head Office', 'Branch Office', 'Regional Office', 'Sales Office', 'Support Office',
    'Administrative Office', 'Operations Office', 'Customer Service Office', 'IT Office',
    'Finance Office', 'HR Office', 'Marketing Office'
  ];

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadLocations();
    this.loadProjects();
  }

  loadLocations() {
    this.loading = true;
    this.api.getLocations().subscribe({
      next: (locations) => {
        this.locations = locations;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load locations';
        this.loading = false;
        console.error('Error loading locations:', error);
      }
    });
  }

  loadProjects() {
    this.api.getProjects().subscribe({
      next: (projects) => {
        this.projects = projects.filter(p => p.isActive);
      },
      error: (error) => {
        console.error('Error loading projects:', error);
      }
    });
  }

  openCreateModal() {
    this.createForm = {
      name: '',
      projectId: 0,
      region: '',
      state: '',
      plaza: '',
      lane: '',
      office: '',
      address: ''
    };
    this.showCreateModal = true;
    this.error = '';
    this.success = '';
  }

  closeModals() {
    this.showCreateModal = false;
    this.selectedLocation = null;
    this.error = '';
    this.success = '';
  }

  // Auto-select dropdown options
  selectRegion(region: string) {
    this.createForm.region = region;
  }

  selectState(state: string) {
    this.createForm.state = state;
  }

  selectPlaza(plaza: string) {
    this.createForm.plaza = plaza;
  }

  selectOffice(office: string) {
    this.createForm.office = office;
  }

  // Auto-generate location name
  generateLocationName() {
    if (this.createForm.region && this.createForm.state) {
      const projectName = this.getProjectName(this.createForm.projectId);
      if (projectName !== 'Unknown') {
        this.createForm.name = `${projectName} - ${this.createForm.region} - ${this.createForm.state}`;
      }
    }
  }

  createLocation() {
    if (!this.isFormValid()) {
      this.error = 'Please fill all required fields correctly';
      return;
    }

    this.loading = true;
    this.api.createLocation(this.createForm).subscribe({
      next: (location) => {
        this.locations.push(location);
        this.success = 'Location created successfully';
        this.loading = false;
        this.closeModals();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to create location';
        this.loading = false;
        console.error('Error creating location:', error);
      }
    });
  }

  isFormValid(): boolean {
    return this.createForm.name.length >= 2 &&
           this.createForm.projectId > 0 &&
           this.createForm.region.length >= 2 &&
           this.createForm.state.length >= 2;
  }

  getProjectName(projectId: number): string {
    const project = this.projects.find(p => p.id === projectId);
    return project ? project.name : 'Unknown';
  }

  getFilteredLocations(): Location[] {
    if (this.selectedProjectFilter === 0) {
      return this.locations;
    }
    return this.locations.filter(l => l.projectId === this.selectedProjectFilter);
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }
}
