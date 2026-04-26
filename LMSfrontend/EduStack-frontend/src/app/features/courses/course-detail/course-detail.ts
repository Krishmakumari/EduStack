import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { CourseService, CourseDetailResponse } from '../../../services/course.service';
import { AuthService } from '../../../services/auth.service';
import { EnrollmentService } from '../../../services/enrollment.service';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './course-detail.html',
  styleUrl: './course-detail.css',
})
export class CourseDetail implements OnInit {
  course: CourseDetailResponse | null = null;
  loading = true;
  errorMessage = '';
  expandedSections: Set<string> = new Set();
  enrolling = false;

  constructor(
    private courseService: CourseService,
    private authService: AuthService,
    private enrollmentService: EnrollmentService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    console.log('CourseDetail: OnInit. Fetching id:', id);
    this.courseService.getCourseById(id).subscribe({
      next: (data) => {
        console.log('CourseDetail: Received data:', data);
        try {
          this.course = data;
          this.loading = false;
          // Expand first section by default
          if (data.sections && data.sections.length > 0) {
            this.expandedSections.add(data.sections[0].sectionId);
          }
          console.log('CourseDetail: Processed data successfully.');
          this.cdr.detectChanges(); // Force UI update
        } catch (e) {
          console.error('CourseDetail: Error in next block:', e);
          this.errorMessage = 'An error occurred while rendering the course.';
          this.loading = false;
          this.cdr.detectChanges(); // Force UI update
        }
      },
      error: (err) => {
        console.error('CourseDetail: API Error:', err);
        this.errorMessage = 'Course not found.';
        this.loading = false;
      },
    });
  }

  toggleSection(sectionId: string) {
    if (this.expandedSections.has(sectionId)) {
      this.expandedSections.delete(sectionId);
    } else {
      this.expandedSections.add(sectionId);
    }
  }

  isSectionExpanded(sectionId: string): boolean {
    return this.expandedSections.has(sectionId);
  }

  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return mins > 0 ? `${mins}m ${secs}s` : `${secs}s`;
  }

  enroll() {
    if (!this.authService.isBrowser) return;

    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    if (!this.course) return;

    this.enrolling = true;
    this.enrollmentService.enroll({
      courseId: this.course.courseId,
      courseTitle: this.course.title,
      pricePaid: this.course.price
    }).subscribe({
      next: (res) => {
        this.router.navigate(['/student/learning', res.enrollmentId]);
      },
      error: (err) => {
        console.error('Enrollment failed:', err);
        alert(err.error?.message || 'Enrollment failed. You might already be enrolled.');
        this.enrolling = false;
        this.cdr.detectChanges();
      }
    });
  }

  get totalLessons(): number {
    try {
      return this.course?.sections?.reduce((sum, s) => sum + (s.lessons?.length || 0), 0) ?? 0;
    } catch (e) {
      console.error('Error in totalLessons:', e);
      return 0;
    }
  }

  get totalDuration(): string {
    try {
      const totalSec = this.course?.sections?.reduce(
        (sum, s) => sum + (s.lessons?.reduce((ls, l) => ls + (l.durationInSeconds || 0), 0) || 0), 0
      ) ?? 0;
      const hours = Math.floor(totalSec / 3600);
      const mins = Math.floor((totalSec % 3600) / 60);
      if (hours > 0) return `${hours}h ${mins}m`;
      return `${mins}m`;
    } catch (e) {
      console.error('Error in totalDuration:', e);
      return '0m';
    }
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Free' : `₹${price.toLocaleString()}`;
  }
}
