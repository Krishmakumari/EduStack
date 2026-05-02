import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const instructorGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isBrowser) return true;

  if (authService.getUserRole() === 'Instructor') {
    return true;
  }

  // If Admin tries to access instructor pages, they are redirected back to Admin Dashboard
  if (authService.getUserRole() === 'Admin') {
    router.navigate(['/admin/dashboard']);
    return false;
  }

  router.navigate(['/auth/login']);
  return false;
};
