import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../services/api';

interface SystemSetting {
  id: number;
  settingKey: string;
  settingValue: string;
  description: string;
  category: string;
  dataType: string;
  isEditable: boolean;
  updatedAt?: string;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html',
  styleUrls: ['./settings.scss']
})
export class Settings implements OnInit {
  settings: SystemSetting[] = [];
  filteredSettings: SystemSetting[] = [];
  loading = false;
  saving = false;
  error: string | null = null;
  success: string | null = null;
  
  selectedCategory = 'All';
  categories: string[] = [];
  
  editedSettings: Map<number, string> = new Map();

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadSettings();
  }

  loadSettings() {
    this.loading = true;
    this.error = null;

    this.api.getSystemSettings()
      .subscribe({
        next: (data: SystemSetting[]) => {
          this.settings = data;
          this.extractCategories();
          this.filterSettings();
          this.loading = false;
        },
        error: (err: any) => {
          this.error = 'Failed to load system settings';
          this.loading = false;
          console.error('Error loading settings:', err);
        }
      });
  }

  extractCategories() {
    const uniqueCategories = [...new Set(this.settings.map(s => s.category))];
    this.categories = ['All', ...uniqueCategories.sort()];
  }

  filterSettings() {
    if (this.selectedCategory === 'All') {
      this.filteredSettings = this.settings;
    } else {
      this.filteredSettings = this.settings.filter(s => s.category === this.selectedCategory);
    }
  }

  onCategoryChange() {
    this.filterSettings();
  }

  onSettingChange(setting: SystemSetting, newValue: string) {
    this.editedSettings.set(setting.id, newValue);
  }

  hasChanges(): boolean {
    return this.editedSettings.size > 0;
  }

  resetChanges() {
    this.editedSettings.clear();
    this.success = null;
    this.error = null;
  }

  saveChanges() {
    if (!this.hasChanges()) {
      return;
    }

    this.saving = true;
    this.error = null;
    this.success = null;

    const updates = Array.from(this.editedSettings.entries()).map(([id, value]) => ({
      id,
      settingValue: value
    }));

    this.api.bulkUpdateSettings(updates)
      .subscribe({
        next: (response: any) => {
          this.success = response.message || 'Settings updated successfully';
          this.editedSettings.clear();
          this.saving = false;
          
          // Reload settings to get updated values
          setTimeout(() => {
            this.loadSettings();
            this.success = null;
          }, 2000);
        },
        error: (err: any) => {
          this.error = 'Failed to update settings';
          this.saving = false;
          console.error('Error updating settings:', err);
        }
      });
  }

  getSettingValue(setting: SystemSetting): string {
    return this.editedSettings.get(setting.id) ?? setting.settingValue;
  }

  getInputType(dataType: string): string {
    switch (dataType) {
      case 'Integer':
      case 'Decimal':
        return 'number';
      case 'Boolean':
        return 'checkbox';
      default:
        return 'text';
    }
  }

  isBooleanType(dataType: string): boolean {
    return dataType === 'Boolean';
  }

  getBooleanValue(setting: SystemSetting): boolean {
    const value = this.getSettingValue(setting);
    return value.toLowerCase() === 'true';
  }

  onBooleanChange(setting: SystemSetting, checked: boolean) {
    this.onSettingChange(setting, checked.toString());
  }

  getCategoryIcon(category: string): string {
    switch (category) {
      case 'Security':
        return 'fas fa-shield-alt';
      case 'General':
        return 'fas fa-cog';
      default:
        return 'fas fa-sliders-h';
    }
  }

  getCategoryClass(category: string): string {
    switch (category) {
      case 'Security':
        return 'badge bg-danger';
      case 'General':
        return 'badge bg-primary';
      default:
        return 'badge bg-secondary';
    }
  }
}
