import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { CourseService, CourseDetailResponse } from '../../../services/course.service';
import { PaymentService, PaymentResponse } from '../../../services/payment.service';
import { EnrollmentService } from '../../../services/enrollment.service';
import { Navbar } from '../../../core/navbar/navbar';

import { Footer } from '../../../core/footer/footer';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, Navbar, Footer],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class Checkout implements OnInit {
  courseId = '';
  course: CourseDetailResponse | null = null;
  loading = true;
  errorMessage = '';

  // Payment form
  selectedMethod = 'UPI';
  paymentMethods = [
    { value: 'UPI', label: 'UPI', icon: '📱', desc: 'Google Pay, PhonePe, Paytm' },
    { value: 'CreditCard', label: 'Credit Card', icon: '💳', desc: 'Visa, Mastercard, RuPay' },
    { value: 'DebitCard', label: 'Debit Card', icon: '🏧', desc: 'All bank debit cards' },
    { value: 'NetBanking', label: 'Net Banking', icon: '🏦', desc: 'Direct bank transfer' },
    { value: 'Wallet', label: 'Wallet', icon: '👛', desc: 'Paytm, PhonePe wallet' },
  ];

  // Payment states
  processing = false;
  paymentStep: 'form' | 'processing' | 'success' | 'failed' = 'form';
  payment: PaymentResponse | null = null;
  failureReason = '';
  actionMessage = '';

  constructor(
    private courseService: CourseService,
    private paymentService: PaymentService,
    private enrollmentService: EnrollmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.courseId = this.route.snapshot.paramMap.get('courseId') || '';
    if (!this.courseId) {
      this.errorMessage = 'No course specified.';
      this.loading = false;
      return;
    }

    this.loadCourse();
  }

  loadCourse() {
    this.courseService.getCourseById(this.courseId).subscribe({
      next: (data) => {
        this.course = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Failed to load course details.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  get totalLessons(): number {
    return this.course?.sections?.reduce((sum, s) => sum + (s.lessons?.length || 0), 0) ?? 0;
  }

  formatPrice(price: number): string {
    return price === 0 ? 'Free' : `₹${price.toLocaleString()}`;
  }

  // ─── Payment Flow ────────────────────────────────────────────────────────

  initiatePayment() {
    if (!this.course) return;

    this.processing = true;
    this.paymentStep = 'processing';
    this.errorMessage = '';
    this.cdr.detectChanges();

    // Step 1: Initiate payment → creates Pending record
    this.paymentService.initiatePayment({
      courseId: this.course.courseId,
      courseTitle: this.course.title,
      amount: this.course.price,
      method: this.selectedMethod,
    }).subscribe({
      next: (payment) => {
        this.payment = payment;
        // Step 2: Simulate gateway processing (2s delay)
        setTimeout(() => this.simulateGateway(payment.paymentId), 2000);
      },
      error: (err) => {
        this.errorMessage = err.error?.error || err.error?.message || 'Failed to initiate payment.';
        this.paymentStep = 'form';
        this.processing = false;
        this.cdr.detectChanges();
      },
    });
  }

  // Simulates a payment gateway confirming the transaction.
  // In production, this would be a webhook callback from Razorpay/Stripe.
  private simulateGateway(paymentId: string) {
    const fakeTransactionId = 'TXN_' + Date.now() + '_' + Math.random().toString(36).substring(2, 8).toUpperCase();

    this.paymentService.confirmPayment(paymentId, {
      transactionId: fakeTransactionId,
    }).subscribe({
      next: (confirmed) => {
        this.payment = confirmed;
        this.paymentStep = 'success';
        this.processing = false;
        this.cdr.detectChanges();

        // Step 3: Auto-enroll student after successful payment
        this.autoEnroll();
      },
      error: (err) => {
        // If confirm fails, mark payment as failed
        this.failureReason = err.error?.error || err.error?.message || 'Payment gateway error.';
        this.markPaymentFailed(paymentId, this.failureReason);
      },
    });
  }

  private markPaymentFailed(paymentId: string, reason: string) {
    this.paymentService.failPayment(paymentId, { reason }).subscribe({
      next: (failed) => {
        this.payment = failed;
        this.paymentStep = 'failed';
        this.processing = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.paymentStep = 'failed';
        this.processing = false;
        this.cdr.detectChanges();
      },
    });
  }

  private autoEnroll() {
    if (!this.course) return;
    this.enrollmentService.enroll({
      courseId: this.course.courseId,
      courseTitle: this.course.title,
      totalLessons: this.totalLessons,
      pricePaid: this.course.price,
    }).subscribe({
      next: () => {
        // Enrollment successful — student can now access the course
      },
      error: () => {
        // Enrollment failed after successful payment — show a non-blocking warning
        this.actionMessage = 'Payment successful, but auto-enrollment failed. Please contact support if your course is not accessible.';
      },
    });
  }

  retryPayment() {
    this.paymentStep = 'form';
    this.payment = null;
    this.errorMessage = '';
    this.failureReason = '';
    this.processing = false;
  }

  goToLearning() {
    this.router.navigate(['/student/my-learning']);
  }

  goToPayments() {
    this.router.navigate(['/student/payments']);
  }
}
