import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-locations',
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12">
          <h2>Location Management</h2>
          <div class="alert alert-info">
            <i class="fas fa-info-circle me-2"></i>
            Location management is being redesigned with the new UX requirements.
            This will be available soon with the new tab-based, no-scroll interface.
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrl: './locations.scss',
})
export class Locations implements OnInit {
  constructor() {}

  ngOnInit() {}
}
