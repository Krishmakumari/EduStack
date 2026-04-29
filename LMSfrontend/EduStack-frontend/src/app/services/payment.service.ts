import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

// ─── Response Interfaces (match backend DTOs) ──────────────────────────────

export interface PaymentResponse {
  paymentId: string;
  studentId: string;
  studentName: string;
  courseId: string;
  courseTitle: string;
  amount: number;
  status: string;        // "Pending" | "Completed" | "Failed" | "Refunded"
  method: string;        // "UPI" | "CreditCard" | "DebitCard" | "NetBanking" | "Wallet"
  transactionId: string | null;
  failureReason: string | null;
  createdAt: string;
  completedAt: string | null;
  refundedAt: string | null;
}

export interface RefundResponse {
  refundId: string;
  paymentId: string;
  amount: number;
  reason: string;
  transactionId: string | null;
  createdAt: string;
}

// ─── Request Interfaces (match backend DTOs) ───────────────────────────────

export interface InitiatePaymentRequest {
  courseId: string;
  courseTitle: string;
  amount: number;
  method: string;   // "UPI" | "CreditCard" | "DebitCard" | "NetBanking" | "Wallet"
}

export interface ConfirmPaymentRequest {
  transactionId: string;
}

export interface FailPaymentRequest {
  reason: string;
}

export interface RefundPaymentRequest {
  reason: string;
}

// ─── Payment Service ───────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class PaymentService {
  /** Gateway URL — Ocelot maps /gateway/payments/* → /api/payments/* on PaymentService (port 5117) */
  private baseUrl = 'http://localhost:5271/gateway/payments';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  // ─── Lifecycle Endpoints ────────────────────────────────────────────────

  /** POST /gateway/payments/initiate — Start a new payment (Pending status) */
  initiatePayment(data: InitiatePaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(`${this.baseUrl}/initiate`, data, {
      headers: this.authHeaders(),
    });
  }

  /** POST /gateway/payments/{id}/confirm — Mark payment as Completed */
  confirmPayment(paymentId: string, data: ConfirmPaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(`${this.baseUrl}/${paymentId}/confirm`, data, {
      headers: this.authHeaders(),
    });
  }

  /** POST /gateway/payments/{id}/fail — Mark payment as Failed */
  failPayment(paymentId: string, data: FailPaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(`${this.baseUrl}/${paymentId}/fail`, data, {
      headers: this.authHeaders(),
    });
  }

  /** POST /gateway/payments/{id}/refund — Request a refund */
  refundPayment(paymentId: string, data: RefundPaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(`${this.baseUrl}/${paymentId}/refund`, data, {
      headers: this.authHeaders(),
    });
  }

  // ─── Query Endpoints ────────────────────────────────────────────────────

  /** GET /gateway/payments/my — Student's full purchase history */
  getMyPayments(): Observable<PaymentResponse[]> {
    return this.http.get<PaymentResponse[]>(`${this.baseUrl}/my`, {
      headers: this.authHeaders(),
    });
  }

  /** GET /gateway/payments/{id} — Single payment detail */
  getPaymentById(paymentId: string): Observable<PaymentResponse> {
    return this.http.get<PaymentResponse>(`${this.baseUrl}/${paymentId}`, {
      headers: this.authHeaders(),
    });
  }

  /** GET /gateway/payments/course/{courseId} — All payments for a course (Admin/Instructor) */
  getPaymentsByCourse(courseId: string): Observable<PaymentResponse[]> {
    return this.http.get<PaymentResponse[]>(`${this.baseUrl}/course/${courseId}`, {
      headers: this.authHeaders(),
    });
  }

  // ─── Auth Helper ──────────────────────────────────────────────────────

  private authHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  }
}
