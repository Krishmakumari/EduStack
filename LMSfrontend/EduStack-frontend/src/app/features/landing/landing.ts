import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { RouterLink, Router, RouterModule } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Footer } from '../../core/footer/footer';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, RouterModule, CommonModule, Footer],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing {
  currentYear = new Date().getFullYear();
  showUserMenu = false;
  private isBrowser: boolean;

  constructor(
    public auth: AuthService,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: Object,
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit() {
    if (this.isBrowser && this.auth.isLoggedIn() && this.userRole === 'Admin') {
      this.router.navigate(['/admin/dashboard']);
    }
  }

  get userName(): string {
    return this.isBrowser ? (localStorage.getItem('userFullName') || 'User') : 'User';
  }

  get userRole(): string {
    return this.isBrowser ? (localStorage.getItem('userRole') || '') : '';
  }

  get userInitial(): string {
    return this.userName.charAt(0).toUpperCase();
  }

  toggleUserMenu() {
    this.showUserMenu = !this.showUserMenu;
  }

  logout() {
    this.auth.logout();
    this.showUserMenu = false;
    this.router.navigate(['/']);
  }
}
