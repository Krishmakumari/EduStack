// EnrollmentServiceTests — Unit tests for EnrollmentService.Application.Services.EnrollmentService
// Tests enrollment creation, duplicate prevention, progress tracking, and ownership validation.

using Moq;
using EnrollmentService.Application.DTOs.Requests;
using EnrollmentService.Application.Interfaces;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Domain.Exceptions;
using EnrollmentService.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

namespace NUnitTesting.EnrollmentServiceTests;

[TestFixture]
public class EnrollmentServiceTests
{
    private Mock<IEnrollmentRepository> _enrollmentRepoMock;
    private Mock<ILessonProgressRepository> _progressRepoMock;
    private Mock<RabbitMqPublisher> _publisherMock;
    private EnrollmentService.Application.Services.EnrollmentService _enrollmentService;

    [SetUp]
    public void Setup()
    {
        _enrollmentRepoMock = new Mock<IEnrollmentRepository>();
        _progressRepoMock = new Mock<ILessonProgressRepository>();

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
        _publisherMock = new Mock<RabbitMqPublisher>(configMock.Object);

        _enrollmentService = new EnrollmentService.Application.Services.EnrollmentService(
            _enrollmentRepoMock.Object,
            _progressRepoMock.Object,
            _publisherMock.Object);
    }

    // ─── Enroll Tests ─────────────────────────────────────────────────────────

    [Test]
    public async Task EnrollAsync_NewEnrollment_SavesEnrollmentToRepo()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _enrollmentRepoMock.Setup(r => r.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync((Enrollment?)null);
        _enrollmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Enrollment>())).Returns(Task.CompletedTask);
        _enrollmentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        // Note: RabbitMqPublisher.PublishAsync is non-virtual and will throw in test env.

        var request = new EnrollRequest
        {
            CourseId = courseId,
            CourseTitle = "C# Masterclass",
            PricePaid = 499m
        };

        // Act — catch the RabbitMQ connection failure (publish happens after DB save)
        try
        {
            await _enrollmentService.EnrollAsync(studentId, "Student Name", "student@test.com", request);
        }
        catch { /* Expected: RabbitMQ connection failure */ }

        // Assert — verify the enrollment was persisted (core business logic)
        _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Enrollment>()), Times.Once);
        _enrollmentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void EnrollAsync_AlreadyEnrolled_ThrowsAlreadyEnrolledException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existingEnrollment = Enrollment.Create(studentId, "Student", courseId, "Course", 100m);

        _enrollmentRepoMock.Setup(r => r.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync(existingEnrollment);

        var request = new EnrollRequest
        {
            CourseId = courseId,
            CourseTitle = "Course",
            PricePaid = 100m
        };

        // Act & Assert
        Assert.ThrowsAsync<AlreadyEnrolledException>(
            async () => await _enrollmentService.EnrollAsync(studentId, "Student", "s@test.com", request));
    }

    // ─── GetMyEnrollments Tests ───────────────────────────────────────────────

    [Test]
    public async Task GetMyEnrollmentsAsync_ReturnsStudentEnrollments()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollments = new List<Enrollment>
        {
            Enrollment.Create(studentId, "Student", Guid.NewGuid(), "Course 1", 200m),
            Enrollment.Create(studentId, "Student", Guid.NewGuid(), "Course 2", 300m)
        };

        _enrollmentRepoMock.Setup(r => r.GetByStudentIdAsync(studentId)).ReturnsAsync(enrollments);

        // Act
        var result = (await _enrollmentService.GetMyEnrollmentsAsync(studentId)).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(e => e.StudentId == studentId), Is.True);
    }

    // ─── GetEnrollmentById Tests ──────────────────────────────────────────────

    [Test]
    public void GetEnrollmentByIdAsync_WrongStudent_ThrowsUnauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var enrollment = Enrollment.Create(ownerId, "Owner", Guid.NewGuid(), "Course", 100m);

        _enrollmentRepoMock.Setup(r => r.GetByIdWithProgressAsync(enrollment.EnrollmentId))
            .ReturnsAsync(enrollment);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedEnrollmentAccessException>(
            async () => await _enrollmentService.GetEnrollmentByIdAsync(attackerId, enrollment.EnrollmentId));
    }

    [Test]
    public void GetEnrollmentByIdAsync_NotFound_ThrowsEnrollmentNotFound()
    {
        // Arrange
        _enrollmentRepoMock.Setup(r => r.GetByIdWithProgressAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Enrollment?)null);

        // Act & Assert
        Assert.ThrowsAsync<EnrollmentNotFoundException>(
            async () => await _enrollmentService.GetEnrollmentByIdAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    // ─── IsUserEnrolled Tests ─────────────────────────────────────────────────

    [Test]
    public async Task IsUserEnrolledAsync_Enrolled_ReturnsTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enrollment = Enrollment.Create(studentId, "Student", courseId, "Course", 100m);

        _enrollmentRepoMock.Setup(r => r.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _enrollmentService.IsUserEnrolledAsync(studentId, courseId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsUserEnrolledAsync_NotEnrolled_ReturnsFalse()
    {
        // Arrange
        _enrollmentRepoMock.Setup(r => r.GetByStudentAndCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((Enrollment?)null);

        // Act
        var result = await _enrollmentService.IsUserEnrolledAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.That(result, Is.False);
    }

    // ─── GetAllEnrollments Tests ──────────────────────────────────────────────

    [Test]
    public async Task GetAllEnrollmentsAsync_ReturnsAllEnrollments()
    {
        // Arrange
        var enrollments = new List<Enrollment>
        {
            Enrollment.Create(Guid.NewGuid(), "A", Guid.NewGuid(), "C1", 100m),
            Enrollment.Create(Guid.NewGuid(), "B", Guid.NewGuid(), "C2", 200m),
            Enrollment.Create(Guid.NewGuid(), "C", Guid.NewGuid(), "C3", 300m)
        };

        _enrollmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(enrollments);

        // Act
        var result = (await _enrollmentService.GetAllEnrollmentsAsync()).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
    }
}
