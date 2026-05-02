// PaymentServiceTests — Unit tests for PaymentService.Application.Services.PaymentService
// Tests the 4-state payment lifecycle: Initiate → Confirm | Fail → Refund.

using Moq;
using PaymentService.Application.DTOs.Requests;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;

namespace NUnitTesting.PaymentServiceTests;

[TestFixture]
public class PaymentServiceTests
{
    private Mock<IPaymentRepository> _paymentRepoMock;
    private Mock<IRefundRepository> _refundRepoMock;
    private PaymentService.Application.Services.PaymentService _paymentService;

    [SetUp]
    public void Setup()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _refundRepoMock = new Mock<IRefundRepository>();

        _paymentService = new PaymentService.Application.Services.PaymentService(
            _paymentRepoMock.Object,
            _refundRepoMock.Object);
    }

    // ─── InitiatePayment Tests ────────────────────────────────────────────────

    [Test]
    public async Task InitiatePaymentAsync_NewPayment_ReturnsPendingResponse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _paymentRepoMock.Setup(r => r.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync((Payment?)null);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
        _paymentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new InitiatePaymentRequest
        {
            CourseId = courseId,
            CourseTitle = "C# Course",
            Amount = 499m,
            Method = "UPI"
        };

        // Act
        var result = await _paymentService.InitiatePaymentAsync(studentId, "Student", request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo("Pending"));
        Assert.That(result.Amount, Is.EqualTo(499m));
        Assert.That(result.Method, Is.EqualTo("UPI"));
    }

    [Test]
    public void InitiatePaymentAsync_AlreadyPurchased_ThrowsDomainException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existingPayment = Payment.Create(studentId, "Student", courseId, "Course", 100m, PaymentMethod.UPI);
        existingPayment.MarkCompleted("tx-123");

        _paymentRepoMock.Setup(r => r.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync(existingPayment);

        var request = new InitiatePaymentRequest
        {
            CourseId = courseId,
            CourseTitle = "Course",
            Amount = 100m,
            Method = "UPI"
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomainException>(
            async () => await _paymentService.InitiatePaymentAsync(studentId, "Student", request));
        Assert.That(ex!.Message, Does.Contain("already purchased"));
    }

    [Test]
    public void InitiatePaymentAsync_InvalidMethod_ThrowsDomainException()
    {
        // Arrange
        _paymentRepoMock.Setup(r => r.GetByStudentAndCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((Payment?)null);

        var request = new InitiatePaymentRequest
        {
            CourseId = Guid.NewGuid(),
            CourseTitle = "Course",
            Amount = 100m,
            Method = "Bitcoin" // invalid
        };

        // Act & Assert
        Assert.ThrowsAsync<DomainException>(
            async () => await _paymentService.InitiatePaymentAsync(Guid.NewGuid(), "Student", request));
    }

    // ─── ConfirmPayment Tests ─────────────────────────────────────────────────

    [Test]
    public async Task ConfirmPaymentAsync_ValidPayment_ReturnsCompletedResponse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var payment = Payment.Create(studentId, "Student", Guid.NewGuid(), "Course", 499m, PaymentMethod.CreditCard);

        _paymentRepoMock.Setup(r => r.GetByIdAsync(payment.PaymentId)).ReturnsAsync(payment);
        _paymentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new ConfirmPaymentRequest { TransactionId = "RAZORPAY-TX-001" };

        // Act
        var result = await _paymentService.ConfirmPaymentAsync(studentId, payment.PaymentId, request);

        // Assert
        Assert.That(result.Status, Is.EqualTo("Completed"));
        Assert.That(result.TransactionId, Is.EqualTo("RAZORPAY-TX-001"));
    }

    [Test]
    public void ConfirmPaymentAsync_WrongStudent_ThrowsUnauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var payment = Payment.Create(ownerId, "Owner", Guid.NewGuid(), "Course", 100m, PaymentMethod.UPI);

        _paymentRepoMock.Setup(r => r.GetByIdAsync(payment.PaymentId)).ReturnsAsync(payment);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedPaymentAccessException>(
            async () => await _paymentService.ConfirmPaymentAsync(
                attackerId, payment.PaymentId, new ConfirmPaymentRequest { TransactionId = "tx" }));
    }

    [Test]
    public void ConfirmPaymentAsync_NotFound_ThrowsPaymentNotFound()
    {
        // Arrange
        _paymentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Payment?)null);

        // Act & Assert
        Assert.ThrowsAsync<PaymentNotFoundException>(
            async () => await _paymentService.ConfirmPaymentAsync(
                Guid.NewGuid(), Guid.NewGuid(), new ConfirmPaymentRequest { TransactionId = "tx" }));
    }

    // ─── FailPayment Tests ────────────────────────────────────────────────────

    [Test]
    public async Task FailPaymentAsync_ValidPayment_ReturnsFailedResponse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var payment = Payment.Create(studentId, "Student", Guid.NewGuid(), "Course", 100m, PaymentMethod.UPI);

        _paymentRepoMock.Setup(r => r.GetByIdAsync(payment.PaymentId)).ReturnsAsync(payment);
        _paymentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new FailPaymentRequest { Reason = "Card declined" };

        // Act
        var result = await _paymentService.FailPaymentAsync(studentId, payment.PaymentId, request);

        // Assert
        Assert.That(result.Status, Is.EqualTo("Failed"));
        Assert.That(result.FailureReason, Is.EqualTo("Card declined"));
    }

    // ─── RefundPayment Tests ──────────────────────────────────────────────────

    [Test]
    public async Task RefundPaymentAsync_CompletedPayment_ReturnsRefundedResponse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var payment = Payment.Create(studentId, "Student", Guid.NewGuid(), "Course", 500m, PaymentMethod.CreditCard);
        payment.MarkCompleted("tx-001"); // must be completed to refund

        _paymentRepoMock.Setup(r => r.GetByIdAsync(payment.PaymentId)).ReturnsAsync(payment);
        _refundRepoMock.Setup(r => r.AddAsync(It.IsAny<Refund>())).Returns(Task.CompletedTask);
        _refundRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new RefundPaymentRequest { Reason = "Student unsatisfied" };

        // Act
        var result = await _paymentService.RefundPaymentAsync(studentId, payment.PaymentId, request);

        // Assert
        Assert.That(result.Status, Is.EqualTo("Refunded"));
        _refundRepoMock.Verify(r => r.AddAsync(It.IsAny<Refund>()), Times.Once);
    }

    [Test]
    public void RefundPaymentAsync_PendingPayment_ThrowsDomainException()
    {
        // Arrange — payment is still Pending (not Completed), can't refund
        var studentId = Guid.NewGuid();
        var payment = Payment.Create(studentId, "Student", Guid.NewGuid(), "Course", 100m, PaymentMethod.UPI);

        _paymentRepoMock.Setup(r => r.GetByIdAsync(payment.PaymentId)).ReturnsAsync(payment);

        var request = new RefundPaymentRequest { Reason = "Changed mind" };

        // Act & Assert
        Assert.ThrowsAsync<DomainException>(
            async () => await _paymentService.RefundPaymentAsync(studentId, payment.PaymentId, request));
    }

    // ─── GetMyPayments Tests ──────────────────────────────────────────────────

    [Test]
    public async Task GetMyPaymentsAsync_ReturnsStudentPayments()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            Payment.Create(studentId, "Student", Guid.NewGuid(), "Course 1", 100m, PaymentMethod.UPI),
            Payment.Create(studentId, "Student", Guid.NewGuid(), "Course 2", 200m, PaymentMethod.CreditCard)
        };

        _paymentRepoMock.Setup(r => r.GetByStudentIdAsync(studentId)).ReturnsAsync(payments);

        // Act
        var result = (await _paymentService.GetMyPaymentsAsync(studentId)).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    // ─── GetPaymentById Tests ─────────────────────────────────────────────────

    [Test]
    public void GetPaymentByIdAsync_WrongOwner_ThrowsUnauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var payment = Payment.Create(ownerId, "Owner", Guid.NewGuid(), "Course", 100m, PaymentMethod.UPI);

        _paymentRepoMock.Setup(r => r.GetByIdAsync(payment.PaymentId)).ReturnsAsync(payment);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedPaymentAccessException>(
            async () => await _paymentService.GetPaymentByIdAsync(Guid.NewGuid(), payment.PaymentId));
    }
}
