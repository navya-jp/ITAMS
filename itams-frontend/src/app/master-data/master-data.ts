import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-master-data',
  imports: [CommonModule, RouterModule],
  templateUrl: './master-data.html',
  styleUrl: './master-data.scss',
})
export class MasterData implements OnInit {
  activeTab = 'vendors';

  constructor(private router: Router) {}

  ngOnInit() {
    // Set active tab based on current route
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        if (event.url.includes('/vendors')) {
          this.activeTab = 'vendors';
        } else if (event.url.includes('/statuses')) {
          this.activeTab = 'statuses';
        } else if (event.url.includes('/criticality')) {
          this.activeTab = 'criticality';
        }
      });

    // Set initial tab based on current URL
    const currentUrl = this.router.url;
    if (currentUrl.includes('/vendors')) {
      this.activeTab = 'vendors';
    } else if (currentUrl.includes('/statuses')) {
      this.activeTab = 'statuses';
    } else if (currentUrl.includes('/criticality')) {
      this.activeTab = 'criticality';
    }
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    this.router.navigate(['/admin/master-data', tab === 'vendors' ? 'vendors' : tab === 'statuses' ? 'statuses' : 'criticality']);
  }
}
