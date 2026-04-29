import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

// ─── Interfaces ────────────────────────────────────────────────────────────

export interface UpdateProgressRequest {
  courseId: string;
  lessonId: string;
  watchedSeconds: number;
  totalDurationSeconds: number;
}

export interface LessonProgressResponse {
  lessonId: string;
  watchedSeconds: number;
  isCompleted: boolean;
  progressPercentage: number;
}

export interface CourseProgressResponse {
  courseId: string;
  totalLessons: number;
  completedLessons: number;
  completionPercentage: number;
}

@Injectable({ providedIn: 'root' })
export class LearningService {
  private baseUrl = 'http://localhost:5271/gateway/learning';

  constructor(
    private http: HttpClient,
    private auth: AuthService
  ) {}

  /** POST /gateway/learning/progress — Update fine-grained watch progress */
  updateProgress(data: UpdateProgressRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/progress`, data, {
      headers: this.authHeaders(),
    });
  }

  /** GET /gateway/learning/courses/{cId}/lessons/{lId}/progress — Get status for one lesson */
  getLessonProgress(courseId: string, lessonId: string): Observable<LessonProgressResponse> {
    return this.http.get<LessonProgressResponse>(
      `${this.baseUrl}/courses/${courseId}/lessons/${lessonId}/progress`,
      { headers: this.authHeaders() }
    );
  }

  /** GET /gateway/learning/courses/{cId}/progress — Get overall course progress */
  getCourseProgress(courseId: string): Observable<CourseProgressResponse> {
    return this.http.get<CourseProgressResponse>(
      `${this.baseUrl}/courses/${courseId}/progress`,
      { headers: this.authHeaders() }
    );
  }

  private authHeaders(): HttpHeaders {
    const token = this.auth.getAccessToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  }
}
