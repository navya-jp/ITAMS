import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.authService.isAuthenticated$.pipe(
      take(1),
      map(isAuthenticated => {
        if (isAuthenticated) {
          // Check if user is trying to access admin routes
          if (state.url.startsWith('/admin')) {
            if (this.authService.isSuperAdmin()) {
              return true;
            } else {
              // Redirect regular users to user dashboard
              this.router.navigate(['/user/dashboard']);
              return false;
            }
          }
          
          // Check if user is trying to access user routes
          if (state.url.startsWith('/user')) {
            if (this.authService.isUser()) {
              return true;
            } else {
              // Redirect super admin to admin dashboard
              this.router.navigate(['/admin/dashboard']);
              return false;
            }
          }
          
          return true;
        } else {
          // Not authenticated, redirect to login
          this.router.navigate(['/login'], { 
            queryParams: { returnUrl: state.url } 
          });
          return false;
        }
      })
    );
  }
}

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.authService.isAuthenticated$.pipe(
      take(1),
      map(isAuthenticated => {
        if (isAuthenticated && this.authService.isSuperAdmin()) {
          return true;
        } else if (isAuthenticated) {
          // Authenticated but not admin, redirect to user dashboard
          this.router.navigate(['/user/dashboard']);
          return false;
        } else {
          // Not authenticated, redirect to login
          this.router.navigate(['/login']);
          return false;
        }
      })
    );
  }
}

@Injectable({
  providedIn: 'root'
})
export class UserGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.authService.isAuthenticated$.pipe(
      take(1),
      map(isAuthenticated => {
        if (isAuthenticated && this.authService.isUser()) {
          return true;
        } else if (isAuthenticated) {
          // Authenticated but admin, redirect to admin dashboard
          this.router.navigate(['/admin/dashboard']);
          return false;
        } else {
          // Not authenticated, redirect to login
          this.router.navigate(['/login']);
          return false;
        }
      })
    );
  }
}

@Injectable({
  providedIn: 'root'
})
export class LoginGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean> | Promise<boolean> | boolean {
    return this.authService.isAuthenticated$.pipe(
      take(1),
      map(isAuthenticated => {
        if (isAuthenticated) {
          // Already authenticated, redirect based on role
          if (this.authService.isSuperAdmin()) {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.router.navigate(['/user/dashboard']);
          }
          return false;
        }
        return true;
      })
    );
  }
}