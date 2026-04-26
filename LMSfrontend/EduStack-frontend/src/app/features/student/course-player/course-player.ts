import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EnrollmentService, EnrollmentDetailResponse, ProgressResponse } from '../../../services/enrollment.service';
import { CourseService, CourseDetailResponse, LessonResponse } from '../../../services/course.service';
import { forkJoin } from 'rxjs';
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

  constructor(
    private route: ActivatedRoute,
    private enrollmentService: EnrollmentService,
    private courseService: CourseService,
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
    for (const section of this.course.sections) {
      for (const lesson of section.lessons) {
        if (!this.isLessonCompleted(lesson.lessonId)) {
          this.activeLesson = lesson;
          return;
        }
      }
    }

    // If all completed, just set first lesson
    this.activeLesson = this.course.sections[0].lessons[0];
  }

  selectLesson(lesson: LessonResponse) {
    this.activeLesson = lesson;
    this.cdr.detectChanges();
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
