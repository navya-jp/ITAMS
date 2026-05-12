import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../services/api';

@Component({
  selector: 'app-asset-config',
  imports: [CommonModule, FormsModule],
  templateUrl: './asset-config.html'
})
export class AssetConfig implements OnInit {
  activeTab: 'subtypes' | 'properties' = 'subtypes';

  // Subtypes
  subtypes: any[] = [];
  assetTypes: any[] = [];
  newSubtype = { subTypeName: '', typeId: 0 };
  addingSubtype = false;
  subtypeSuccess = '';
  subtypeError = '';

  // Properties
  properties: any[] = [];
  newProperty = { propertyName: '', applicableSubtype: '', dataType: 'Text' };
  addingProperty = false;
  propertySuccess = '';
  propertyError = '';

  commonSubtypes = ['Desktop','Laptop','Server','Monitor','Printer','Scanner','UPS','Switch','Router','Firewall','Keyboard','Camera','NVR'];

  // Show the fixed base list + any user-added custom ones
  get displayedSubtypes() {
    const base = this.commonSubtypes.map(name => {
      const found = this.subtypes.find((s: any) => s.subTypeName === name);
      return found ?? { subTypeName: name, isActive: true, isBase: true };
    });
    const custom = this.subtypes.filter((s: any) => !this.commonSubtypes.includes(s.subTypeName));
    return [...base, ...custom].sort((a: any, b: any) => a.subTypeName.localeCompare(b.subTypeName));
  }

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadSubtypes();
    this.loadAssetTypes();
    this.loadProperties();
  }

  loadSubtypes() {
    this.api.getAssetSubTypes(undefined, true).subscribe({ next: (s) => this.subtypes = s, error: () => {} });
  }

  loadAssetTypes() {
    this.api.getAssetTypes().subscribe({ next: (t) => this.assetTypes = t, error: () => {} });
  }

  loadProperties() {
    this.api.getAssetProperties(undefined, true).subscribe({ next: (p) => this.properties = p, error: () => {} });
  }

  toggleSubtype(s: any) {
    this.api.toggleAssetSubtype(s.id).subscribe({ next: (res) => { s.isActive = res.isActive; } });
  }

  toggleProperty(p: any) {
    this.api.toggleAssetProperty(p.id).subscribe({ next: (res) => { p.isActive = res.isActive; } });
  }

  addSubtype() {
    if (!this.newSubtype.subTypeName.trim()) { this.subtypeError = 'Name is required'; return; }
    // Use the MasterData controller's asset-subtypes endpoint via a POST
    this.api.createAssetSubtype(this.newSubtype).subscribe({
      next: (s) => {
        this.subtypes.push({ ...s, isUserAdded: true });
        this.newSubtype = { subTypeName: '', typeId: 0 };
        this.addingSubtype = false;
        this.subtypeSuccess = 'Subtype added successfully';
        setTimeout(() => this.subtypeSuccess = '', 3000);
      },
      error: (e) => { this.subtypeError = e.error?.message || 'Failed to add subtype'; }
    });
  }

  addProperty() {
    if (!this.newProperty.propertyName.trim()) { this.propertyError = 'Name is required'; return; }
    this.api.createAssetProperty(this.newProperty).subscribe({
      next: (p) => {
        this.properties.push(p);
        this.newProperty = { propertyName: '', applicableSubtype: '', dataType: 'Text' };
        this.addingProperty = false;
        this.propertySuccess = 'Property added successfully';
        setTimeout(() => this.propertySuccess = '', 3000);
      },
      error: (e) => { this.propertyError = e.error?.message || 'Failed to add property'; }
    });
  }

  getTypeName(typeId: number): string {
    return this.assetTypes.find(t => t.id === typeId)?.typeName ?? '-';
  }
}
