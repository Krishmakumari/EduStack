import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, UserResponse, PaymentResponse, EnrollmentResponse, CourseResponse } from '../../services/admin.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {
  activeTab: 'overview' | 'users' | 'courses' | 'payments' | 'enrollments' = 'overview';
  
  users: UserResponse[] = [];
  courses: CourseResponse[] = [];
  payments: PaymentResponse[] = [];
  enrollments: EnrollmentResponse[] = [];

  loading = false;

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadAllData();
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  loadAllData() {
    this.loading = true;
    this.adminService.getAllUsers().subscribe({
      next: (data) => this.users = data,
      error: () => {},
    });
    this.adminService.getAllCourses().subscribe({
      next: (data) => this.courses = data,
      error: () => {},
    });
    this.adminService.getAllPayments().subscribe({
      next: (data) => this.payments = data,
      error: () => {},
    });
    this.adminService.getAllEnrollments().subscribe({
      next: (data) => { this.enrollments = data; this.loading = false; },
      error: () => { this.loading = false; },
    });
  }

  setTab(tab: 'overview' | 'users' | 'courses' | 'payments' | 'enrollments') {
    this.activeTab = tab;
  }

  loadUsers() {
    this.loading = true;
    this.adminService.getAllUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  loadCourses() {
    this.loading = true;
    this.adminService.getAllCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  loadPayments() {
    this.loading = true;
    this.adminService.getAllPayments().subscribe({
      next: (data) => {
        this.payments = data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  loadEnrollments() {
    this.loading = true;
    this.adminService.getAllEnrollments().subscribe({
      next: (data) => {
        this.enrollments = data;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  // User Actions
  toggleBan(user: UserResponse) {
    const action = user.isBanned ? this.adminService.unbanUser(user.userId) : this.adminService.banUser(user.userId);
    action.subscribe(() => this.loadUsers());
  }

  changeRole(user: UserResponse, newRole: string) {
    this.adminService.updateUserRole(user.userId, newRole).subscribe(() => this.loadUsers());
  }

  // Course Actions
  deleteCourse(courseId: string) {
    if (confirm('Are you sure you want to delete this course? This cannot be undone.')) {
      this.adminService.deleteCourse(courseId).subscribe(() => this.loadCourses());
    }
  }

  approveCourse(courseId: string) {
    this.adminService.approveCourse(courseId).subscribe(() => this.loadCourses());
  }

  rejectCourse(courseId: string) {
    this.adminService.rejectCourse(courseId).subscribe(() => this.loadCourses());
  }

  // Stats Helpers
  getTotalRevenue(): number {
    return this.payments
      .filter(p => p.status === 'Completed')
      .reduce((sum, p) => sum + p.amount, 0);
  }

  getPendingCoursesCount(): number {
    return this.courses.filter(c => c.status === 'PendingApproval').length;
  }

  getInstructorCount(): number {
    return this.users.filter(u => u.role === 'Instructor').length;
  }

  getStudentCount(): number {
    return this.users.filter(u => u.role === 'Student').length;
  }
}
