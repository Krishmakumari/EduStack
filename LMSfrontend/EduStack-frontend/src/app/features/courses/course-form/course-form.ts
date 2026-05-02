import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { timeout, retry } from 'rxjs/operators';
import { CourseService, CreateCourseRequest } from '../../../services/course.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-course-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './course-form.html',
  styleUrl: './course-form.css',
})
export class CourseForm implements OnInit {
  courseForm: FormGroup;
  isEditMode = false;
  courseId = '';
  loading = false;
  pageLoading = true;
  errorMessage = '';
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private courseService: CourseService,
    private auth: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {
    this.courseForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(2000)]],
      thumbnailUrl: ['', [Validators.required]],
      price: [0, [Validators.required, Validators.min(0), Validators.max(50000)]],
      level: ['Beginner', [Validators.required]],
      language: ['English', [Validators.required, Validators.minLength(2)]],
    });
  }

  get f() { return this.courseForm.controls; }

  // Expose thumbnail URL for preview
  get thumbnailUrl(): string { return this.courseForm.value.thumbnailUrl; }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.loading = true;
      this.courseService.uploadThumbnail(file).subscribe({
        next: (res) => {
          this.courseForm.patchValue({ thumbnailUrl: res.url });
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = 'Failed to upload image. Please try again.';
          this.cdr.detectChanges();
        }
      });
    }
  }

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
            this.courseForm.patchValue({
              title: course.title,
              description: course.description,
              thumbnailUrl: course.thumbnailUrl,
              price: course.price,
              level: course.level,
              language: course.language,
            });
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
    this.submitted = true;
    this.errorMessage = '';

    if (this.courseForm.invalid) return;

    this.loading = true;

    const data: CreateCourseRequest = this.courseForm.value;

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
