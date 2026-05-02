import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isBrowser) return true;

  if (authService.getUserRole() === 'Admin') {
    return true;
  }

  router.navigate(['/']);
  return false;
};
