import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EnrollmentService, EnrollmentDetailResponse, ProgressResponse } from '../../../services/enrollment.service';
import { CourseService, CourseDetailResponse, LessonResponse } from '../../../services/course.service';
import { LearningService, LessonProgressResponse } from '../../../services/learning.service';
import { forkJoin, interval, Subscription } from 'rxjs';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-course-player',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './course-player.html',
  styleUrl: './course-player.css',
})
export class CoursePlayer implements OnInit {
  enrollmentId: string = '';
  enrollment: EnrollmentDetailResponse | null = null;
  course: CourseDetailResponse | null = null;
  activeLesson: LessonResponse | null = null;
  
  loading = true;
  completing = false;
  errorMessage = '';
  
  private progressSub: Subscription | null = null;
  currentWatchedSeconds = 0;

  constructor(
    private route: ActivatedRoute,
    private enrollmentService: EnrollmentService,
    private courseService: CourseService,
    private learningService: LearningService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.enrollmentId = this.route.snapshot.paramMap.get('enrollmentId')!;
    if (this.auth.isBrowser) {
      this.loadData();
    }
  }

  loadData() {
    this.enrollmentService.getEnrollmentById(this.enrollmentId).subscribe({
      next: (enrollRes) => {
        this.enrollment = enrollRes;
        
        // Now fetch the actual course content
        this.courseService.getCourseById(enrollRes.courseId).subscribe({
          next: (courseRes) => {
            this.course = courseRes;
            
            // Set initial active lesson (first lesson of first section, or first incomplete lesson)
            this.setInitialLesson();
            
            this.loading = false;
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('Failed to load course content:', err);
            this.errorMessage = 'Failed to load course details.';
            this.loading = false;
            this.cdr.detectChanges();
          }
        });
      },
      error: (err) => {
        console.error('Failed to load enrollment:', err);
        this.errorMessage = 'Failed to load enrollment details.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  setInitialLesson() {
    if (!this.course || !this.course.sections.length) return;
    
    // Find first incomplete lesson
    let lessonToSet: LessonResponse | null = null;
    for (const section of this.course.sections) {
      if (section.lessons && section.lessons.length > 0) {
        for (const lesson of section.lessons) {
          if (!this.isLessonCompleted(lesson.lessonId)) {
            lessonToSet = lesson;
            break;
          }
        }
      }
      if (lessonToSet) break;
    }

    // If all completed or none found with lessons, just set first lesson of first section if possible
    if (!lessonToSet && this.course.sections[0].lessons?.length > 0) {
      lessonToSet = this.course.sections[0].lessons[0];
    }

    if (lessonToSet) {
      this.selectLesson(lessonToSet);
    }
  }

  selectLesson(lesson: LessonResponse) {
    if (this.activeLesson?.lessonId === lesson.lessonId) return;
    
    // Save current progress before switching
    this.saveProgress();
    
    this.activeLesson = lesson;
    this.currentWatchedSeconds = 0;
    
    // Fetch detailed progress for this lesson to resume
    if (this.auth.isBrowser) {
      this.learningService.getLessonProgress(this.course!.courseId, lesson.lessonId).subscribe({
        next: (prog) => {
          this.currentWatchedSeconds = prog.watchedSeconds;
          this.startProgressTracking();
          this.cdr.detectChanges();
        },
        error: () => {
          // If no progress found, just start tracking from 0
          this.startProgressTracking();
          this.cdr.detectChanges();
        }
      });
    }
  }

  private startProgressTracking() {
    if (this.progressSub) this.progressSub.unsubscribe();
    
    // Simulate video playing and update progress every 10 seconds
    this.progressSub = interval(10000).subscribe(() => {
      if (this.activeLesson) {
        this.currentWatchedSeconds += 10;
        this.saveProgress();
      }
    });
  }

  private saveProgress() {
    if (!this.activeLesson || !this.course || !this.auth.isBrowser) return;

    this.learningService.updateProgress({
      courseId: this.course.courseId,
      lessonId: this.activeLesson.lessonId,
      watchedSeconds: this.currentWatchedSeconds,
      totalDurationSeconds: this.activeLesson.durationInSeconds || 600 // fallback if duration missing
    }).subscribe({
      next: () => {
        // After updating fine-grained progress, check if course-wide progress needs refresh
        // (IsCompleted is calculated by the learning service)
        this.refreshEnrollmentData();
      }
    });
  }

  private refreshEnrollmentData() {
    this.enrollmentService.getEnrollmentById(this.enrollmentId).subscribe(res => {
      this.enrollment = res;
      this.cdr.detectChanges();
    });
  }

  isLessonCompleted(lessonId: string): boolean {
    return this.enrollment?.lessonProgresses.some(p => p.lessonId === lessonId && p.isCompleted) ?? false;
  }

  markComplete() {
    if (!this.activeLesson || this.completing) return;
    if (this.isLessonCompleted(this.activeLesson.lessonId)) return;

    this.completing = true;
    this.enrollmentService.markLessonComplete(this.enrollmentId, this.activeLesson.lessonId).subscribe({
      next: (progress: ProgressResponse) => {
        // Update enrollment with new lesson progress list
        if (this.enrollment) {
          this.enrollment.lessonProgresses = progress.lessonProgresses;
          this.enrollment.status = progress.status;
        }
        this.completing = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to mark complete:', err);
        this.completing = false;
        this.cdr.detectChanges();
      }
    });
  }

  get progressPercentage(): number {
    if (!this.course || !this.enrollment) return 0;
    const total = this.course.sections.reduce((sum, s) => sum + s.lessons.length, 0);
    if (total === 0) return 0;
    const completed = this.enrollment.lessonProgresses.filter(p => p.isCompleted).length;
    return Math.round((completed / total) * 100);
  }
}
