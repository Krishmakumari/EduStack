import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { timeout, retry } from 'rxjs/operators';
import { CourseService, CreateCourseRequest } from '../../../services/course.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-course-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './course-form.html',
  styleUrl: './course-form.css',
})
export class CourseForm implements OnInit {
  isEditMode = false;
  courseId = '';
  loading = false;
  pageLoading = true;
  errorMessage = '';

  title = '';
  description = '';
  thumbnailUrl = '';
  price = 0;
  level = 'Beginner';
  language = 'English';

  constructor(
    private courseService: CourseService,
    private auth: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.courseId = this.route.snapshot.paramMap.get('id') || '';
    this.isEditMode = !!this.courseId;

    if (this.isEditMode && this.auth.isBrowser) {
      this.courseService.getCourseById(this.courseId)
        .pipe(
          timeout(15000),  // fail fast if backend is unreachable
          retry(1)         // retry once on transient failure
        )
        .subscribe({
          next: (course) => {
            this.title = course.title;
            this.description = course.description;
            this.thumbnailUrl = course.thumbnailUrl;
            this.price = course.price;
            this.level = course.level;
            this.language = course.language;
            this.pageLoading = false;
            this.cdr.detectChanges(); // force UI update
          },
          error: (err) => {
            const isTimeout = err?.name === 'TimeoutError';
            this.errorMessage = isTimeout
              ? 'Request timed out. Please retry.'
              : 'Failed to load course data.';
            this.pageLoading = false;
            this.cdr.detectChanges(); // force UI update
          },
        });
    } else {
      this.pageLoading = false;
    }
  }

  onSubmit() {
    this.errorMessage = '';
    this.loading = true;

    const data: CreateCourseRequest = {
      title: this.title,
      description: this.description,
      thumbnailUrl: this.thumbnailUrl,
      price: this.price,
      level: this.level,
      language: this.language,
    };

    const obs$ = this.isEditMode
      ? this.courseService.updateCourse(this.courseId, data)
      : this.courseService.createCourse(data);

    obs$.subscribe({
      next: (result) => {
        this.loading = false;
        if (this.isEditMode) {
          this.router.navigate(['/instructor/my-courses']);
        } else {
          // After creating, go to manage page to add sections & lessons
          this.router.navigate(['/instructor/courses/manage', result.courseId]);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to save course.';
      },
    });
  }
}
