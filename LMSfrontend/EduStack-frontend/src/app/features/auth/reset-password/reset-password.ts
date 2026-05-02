import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CommonModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
})
export class ResetPassword implements OnInit {
  resetForm: FormGroup;
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
    this.resetForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      otpCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6), Validators.pattern(/^\d{6}$/)]],
      newPassword: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/),
      ]],
      confirmPassword: ['', [Validators.required]],
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  ngOnInit() {
    // Pre-fill email if passed from forgot-password page
    this.route.queryParams.subscribe((params) => {
      if (params['email']) {
        this.resetForm.patchValue({ email: params['email'] });
      }
    });
  }

  get f() { return this.resetForm.controls; }

  get passwordMismatch(): boolean {
    return this.resetForm.hasError('passwordMismatch') &&
      this.f['confirmPassword'].dirty;
  }

  onSubmit() {
    this.submitted = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.resetForm.invalid) return;

    this.loading = true;

    this.auth
      .resetPassword({
        email: this.resetForm.value.email,
        otpCode: this.resetForm.value.otpCode,
        newPassword: this.resetForm.value.newPassword,
      })
      .subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/auth/login'], {
            queryParams: { passwordReset: 'true' },
          });
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage =
            err.error?.message || 'Password reset failed. Please try again.';
        },
      });
  }
}
