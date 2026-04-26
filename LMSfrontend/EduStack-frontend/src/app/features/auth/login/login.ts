import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  email = '';
  password = '';
  errorMessage = '';
  successMessage = '';
  loading = false;

  constructor(
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    // Show success message if redirected from register or password reset
    this.route.queryParams.subscribe((params) => {
      if (params['registered'] === 'true') {
        this.successMessage = 'Registration successful! You can now sign in.';
      }
      if (params['passwordReset'] === 'true') {
        this.successMessage = 'Password reset successful! Sign in with your new password.';
      }
    });
  }

  onLogin() {
    this.errorMessage = '';
    this.successMessage = '';
    this.loading = true;

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.loading = false;
        // Tokens are already stored by AuthService.login() via tap()
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage =
          err.error?.message || 'Login failed. Please check your credentials.';
      },
    });
  }
}
