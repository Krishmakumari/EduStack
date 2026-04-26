import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap } from 'rxjs';

// ─── Response interfaces matching backend DTOs ─────────────────────────────

/** Matches AuthService.Application.DTOs.Responses.AuthResponse */
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  fullName: string;
  email: string;
  role: string;
}

/** Matches AuthService.Application.DTOs.Responses.MessageResponse */
export interface MessageResponse {
  message: string;
}

/** Matches GET /api/auth/me response */
export interface UserProfile {
  userId: string;
  email: string;
  role: string;
  fullName: string;
}

// ─── Request interfaces matching backend DTOs ──────────────────────────────

/** Matches AuthService.Application.DTOs.Requests.RegisterRequest */
export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role?: string; // defaults to "Student" on backend
}

/** Matches AuthService.Application.DTOs.Requests.LoginRequest */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Matches AuthService.Application.DTOs.Requests.RefreshTokenRequest */
export interface RefreshTokenRequest {
  refreshToken: string;
}

/** Matches AuthService.Application.DTOs.Requests.ForgotPasswordRequest */
export interface ForgotPasswordRequest {
  email: string;
}

/** Matches AuthService.Application.DTOs.Requests.ResetPasswordRequest */
export interface ResetPasswordRequest {
  email: string;
  otpCode: string;
  newPassword: string;
}

// ─── Auth Service ──────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class AuthService {
  /** Gateway URL — Ocelot maps /gateway/auth/* → /api/auth/* on AuthService (port 5124) */
  private baseUrl = 'http://127.0.0.1:5271/gateway/auth';

  public isBrowser: boolean;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: Object,
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  // ─── POST /gateway/auth/register ──────────────────────────────────────────
  register(data: RegisterRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/register`, data);
  }

  // ─── POST /gateway/auth/login ─────────────────────────────────────────────
  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, data).pipe(
      tap(res => this.storeTokens(res))
    );
  }

  // ─── POST /gateway/auth/refresh-token ─────────────────────────────────────
  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/refresh-token`, { refreshToken } as RefreshTokenRequest)
      .pipe(tap(res => this.storeTokens(res)));
  }

  // ─── POST /gateway/auth/forgot-password ───────────────────────────────────
  forgotPassword(data: ForgotPasswordRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/forgot-password`, data);
  }

  // ─── POST /gateway/auth/reset-password ────────────────────────────────────
  resetPassword(data: ResetPasswordRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/reset-password`, data);
  }

  // ─── GET /gateway/auth/verify-email?token=xxx ─────────────────────────────
  verifyEmail(token: string): Observable<MessageResponse> {
    return this.http.get<MessageResponse>(`${this.baseUrl}/verify-email`, {
      params: { token },
    });
  }

  // ─── GET /gateway/auth/me (requires JWT) ──────────────────────────────────
  getMe(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/me`, {
      headers: this.authHeaders(),
    });
  }

  // ─── Token helpers ────────────────────────────────────────────────────────

  /** Persist both tokens + user info to localStorage after login/refresh */
  private storeTokens(res: AuthResponse): void {
    if (!this.isBrowser) return;
    localStorage.setItem('accessToken', res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
    localStorage.setItem('userFullName', res.fullName);
    localStorage.setItem('userEmail', res.email);
    localStorage.setItem('userRole', res.role);
  }

  getAccessToken(): string | null {
    return this.isBrowser ? localStorage.getItem('accessToken') : null;
  }

  getRefreshToken(): string | null {
    return this.isBrowser ? localStorage.getItem('refreshToken') : null;
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  logout(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userFullName');
    localStorage.removeItem('userEmail');
    localStorage.removeItem('userRole');
  }

  /** Build Authorization header for protected endpoints */
  private authHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.getAccessToken()}`,
    });
  }
}
