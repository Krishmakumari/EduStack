import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface UserResponse {
  userId: string;
  fullName: string;
  email: string;
  role: string;
  isBanned: boolean;
  createdAt: string;
}

export interface PaymentResponse {
  paymentId: string;
  studentId: string;
  studentName: string;
  courseId: string;
  courseTitle: string;
  amount: number;
  status: string;
  method: string;
  transactionId: string;
  createdAt: string;
}

export interface EnrollmentResponse {
  enrollmentId: string;
  studentId: string;
  studentName: string;
  courseId: string;
  courseTitle: string;
  status: string;
  enrolledAt: string;
}

export interface CourseResponse {
  courseId: string;
  title: string;
  instructorName: string;
  price: number;
  status: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private baseUrl = 'http://127.0.0.1:5271/gateway/admin';

  constructor(
    private http: HttpClient,
    private authService: AuthService,
  ) {}

  // ─── User Management (AuthService → port 5124) ────────────────────────────

  getAllUsers(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(`${this.baseUrl}/users`, {
      headers: this.authHeaders(),
    });
  }

  banUser(userId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/users/${userId}/ban`, {}, {
      headers: this.authHeaders(),
    });
  }

  unbanUser(userId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/users/${userId}/unban`, {}, {
      headers: this.authHeaders(),
    });
  }

  updateUserRole(userId: string, role: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/users/${userId}/role`, JSON.stringify(role), {
      headers: this.authHeaders().set('Content-Type', 'application/json'),
    });
  }

  // ─── Payment Reporting (PaymentService → port 5117) ──────────────────────

  getAllPayments(): Observable<PaymentResponse[]> {
    return this.http.get<PaymentResponse[]>(`${this.baseUrl}/payments`, {
      headers: this.authHeaders(),
    });
  }

  // ─── Enrollment Monitoring (EnrollmentService → port 5249) ───────────────

  getAllEnrollments(): Observable<EnrollmentResponse[]> {
    return this.http.get<EnrollmentResponse[]>(`${this.baseUrl}/enrollments`, {
      headers: this.authHeaders(),
    });
  }

  // ─── Course Management (CourseService → port 5248) ───────────────────────

  getAllCourses(): Observable<CourseResponse[]> {
    return this.http.get<CourseResponse[]>(`${this.baseUrl}/courses`, {
      headers: this.authHeaders(),
    });
  }

  deleteCourse(courseId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/courses/${courseId}`, {
      headers: this.authHeaders(),
    });
  }

  approveCourse(courseId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/courses/${courseId}/approve`, {}, {
      headers: this.authHeaders(),
    });
  }

  rejectCourse(courseId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/courses/${courseId}/reject`, {}, {
      headers: this.authHeaders(),
    });
  }

  // ─── Auth Helper ──────────────────────────────────────────────────────────

  private authHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }
}
