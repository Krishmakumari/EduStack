import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  loginForm: FormGroup;
  errorMessage = '';
  successMessage = '';
  loading = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

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

  // Getter shortcuts for template validation access
  get f() { return this.loginForm.controls; }

  onLogin() {
    this.submitted = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.loginForm.invalid) return;

    this.loading = true;

    this.auth.login({
      email: this.loginForm.value.email,
      password: this.loginForm.value.password,
    }).subscribe({
      next: (res) => {
        this.loading = false;
        // Tokens are already stored by AuthService.login() via tap()

        if (res.role === 'Admin') {
          this.router.navigate(['/admin/dashboard']);
        } else if (res.role === 'Instructor') {
          this.router.navigate(['/']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage =
          err.error?.message || 'Login failed. Please check your credentials.';
      },
    });
  }
}
