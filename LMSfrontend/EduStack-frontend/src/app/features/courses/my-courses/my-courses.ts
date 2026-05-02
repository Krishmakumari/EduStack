import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CourseService, CourseResponse } from '../../../services/course.service';
import { AuthService } from '../../../services/auth.service';

import { Navbar } from '../../../core/navbar/navbar';
import { Footer } from '../../../core/footer/footer';

@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [CommonModule, RouterLink, Navbar, Footer],
  templateUrl: './my-courses.html',
  styleUrl: './my-courses.css',
})
export class MyCourses implements OnInit {
  courses: CourseResponse[] = [];
  loading = true;
  errorMessage = '';
  actionMessage = '';

  constructor(
    private courseService: CourseService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (this.auth.isBrowser) {
      this.loadCourses();
    }
  }

  loadCourses() {
    this.loading = true;
    this.courseService.getMyCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Failed to load your courses. Are you logged in as an Instructor?';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  deleteCourse(courseId: string, title: string) {
    if (!confirm(`Delete "${title}" and all its sections/lessons? This cannot be undone.`)) return;
    this.courseService.deleteCourse(courseId).subscribe({
      next: () => {
        this.courses = this.courses.filter(c => c.courseId !== courseId);
        this.showAction('Course deleted successfully.');
      },
      error: (err) => this.showAction(err.error?.message || 'Delete failed.'),
    });
  }

  publishCourse(courseId: string) {
    this.courseService.publishCourse(courseId).subscribe({
      next: () => {
        const c = this.courses.find(x => x.courseId === courseId);
        if (c) c.status = 'Published';
        this.showAction('Course published!');
      },
      error: (err) => this.showAction(err.error?.message || 'Publish failed.'),
    });
  }

  unpublishCourse(courseId: string) {
    this.courseService.unpublishCourse(courseId).subscribe({
      next: () => {
        const c = this.courses.find(x => x.courseId === courseId);
        if (c) c.status = 'Draft';
        this.showAction('Course unpublished.');
      },
      error: (err) => this.showAction(err.error?.message || 'Unpublish failed.'),
    });
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Free' : `₹${price.toLocaleString()}`;
  }

  private showAction(msg: string) {
    this.actionMessage = msg;
    setTimeout(() => (this.actionMessage = ''), 3000);
  }
}
