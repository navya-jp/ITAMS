import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Api } from '../services/api';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  totalUsers = 0;
  activeUsers = 0;
  totalProjects = 0;
  totalLocations = 0;

  constructor(private api: Api) {}

  ngOnInit() {
    this.loadStats();
  }

  private loadStats() {
    // Load users stats
    this.api.getUsers().subscribe({
      next: (users) => {
        this.totalUsers = users.length;
        this.activeUsers = users.filter(u => u.isActive && !u.isLocked).length;
      },
      error: (error) => console.error('Error loading users:', error)
    });

    // Load projects stats
    this.api.getProjects().subscribe({
      next: (projects) => {
        this.totalProjects = projects.length;
      },
      error: (error) => console.error('Error loading projects:', error)
    });

    // Load locations stats
    this.api.getLocations().subscribe({
      next: (locations) => {
        this.totalLocations = locations.length;
      },
      error: (error) => console.error('Error loading locations:', error)
    });
  }
}
