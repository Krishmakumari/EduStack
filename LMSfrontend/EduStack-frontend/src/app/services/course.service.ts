import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

// ─── Response Interfaces (match backend DTOs) ──────────────────────────────

export interface CourseResponse {
  courseId: string;
  title: string;
  description: string;
  thumbnailUrl: string;
  price: number;
  level: string;   // Beginner | Intermediate | Advanced
  status: string;  // Draft | Published | Archived
  language: string;
  instructorName: string;
  instructorId: string;
  createdAt: string;
}

export interface CourseDetailResponse {
  courseId: string;
  title: string;
  description: string;
  thumbnailUrl: string;
  price: number;
  level: string;
  status: string;
  language: string;
  instructorName: string;
  instructorId: string;
  createdAt: string;
  sections: SectionResponse[];
}

export interface SectionResponse {
  sectionId: string;
  title: string;
  order: number;
  lessons: LessonResponse[];
}

export interface LessonResponse {
  lessonId: string;
  title: string;
  videoUrl: string | null;
  content: string | null;
  durationInSeconds: number;
  order: number;
  isFreePreview: boolean;
}

export interface MessageResponse {
  message: string;
}

// ─── Request Interfaces (match backend DTOs) ───────────────────────────────

export interface CreateCourseRequest {
  title: string;
  description: string;
  thumbnailUrl: string;
  price: number;
  level: string;
  language: string;
}

export interface UpdateCourseRequest {
  title: string;
  description: string;
  thumbnailUrl: string;
  price: number;
  level: string;
  language: string;
}

export interface AddSectionRequest {
  title: string;
  order: number;
}

export interface UpdateSectionRequest {
  title: string;
  order: number;
}

export interface AddLessonRequest {
  title: string;
  videoUrl?: string | null;
  content?: string | null;
  durationInSeconds: number;
  order: number;
  isFreePreview: boolean;
}

export interface UpdateLessonRequest {
  title: string;
  videoUrl?: string | null;
  content?: string | null;
  durationInSeconds: number;
  order: number;
  isFreePreview: boolean;
}

// ─── Course Service ────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class CourseService {
  /** Gateway URL — Ocelot maps /gateway/courses/* → /api/courses/* on CourseService (port 5248) */
  private baseUrl = 'http://localhost:5271/gateway/courses';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  // ─── Course Read Endpoints (Public) ───────────────────────────────────────

  /** GET /gateway/courses — Public catalog listing */
  getAllCourses(): Observable<CourseResponse[]> {
    return this.http.get<CourseResponse[]>(this.baseUrl);
  }

  /** GET /gateway/courses/{id} — Public course detail with sections + lessons */
  getCourseById(courseId: string): Observable<CourseDetailResponse> {
    return this.http.get<CourseDetailResponse>(`${this.baseUrl}/${courseId}`);
  }

  // ─── Course Write Endpoints (Instructor/Admin — JWT required) ─────────────

  /** GET /gateway/courses/my — Instructor's own courses */
  getMyCourses(): Observable<CourseResponse[]> {
    return this.http.get<CourseResponse[]>(`${this.baseUrl}/my`, {
      headers: this.authHeaders(),
    });
  }

  /** POST /gateway/courses — Create a new course (Draft status) */
  createCourse(data: CreateCourseRequest): Observable<CourseResponse> {
    return this.http.post<CourseResponse>(this.baseUrl, data, {
      headers: this.authHeaders(),
    });
  }

  /** PUT /gateway/courses/{id} — Update course details */
  updateCourse(courseId: string, data: UpdateCourseRequest): Observable<CourseResponse> {
    return this.http.put<CourseResponse>(`${this.baseUrl}/${courseId}`, data, {
      headers: this.authHeaders(),
    });
  }

  /** DELETE /gateway/courses/{id} — Delete course + all sections/lessons */
  deleteCourse(courseId: string): Observable<MessageResponse> {
    return this.http.delete<MessageResponse>(`${this.baseUrl}/${courseId}`, {
      headers: this.authHeaders(),
    });
  }

  /** POST /gateway/courses/{id}/submit — Submit course for admin review */
  submitCourse(courseId: string): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/${courseId}/submit`, {}, {
      headers: this.authHeaders(),
    });
  }

  /** POST /gateway/courses/{id}/unpublish — Revert to Draft */
  unpublishCourse(courseId: string): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.baseUrl}/${courseId}/unpublish`, {}, {
      headers: this.authHeaders(),
    });
  }

  // ─── Section Endpoints (Instructor/Admin) ─────────────────────────────────

  /** POST /gateway/courses/{courseId}/sections — Add section to course */
  addSection(courseId: string, data: AddSectionRequest): Observable<SectionResponse> {
    return this.http.post<SectionResponse>(`${this.baseUrl}/${courseId}/sections`, data, {
      headers: this.authHeaders(),
    });
  }

  /** PUT /gateway/courses/sections/{sectionId} — Update section */
  updateSection(sectionId: string, data: UpdateSectionRequest): Observable<SectionResponse> {
    return this.http.put<SectionResponse>(`${this.baseUrl}/sections/${sectionId}`, data, {
      headers: this.authHeaders(),
    });
  }

  /** DELETE /gateway/courses/sections/{sectionId} — Delete section + lessons */
  deleteSection(sectionId: string): Observable<MessageResponse> {
    return this.http.delete<MessageResponse>(`${this.baseUrl}/sections/${sectionId}`, {
      headers: this.authHeaders(),
    });
  }

  // ─── Lesson Endpoints (Instructor/Admin) ──────────────────────────────────

  /** POST /gateway/courses/sections/{sectionId}/lessons — Add lesson to section */
  addLesson(sectionId: string, data: AddLessonRequest): Observable<LessonResponse> {
    return this.http.post<LessonResponse>(`${this.baseUrl}/sections/${sectionId}/lessons`, data, {
      headers: this.authHeaders(),
    });
  }

  /** PUT /gateway/courses/lessons/{lessonId} — Update lesson */
  updateLesson(lessonId: string, data: UpdateLessonRequest): Observable<LessonResponse> {
    return this.http.put<LessonResponse>(`${this.baseUrl}/lessons/${lessonId}`, data, {
      headers: this.authHeaders(),
    });
  }

  /** DELETE /gateway/courses/lessons/{lessonId} — Delete lesson */
  deleteLesson(lessonId: string): Observable<MessageResponse> {
    return this.http.delete<MessageResponse>(`${this.baseUrl}/lessons/${lessonId}`, {
      headers: this.authHeaders(),
    });
  }

  /** POST /gateway/media/upload — Upload a physical image file */
  uploadThumbnail(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>('http://localhost:5271/gateway/media/upload', formData, {
      headers: this.authHeaders(),
    });
  }

  // ─── Auth Helper ──────────────────────────────────────────────────────────

  private authHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  }
}
