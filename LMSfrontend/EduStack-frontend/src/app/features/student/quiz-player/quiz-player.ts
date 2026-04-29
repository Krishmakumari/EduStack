import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { QuizService, Quiz, Question } from '../../../services/quiz.service';
import { CertificateService } from '../../../services/certificate.service';
import { AuthService } from '../../../services/auth.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-quiz-player',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './quiz-player.html',
  styleUrl: './quiz-player.css'
})
export class QuizPlayer implements OnInit {
  courseId: string = '';
  quiz: Quiz | null = null;
  loading = true;
  errorMessage = '';

  attemptId: string | null = null;
  quizStarted = false;
  submitting = false;
  
  // Mapping of questionId to selected answer
  answers: { [questionId: string]: string } = {};

  result: any | null = null;
  generatingCertificate = false;
  certificateId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private quizService: QuizService,
    private certificateService: CertificateService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.courseId = this.route.snapshot.paramMap.get('courseId')!;
    if (this.quizService.isBrowser) {
      this.loadQuiz();
    }
  }

  loadQuiz() {
    this.quizService.getQuizByCourse(this.courseId).subscribe({
      next: (q) => {
        this.quiz = q;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        if (err.status === 404) {
          this.errorMessage = 'No quiz available for this course yet.';
        } else {
          this.errorMessage = 'Failed to load quiz details.';
        }
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  startQuiz() {
    if (!this.quiz) return;
    this.loading = true;
    this.quizService.startQuiz(this.quiz.quizId).subscribe({
      next: (res) => {
        this.attemptId = res.attemptId;
        this.quizStarted = true;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = 'Failed to start quiz attempt.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getOptions(question: Question): string[] {
    if (question.options) {
      try {
        return JSON.parse(question.options);
      } catch (e) {
        return [];
      }
    }
    if (question.type === 1) { // Assuming 1 is TrueFalse
      return ['True', 'False'];
    }
    return [];
  }

  submitQuiz() {
    if (!this.attemptId || !this.quiz) return;
    this.submitting = true;

    const answersArray = Object.keys(this.answers).map(qId => ({
      questionId: qId,
      selectedAnswer: this.answers[qId]
    }));

    this.quizService.submitQuiz(this.attemptId, { answers: answersArray }).subscribe({
      next: (res) => {
        this.result = res;
        this.submitting = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = 'Failed to submit quiz.';
        this.submitting = false;
        this.cdr.detectChanges();
      }
    });
  }

  generateCertificate() {
    if (!this.quiz || !this.result || !this.result.passed) return;
    this.generatingCertificate = true;
    this.cdr.detectChanges();

    this.authService.getMe().subscribe({
      next: (user) => {
        const dto = {
          userId: user.userId,
          courseId: this.courseId,
          userName: user.fullName,
          userEmail: user.email,
          courseTitle: this.quiz!.title
        };

        this.certificateService.generateCertificate(dto).subscribe({
          next: (res) => {
            this.certificateId = res.certificateId;
            this.downloadFile(res.certificateId);
            this.generatingCertificate = false;
            this.cdr.detectChanges();
          },
          error: () => {
            this.errorMessage = 'Failed to generate certificate.';
            this.generatingCertificate = false;
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.errorMessage = 'Failed to fetch user profile for certificate.';
        this.generatingCertificate = false;
        this.cdr.detectChanges();
      }
    });
  }

  private downloadFile(id: string) {
    this.certificateService.downloadCertificate(id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Certificate_${this.quiz?.title.replace(/\s+/g, '_')}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.errorMessage = 'Failed to download certificate file.';
        this.cdr.detectChanges();
      }
    });
  }
}
