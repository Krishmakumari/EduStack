import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CourseService, CourseResponse } from '../../../services/course.service';

@Component({
  selector: 'app-course-catalog',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './course-catalog.html',
  styleUrl: './course-catalog.css',
})
export class CourseCatalog implements OnInit {
  courses: CourseResponse[] = [];
  loading = true;
  errorMessage = '';

  constructor(
    private courseService: CourseService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.courseService.getAllCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.loading = false;
        this.cdr.detectChanges(); // Force UI update
      },
      error: () => {
        this.errorMessage = 'Failed to load courses.';
        this.loading = false;
        this.cdr.detectChanges(); // Force UI update
      },
    });
  }

  getLevelClass(level: string): string {
    return `level-${level.toLowerCase()}`;
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Free' : `₹${price.toLocaleString()}`;
  }
}
