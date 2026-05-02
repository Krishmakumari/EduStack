import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EnrollmentService, EnrollmentResponse, ProgressResponse } from '../../../services/enrollment.service';
import { AuthService } from '../../../services/auth.service';
import { Navbar } from '../../../core/navbar/navbar';

import { Footer } from '../../../core/footer/footer';

@Component({
  selector: 'app-my-learning',
  standalone: true,
  imports: [CommonModule, RouterLink, Navbar, Footer],
  templateUrl: './my-learning.html',
  styleUrl: './my-learning.css',
})
export class MyLearning implements OnInit {
  enrollments: EnrollmentResponse[] = [];
  progressMap: Map<string, ProgressResponse> = new Map();
  loading = true;
  errorMessage = '';

  constructor(
    private enrollmentService: EnrollmentService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (this.auth.isBrowser) {
      this.loadEnrollments();
    }
  }

  loadEnrollments() {
    this.enrollmentService.getMyEnrollments().subscribe({
      next: (data) => {
        this.enrollments = data;
        this.loading = false;
        this.cdr.detectChanges();
        // Fetch progress for each enrollment
        data.forEach(enrollment => this.loadProgress(enrollment.enrollmentId));
      },
      error: (err) => {
        console.error('Failed to load enrollments:', err);
        this.errorMessage = 'Failed to load your learning dashboard.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private loadProgress(enrollmentId: string) {
    this.enrollmentService.getProgress(enrollmentId).subscribe({
      next: (progress) => {
        this.progressMap.set(enrollmentId, progress);
        this.cdr.detectChanges();
      },
      error: () => { /* silently ignore — progress just won't show */ }
    });
  }

  getProgress(enrollmentId: string): ProgressResponse | undefined {
    return this.progressMap.get(enrollmentId);
  }

  getStatusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }
}
