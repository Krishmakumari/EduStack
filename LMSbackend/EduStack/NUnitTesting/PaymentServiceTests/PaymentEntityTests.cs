// PaymentEntityTests — Unit tests for Payment and Refund domain entities.
// Tests factory methods, guard methods, and lifecycle state transitions.

using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;

namespace NUnitTesting.PaymentServiceTests;

[TestFixture]
public class PaymentEntityTests
{
    [Test]
    public void Create_SetsDefaultValues()
    {
        var payment = Payment.Create(
            Guid.NewGuid(), "Student", Guid.NewGuid(), "Course", 499m, PaymentMethod.UPI);

        Assert.That(payment.PaymentId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Pending));
        Assert.That(payment.Amount, Is.EqualTo(499m));
        Assert.That(payment.Method, Is.EqualTo(PaymentMethod.UPI));
        Assert.That(payment.TransactionId, Is.Null);
        Assert.That(payment.CompletedAt, Is.Null);
        Assert.That(payment.RefundedAt, Is.Null);
    }

    [Test]
    public void MarkCompleted_TransitionsToPendingToCompleted()
    {
        var payment = Payment.Create(Guid.NewGuid(), "S", Guid.NewGuid(), "C", 100m, PaymentMethod.CreditCard);

        payment.MarkCompleted("tx-123");

        Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Completed));
        Assert.That(payment.TransactionId, Is.EqualTo("tx-123"));
        Assert.That(payment.CompletedAt, Is.Not.Null);
    }

    [Test]
    public void MarkCompleted_DoubleConfirm_ThrowsPaymentAlreadyCompleted()
    {
        var payment = Payment.Create(Guid.NewGuid(), "S", Guid.NewGuid(), "C", 100m, PaymentMethod.UPI);
        payment.MarkCompleted("tx-1");

        // Second confirm should throw
        Assert.Throws<PaymentAlreadyCompletedException>(() => payment.MarkCompleted("tx-2"));
    }

    [Test]
    public void MarkFailed_SetsStatusAndReason()
    {
        var payment = Payment.Create(Guid.NewGuid(), "S", Guid.NewGuid(), "C", 100m, PaymentMethod.UPI);

        payment.MarkFailed("Insufficient funds");

        Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Failed));
        Assert.That(payment.FailureReason, Is.EqualTo("Insufficient funds"));
    }

    [Test]
    public void MarkRefunded_FromCompleted_SetsStatusAndTimestamp()
    {
        var payment = Payment.Create(Guid.NewGuid(), "S", Guid.NewGuid(), "C", 100m, PaymentMethod.UPI);
        payment.MarkCompleted("tx-1"); // must complete first

        payment.MarkRefunded();

        Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Refunded));
        Assert.That(payment.RefundedAt, Is.Not.Null);
    }

    [Test]
    public void MarkRefunded_FromPending_ThrowsDomainException()
    {
        var payment = Payment.Create(Guid.NewGuid(), "S", Guid.NewGuid(), "C", 100m, PaymentMethod.UPI);

        var ex = Assert.Throws<DomainException>(() => payment.MarkRefunded());
        Assert.That(ex!.Message, Does.Contain("Only completed payments"));
    }

    [Test]
    public void MarkRefunded_FromFailed_ThrowsDomainException()
    {
        var payment = Payment.Create(Guid.NewGuid(), "S", Guid.NewGuid(), "C", 100m, PaymentMethod.UPI);
        payment.MarkFailed("reason");

        Assert.Throws<DomainException>(() => payment.MarkRefunded());
    }
}

[TestFixture]
public class RefundEntityTests
{
    [Test]
    public void Create_SetsDefaultValues()
    {
        var paymentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var refund = Refund.Create(paymentId, studentId, 500m, "Not satisfied");

        Assert.That(refund.RefundId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(refund.PaymentId, Is.EqualTo(paymentId));
        Assert.That(refund.StudentId, Is.EqualTo(studentId));
        Assert.That(refund.Amount, Is.EqualTo(500m));
        Assert.That(refund.Reason, Is.EqualTo("Not satisfied"));
        Assert.That(refund.TransactionId, Is.Null);
    }

    [Test]
    public void SetTransactionId_SetsGatewayReference()
    {
        var refund = Refund.Create(Guid.NewGuid(), Guid.NewGuid(), 100m, "Reason");

        refund.SetTransactionId("REFUND-TX-001");

        Assert.That(refund.TransactionId, Is.EqualTo("REFUND-TX-001"));
    }
}
