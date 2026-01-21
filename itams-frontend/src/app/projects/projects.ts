import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api, Project, CreateProject } from '../services/api';

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

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  selectedProject: Project | null = null;

  // Form data
  createForm: CreateProject = {
    name: '',
    code: '',
    description: ''
  };

  editForm: Partial<Project> = {};

  // Automated dropdown options
  projectTypes = [
    'IT Infrastructure', 'Software Development', 'Asset Management', 'Digital Transformation',
    'Cloud Migration', 'Security Enhancement', 'Data Analytics', 'Mobile Application',
    'Web Platform', 'Enterprise System', 'Customer Portal', 'Internal Tools'
  ];

  constructor(private api: Api) {}

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

  openCreateModal() {
    this.createForm = {
      name: '',
      code: '',
      description: ''
    };
    this.showCreateModal = true;
    this.error = '';
    this.success = '';
  }

  openEditModal(project: Project) {
    this.selectedProject = project;
    this.editForm = {
      name: project.name,
      description: project.description,
      isActive: project.isActive
    };
    this.showEditModal = true;
    this.error = '';
    this.success = '';
  }

  closeModals() {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.selectedProject = null;
    this.error = '';
    this.success = '';
  }

  // Auto-generate project code from name
  generateProjectCode() {
    if (this.createForm.name) {
      const code = this.createForm.name
        .toUpperCase()
        .replace(/[^A-Z0-9\s]/g, '')
        .split(' ')
        .map(word => word.substring(0, 3))
        .join('_')
        .substring(0, 20);
      this.createForm.code = code;
    }
  }

  // Select project type
  selectProjectType(type: string) {
    this.createForm.name = type;
    this.generateProjectCode();
  }

  validateProjectCode(code: string): boolean {
    const codeRegex = /^[A-Z0-9_-]+$/;
    return codeRegex.test(code);
  }

  createProject() {
    if (!this.isFormValid()) {
      this.error = 'Please fill all required fields correctly';
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

  // Deactivate project instead of delete
  deactivateProject(project: Project) {
    if (confirm(`Are you sure you want to deactivate project ${project.name}? This will affect all associated locations and assets.`)) {
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

  // Activate project
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

  isFormValid(): boolean {
    return this.createForm.name.length >= 2 &&
           this.createForm.code.length >= 2 &&
           this.validateProjectCode(this.createForm.code);
  }

  clearMessages() {
    this.error = '';
    this.success = '';
  }
}
