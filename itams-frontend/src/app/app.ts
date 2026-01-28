import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Navigation } from './navigation/navigation';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, Navigation],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected title = 'ITAMS - IT Asset Management System';

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // Initialize authentication state
    // The AuthService constructor already handles this
  }
}
