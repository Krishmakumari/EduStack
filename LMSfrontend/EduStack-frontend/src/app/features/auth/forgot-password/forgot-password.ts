import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CommonModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  forgotForm: FormGroup;
  errorMessage = '';
  successMessage = '';
  loading = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
  ) {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  get f() { return this.forgotForm.controls; }

  onSubmit() {
    this.submitted = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.forgotForm.invalid) return;

    this.loading = true;

    this.auth.forgotPassword({ email: this.forgotForm.value.email }).subscribe({
      next: (res) => {
        this.loading = false;
        this.successMessage = res.message;
        // Navigate to reset-password page after a short delay, pre-filling the email
        setTimeout(() => {
          this.router.navigate(['/auth/reset-password'], {
            queryParams: { email: this.forgotForm.value.email },
          });
        }, 2000);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage =
          err.error?.message || 'Something went wrong. Please try again.';
      },
    });
  }
}
