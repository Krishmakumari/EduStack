import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { PaymentService, PaymentResponse } from '../../../services/payment.service';
import { Navbar } from '../../../core/navbar/navbar';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, Navbar],
  templateUrl: './payments.html',
  styleUrl: './payments.css',
})
export class Payments implements OnInit {
  payments: PaymentResponse[] = [];
  loading = true;
  errorMessage = '';
  actionMessage = '';

  // Refund modal
  showRefundModal = false;
  refundPaymentId = '';
  refundReason = '';
  refundProcessing = false;

  constructor(
    private paymentService: PaymentService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    if (this.auth.isBrowser) {
      this.loadPayments();
    }
  }

  loadPayments() {
    this.loading = true;
    this.paymentService.getMyPayments().subscribe({
      next: (data) => {
        this.payments = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Failed to load payment history.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  // ─── Refund Flow ─────────────────────────────────────────────────────────

  openRefund(paymentId: string) {
    this.refundPaymentId = paymentId;
    this.refundReason = '';
    this.showRefundModal = true;
  }

  cancelRefund() {
    this.showRefundModal = false;
    this.refundPaymentId = '';
    this.refundReason = '';
  }

  submitRefund() {
    if (!this.refundReason.trim()) return;

    this.refundProcessing = true;
    this.paymentService.refundPayment(this.refundPaymentId, {
      reason: this.refundReason
    }).subscribe({
      next: (updated) => {
        const idx = this.payments.findIndex(p => p.paymentId === this.refundPaymentId);
        if (idx >= 0) this.payments[idx] = updated;
        this.cancelRefund();
        this.refundProcessing = false;
        this.showAction('Refund processed successfully.');
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.refundProcessing = false;
        this.showAction(err.error?.error || err.error?.message || 'Refund failed.');
        this.cdr.detectChanges();
      },
    });
  }

  // ─── Helpers ─────────────────────────────────────────────────────────────

  formatPrice(amount: number): string {
    return amount === 0 ? 'Free' : `₹${amount.toLocaleString()}`;
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-IN', {
      day: 'numeric', month: 'short', year: 'numeric'
    });
  }

  formatDateTime(dateStr: string | null): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleString('en-IN', {
      day: 'numeric', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }

  private showAction(msg: string) {
    this.actionMessage = msg;
    setTimeout(() => (this.actionMessage = ''), 4000);
  }
}
