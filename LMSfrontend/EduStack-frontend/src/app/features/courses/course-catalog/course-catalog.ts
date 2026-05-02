import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CourseService, CourseResponse } from '../../../services/course.service';
import { EnrollmentService } from '../../../services/enrollment.service';
import { AuthService } from '../../../services/auth.service';
import { Navbar } from '../../../core/navbar/navbar';

import { Footer } from '../../../core/footer/footer';

@Component({
  selector: 'app-course-catalog',
  standalone: true,
  imports: [CommonModule, RouterLink, Navbar, Footer],
  templateUrl: './course-catalog.html',
  styleUrl: './course-catalog.css',
})
export class CourseCatalog implements OnInit {
  courses: CourseResponse[] = [];
  loading = true;
  errorMessage = '';
  enrolledCourseIds = new Set<string>();

  constructor(
    private courseService: CourseService,
    private enrollmentService: EnrollmentService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit() {
    if (this.authService.isBrowser) {
      const role = this.authService.getUserRole();
      if (role === 'Admin') {
        this.router.navigate(['/admin/dashboard']);
        return;
      }
    }
    this.courseService.getAllCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Failed to load courses.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });

    // Load enrolled course IDs for the logged-in student
    if (this.authService.isLoggedIn()) {
      this.enrollmentService.getMyEnrollments().subscribe({
        next: (enrollments) => {
          enrollments.forEach(e => this.enrolledCourseIds.add(e.courseId));
          this.cdr.detectChanges();
        },
        error: () => { /* ignore — user might not be a student */ }
      });
    }
  }

  isEnrolled(courseId: string): boolean {
    return this.enrolledCourseIds.has(courseId);
  }

  getLevelClass(level: string): string {
    return `level-${level.toLowerCase()}`;
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Free' : `₹${price.toLocaleString()}`;
  }
}
