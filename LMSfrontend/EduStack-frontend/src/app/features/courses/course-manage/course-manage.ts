import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { timeout, retry } from 'rxjs/operators';
import { AuthService } from '../../../services/auth.service';
import {
  CourseService,
  CourseDetailResponse,
  SectionResponse,
  LessonResponse,
  AddSectionRequest,
  AddLessonRequest,
} from '../../../services/course.service';
import { QuizService, Quiz } from '../../../services/quiz.service';

@Component({
  selector: 'app-course-manage',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './course-manage.html',
  styleUrl: './course-manage.css',
})
export class CourseManage implements OnInit {
  courseId = '';
  course: CourseDetailResponse | null = null;
  loading = true;
  errorMessage = '';
  actionMessage = '';

  // Section form
  showSectionForm = false;
  editSectionId: string | null = null;
  sectionTitle = '';
  sectionOrder = 1;

  // Lesson form
  showLessonFormFor: string | null = null; // sectionId
  editLessonId: string | null = null;
  lessonTitle = '';
  lessonVideoUrl = '';
  lessonContent = '';
  lessonDuration = 0;
  lessonOrder = 1;
  lessonFreePreview = false;

  // Quiz Management
  quiz: Quiz | null = null;
  quizLoading = false;
  showQuizForm = false;
  editQuizId: string | null = null;
  quizTitle = '';
  quizPassingScore = 60;

  // Question Form
  showQuestionForm = false;
  editQuestionId: string | null = null;
  qText = '';
  qType = 'MultipleChoice';
  qCorrectAnswer = '';
  qOptions = ''; // JSON or comma separated

  constructor(
    private courseService: CourseService,
    private auth: AuthService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private quizService: QuizService,
  ) {}

  ngOnInit() {
    this.courseId = this.route.snapshot.paramMap.get('id')!;
    if (this.auth.isBrowser) {
      this.loadCourse();
      this.loadQuiz();
    }
  }

  loadCourse() {
    this.loading = true;
    this.errorMessage = '';
    this.courseService.getCourseById(this.courseId)
      .pipe(
        timeout(15000),   // fail fast if backend is unreachable
        retry(1)          // retry once on transient failure
      )
      .subscribe({
        next: (data) => {
          this.course = data;
          this.loading = false;
          this.cdr.detectChanges(); // force UI update
        },
        error: (err) => {
          const isTimeout = err?.name === 'TimeoutError';
          this.errorMessage = isTimeout
            ? 'Request timed out. The server may be slow — please retry.'
            : 'Failed to load course. Please check your connection and retry.';
          this.loading = false;
          this.cdr.detectChanges(); // force UI update
        },
      });
  }

  loadQuiz() {
    this.quizLoading = true;
    this.quizService.getQuizByCourse(this.courseId).subscribe({
      next: (quiz) => {
        this.quiz = quiz;
        this.quizLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        // If 404, it means no quiz exists yet.
        this.quiz = null;
        this.quizLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openAddQuiz() {
    this.editQuizId = null;
    this.showQuizForm = true;
    this.quizTitle = this.course?.title ? `${this.course.title} Quiz` : 'Course Quiz';
    this.quizPassingScore = 60;
  }

  openEditQuiz() {
    if (!this.quiz) return;
    this.editQuizId = this.quiz.quizId;
    this.quizTitle = this.quiz.title;
    this.quizPassingScore = this.quiz.passingScore;
    this.showQuizForm = true;
  }

  cancelQuizForm() {
    this.showQuizForm = false;
    this.editQuizId = null;
  }

  saveQuiz() {
    const dto = {
      courseId: this.courseId,
      title: this.quizTitle,
      passingScore: this.quizPassingScore
    };

    if (this.editQuizId) {
      this.quizService.updateQuiz(this.editQuizId, dto).subscribe({
        next: () => {
          this.showQuizForm = false;
          this.editQuizId = null;
          this.showAction('Quiz updated.');
          this.loadQuiz();
        },
        error: (err) => this.showAction(err.error?.message || 'Failed to update quiz.')
      });
    } else {
      this.quizService.createQuiz(dto).subscribe({
        next: (res) => {
          this.showQuizForm = false;
          this.showAction('Quiz created.');
          this.loadQuiz();
        },
        error: (err) => this.showAction(err.error?.message || 'Failed to create quiz.')
      });
    }
  }

  openAddQuestion() {
    this.editQuestionId = null;
    this.showQuestionForm = true;
    this.qText = '';
    this.qType = 'MultipleChoice';
    this.qCorrectAnswer = '';
    this.qOptions = '';
  }

  openEditQuestion(q: any) {
    this.editQuestionId = q.questionId;
    this.qText = q.text;
    this.qType = this.getQuestionTypeString(q.type);
    this.qCorrectAnswer = q.correctAnswer;
    this.qOptions = q.options || '';
    this.showQuestionForm = true;
  }

  private getQuestionTypeString(typeNum: number): string {
    switch(typeNum) {
      case 0: return 'MultipleChoice';
      case 1: return 'TrueFalse';
      case 2: return 'ShortAnswer';
      default: return 'MultipleChoice';
    }
  }

  cancelQuestionForm() {
    this.showQuestionForm = false;
    this.editQuestionId = null;
  }

  saveQuestion() {
    if (!this.quiz) return;
    
    // Parse options if provided
    let optionsJson: string | undefined;
    if (this.qOptions && this.qOptions.trim() !== '') {
      try {
        if (this.qOptions.trim().startsWith('[')) {
          optionsJson = this.qOptions;
        } else {
          const opts = this.qOptions.split(',').map(s => s.trim()).filter(s => s !== '');
          optionsJson = JSON.stringify(opts);
        }
      } catch {
        optionsJson = this.qOptions;
      }
    }

    const dto = {
      text: this.qText,
      type: this.qType,
      correctAnswer: this.qCorrectAnswer,
      options: optionsJson
    };

    if (this.editQuestionId) {
      this.quizService.updateQuestion(this.editQuestionId, dto).subscribe({
        next: () => {
          this.showQuestionForm = false;
          this.editQuestionId = null;
          this.showAction('Question updated.');
          this.loadQuiz();
        },
        error: (err) => this.showAction(err.error?.message || 'Failed to update question.')
      });
    } else {
      this.quizService.addQuestion(this.quiz.quizId, dto).subscribe({
        next: () => {
          this.showQuestionForm = false;
          this.showAction('Question added.');
          this.loadQuiz();
        },
        error: (err) => this.showAction(err.error?.message || 'Failed to add question.')
      });
    }
  }

  deleteQuestion(questionId: string) {
    if (!confirm('Are you sure you want to delete this question?')) return;
    this.quizService.deleteQuestion(questionId).subscribe({
      next: () => {
        this.showAction('Question deleted.');
        this.loadQuiz();
      },
      error: (err) => this.showAction(err.error?.message || 'Failed to delete question.')
    });
  }

  // ─── Section CRUD ─────────────────────────────────────────────────────────

  openAddSection() {
    this.editSectionId = null;
    this.sectionTitle = '';
    this.sectionOrder = (this.course?.sections.length ?? 0) + 1;
    this.showSectionForm = true;
  }

  openEditSection(section: SectionResponse) {
    this.editSectionId = section.sectionId;
    this.sectionTitle = section.title;
    this.sectionOrder = section.order;
    this.showSectionForm = true;
  }

  cancelSectionForm() {
    this.showSectionForm = false;
    this.editSectionId = null;
  }

  saveSection() {
    const data: AddSectionRequest = { title: this.sectionTitle, order: this.sectionOrder };

    if (this.editSectionId) {
      this.courseService.updateSection(this.editSectionId, data).subscribe({
        next: (updated) => {
          const idx = this.course!.sections.findIndex(s => s.sectionId === this.editSectionId);
          if (idx >= 0) {
            this.course!.sections[idx].title = updated.title;
            this.course!.sections[idx].order = updated.order;
          }
          this.cancelSectionForm();
          this.showAction('Section updated.');
        },
        error: (err) => this.showAction(err.error?.message || 'Update failed.'),
      });
    } else {
      this.courseService.addSection(this.courseId, data).subscribe({
        next: (section) => {
          this.course!.sections.push(section);
          this.cancelSectionForm();
          this.showAction('Section added.');
        },
        error: (err) => this.showAction(err.error?.message || 'Add failed.'),
      });
    }
  }

  deleteSection(sectionId: string) {
    if (!confirm('Delete this section and all its lessons?')) return;
    this.courseService.deleteSection(sectionId).subscribe({
      next: () => {
        this.course!.sections = this.course!.sections.filter(s => s.sectionId !== sectionId);
        this.showAction('Section deleted.');
      },
      error: (err) => this.showAction(err.error?.message || 'Delete failed.'),
    });
  }

  // ─── Lesson CRUD ──────────────────────────────────────────────────────────

  openAddLesson(sectionId: string, lessonCount: number) {
    this.editLessonId = null;
    this.showLessonFormFor = sectionId;
    this.lessonTitle = '';
    this.lessonVideoUrl = '';
    this.lessonContent = '';
    this.lessonDuration = 0;
    this.lessonOrder = lessonCount + 1;
    this.lessonFreePreview = false;
  }

  openEditLesson(sectionId: string, lesson: LessonResponse) {
    this.editLessonId = lesson.lessonId;
    this.showLessonFormFor = sectionId;
    this.lessonTitle = lesson.title;
    this.lessonVideoUrl = lesson.videoUrl || '';
    this.lessonContent = lesson.content || '';
    this.lessonDuration = lesson.durationInSeconds;
    this.lessonOrder = lesson.order;
    this.lessonFreePreview = lesson.isFreePreview;
  }

  cancelLessonForm() {
    this.showLessonFormFor = null;
    this.editLessonId = null;
  }

  saveLesson() {
    const sectionId = this.showLessonFormFor!;
    const data: AddLessonRequest = {
      title: this.lessonTitle,
      videoUrl: this.lessonVideoUrl || null,
      content: this.lessonContent || null,
      durationInSeconds: this.lessonDuration,
      order: this.lessonOrder,
      isFreePreview: this.lessonFreePreview,
    };

    if (this.editLessonId) {
      this.courseService.updateLesson(this.editLessonId, data).subscribe({
        next: (updated) => {
          const section = this.course!.sections.find(s => s.sectionId === sectionId);
          if (section) {
            const idx = section.lessons.findIndex(l => l.lessonId === this.editLessonId);
            if (idx >= 0) section.lessons[idx] = updated;
          }
          this.cancelLessonForm();
          this.showAction('Lesson updated.');
        },
        error: (err) => this.showAction(err.error?.message || 'Update failed.'),
      });
    } else {
      this.courseService.addLesson(sectionId, data).subscribe({
        next: (lesson) => {
          const section = this.course!.sections.find(s => s.sectionId === sectionId);
          if (section) section.lessons.push(lesson);
          this.cancelLessonForm();
          this.showAction('Lesson added.');
        },
        error: (err) => this.showAction(err.error?.message || 'Add failed.'),
      });
    }
  }

  deleteLesson(sectionId: string, lessonId: string) {
    if (!confirm('Delete this lesson?')) return;
    this.courseService.deleteLesson(lessonId).subscribe({
      next: () => {
        const section = this.course!.sections.find(s => s.sectionId === sectionId);
        if (section) section.lessons = section.lessons.filter(l => l.lessonId !== lessonId);
        this.showAction('Lesson deleted.');
      },
      error: (err) => this.showAction(err.error?.message || 'Delete failed.'),
    });
  }

  formatDuration(sec: number): string {
    const m = Math.floor(sec / 60);
    const s = sec % 60;
    return m > 0 ? `${m}m ${s}s` : `${s}s`;
  }

  private showAction(msg: string) {
    this.actionMessage = msg;
    setTimeout(() => (this.actionMessage = ''), 3000);
  }
}
