import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

// ─── Response Interfaces ───────────────────────────────────────────────────

export interface EnrollmentResponse {
  enrollmentId: string;
  studentId: string;
  studentName: string;
  courseId: string;
  courseTitle: string;
  pricePaid: number;
  status: string;
  enrolledAt: string;
  completedAt: string | null;
}

export interface EnrollmentDetailResponse extends EnrollmentResponse {
  lessonProgresses: LessonProgressResponse[];
}

export interface LessonProgressResponse {
  lessonId: string;
  isCompleted: boolean;
  completedAt: string | null;
}

export interface ProgressResponse {
  enrollmentId: string;
  courseId: string;
  totalLessons: number;
  completedLessons: number;
  progressPercentage: number;
  status: string;
  lessonProgresses: LessonProgressResponse[];
}

// ─── Request Interfaces ────────────────────────────────────────────────────

export interface EnrollRequest {
  courseId: string;
  courseTitle: string;
  pricePaid: number;
}

export interface MarkLessonCompleteRequest {
  lessonId: string;
}

// ─── Enrollment Service ────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class EnrollmentService {
  private baseUrl = 'http://localhost:5271/gateway/enrollments';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  /** POST /gateway/enrollments — Enroll student in a course */
  enroll(data: EnrollRequest): Observable<EnrollmentResponse> {
    return this.http.post<EnrollmentResponse>(this.baseUrl, data, {
      headers: this.authHeaders(),
    });
  }

  /** GET /gateway/enrollments/my — Student's own enrollments */
  getMyEnrollments(): Observable<EnrollmentResponse[]> {
    return this.http.get<EnrollmentResponse[]>(`${this.baseUrl}/my`, {
      headers: this.authHeaders(),
    });
  }

  /** GET /gateway/enrollments/{id} — Detailed enrollment with progress */
  getEnrollmentById(id: string): Observable<EnrollmentDetailResponse> {
    return this.http.get<EnrollmentDetailResponse>(`${this.baseUrl}/${id}`, {
      headers: this.authHeaders(),
    });
  }

  /** POST /gateway/enrollments/{id}/complete-lesson — Mark lesson as complete */
  markLessonComplete(enrollmentId: string, lessonId: string): Observable<ProgressResponse> {
    return this.http.post<ProgressResponse>(
      `${this.baseUrl}/${enrollmentId}/complete-lesson`,
      { lessonId },
      { headers: this.authHeaders() }
    );
  }

  /** GET /gateway/enrollments/{id}/progress — Get progress stats */
  getProgress(enrollmentId: string): Observable<ProgressResponse> {
    return this.http.get<ProgressResponse>(`${this.baseUrl}/${enrollmentId}/progress`, {
      headers: this.authHeaders(),
    });
  }

  private authHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  }
}
