import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface Question {
  questionId: string;
  quizId: string;
  text: string;
  type: number; // enum, e.g. 0: MultipleChoice, 1: TrueFalse, etc.
  options?: string; // JSON string
}

export interface Quiz {
  quizId: string;
  courseId: string;
  title: string;
  passingScore: number;
  questions: Question[];
}

export interface StartQuizResponse {
  attemptId: string;  // mapped from backend's AttemptId (PascalCase)
}

export interface AnswerDto {
  questionId: string;
  selectedAnswer: string;
}

export interface SubmitQuizDto {
  answers: AnswerDto[];
}

export interface QuizResultDto {
  attemptId: string;
  score: number;
  passed: boolean;
  correctAnswers: number;
  totalQuestions: number;
}

export interface CreateQuizDto {
  courseId: string;
  title: string;
  passingScore: number;
}

export interface CreateQuestionDto {
  text: string;
  type: string;
  correctAnswer: string;
  options?: string;
}

@Injectable({
  providedIn: 'root'
})
export class QuizService {
  /** Gateway URL — Ocelot maps /gateway/quizzes/* → /api/quizzes/* on QuizService (port 5021) */
  private baseUrl = 'http://localhost:5271/gateway/quizzes';

  constructor(
    private http: HttpClient,
    private authService: AuthService,
  ) { }
  
  get isBrowser(): boolean {
    return this.authService.isBrowser;
  }

  // Get a quiz by ID
  getQuiz(quizId: string): Observable<Quiz> {
    return this.http.get<Quiz>(`${this.baseUrl}/${quizId}`, {
      headers: this.authHeaders(),
    });
  }

  // Get a quiz by Course ID
  getQuizByCourse(courseId: string): Observable<Quiz> {
    return this.http.get<Quiz>(`${this.baseUrl}/course/${courseId}?t=${new Date().getTime()}`, {
      headers: this.authHeaders(),
    });
  }

  // Start a quiz attempt — backend returns { AttemptId }, mapped via JSON camelCase to { attemptId }
  startQuiz(quizId: string): Observable<StartQuizResponse> {
    return this.http.post<StartQuizResponse>(`${this.baseUrl}/${quizId}/start`, {}, {
      headers: this.authHeaders(),
    });
  }

  // Submit a quiz attempt
  submitQuiz(attemptId: string, dto: SubmitQuizDto): Observable<QuizResultDto> {
    return this.http.post<QuizResultDto>(`${this.baseUrl}/attempt/${attemptId}/submit`, dto, {
      headers: this.authHeaders(),
    });
  }

  // Create a new quiz (Admin/Instructor)
  createQuiz(dto: CreateQuizDto): Observable<{ quizId: string }> {
    return this.http.post<{ quizId: string }>(this.baseUrl, dto, {
      headers: this.authHeaders(),
    });
  }

  // Add a question to a quiz (Admin/Instructor)
  addQuestion(quizId: string, dto: CreateQuestionDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/${quizId}/questions`, dto, {
      headers: this.authHeaders(),
    });
  }

  // Update a quiz (Admin/Instructor)
  updateQuiz(quizId: string, dto: CreateQuizDto): Observable<any> {
    return this.http.put(`${this.baseUrl}/${quizId}`, dto, {
      headers: this.authHeaders(),
    });
  }

  // Update a question (Admin/Instructor)
  updateQuestion(questionId: string, dto: CreateQuestionDto): Observable<any> {
    return this.http.put(`${this.baseUrl}/questions/${questionId}`, dto, {
      headers: this.authHeaders(),
    });
  }

  // Delete a question (Admin/Instructor)
  deleteQuestion(questionId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/questions/${questionId}`, {
      headers: this.authHeaders(),
    });
  }

  private authHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }
}
