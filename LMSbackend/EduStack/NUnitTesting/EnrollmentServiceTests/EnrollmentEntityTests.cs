// EnrollmentEntityTests — Unit tests for the Enrollment and LessonProgress domain entities.

using EnrollmentService.Domain.Entities;
using EnrollmentService.Domain.Enums;

namespace NUnitTesting.EnrollmentServiceTests;

[TestFixture]
public class EnrollmentEntityTests
{
    [Test]
    public void Create_SetsDefaultValues()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var enrollment = Enrollment.Create(studentId, "Student Name", courseId, "Course Title", 499m);

        Assert.That(enrollment.EnrollmentId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(enrollment.StudentId, Is.EqualTo(studentId));
        Assert.That(enrollment.CourseId, Is.EqualTo(courseId));
        Assert.That(enrollment.PricePaid, Is.EqualTo(499m));
        Assert.That(enrollment.Status, Is.EqualTo(EnrollmentStatus.Active));
        Assert.That(enrollment.CompletedAt, Is.Null);
    }

    [Test]
    public void MarkCompleted_SetsStatusAndTimestamp()
    {
        var enrollment = Enrollment.Create(Guid.NewGuid(), "S", Guid.NewGuid(), "C", 100m);

        enrollment.MarkCompleted();

        Assert.That(enrollment.Status, Is.EqualTo(EnrollmentStatus.Completed));
        Assert.That(enrollment.CompletedAt, Is.Not.Null);
    }

    [Test]
    public void Cancel_SetsStatusToCancelled()
    {
        var enrollment = Enrollment.Create(Guid.NewGuid(), "S", Guid.NewGuid(), "C", 100m);

        enrollment.Cancel();

        Assert.That(enrollment.Status, Is.EqualTo(EnrollmentStatus.Cancelled));
    }
}

[TestFixture]
public class LessonProgressEntityTests
{
    [Test]
    public void Create_SetsDefaultValues()
    {
        var enrollmentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        var progress = LessonProgress.Create(enrollmentId, lessonId);

        Assert.That(progress.LessonProgressId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(progress.EnrollmentId, Is.EqualTo(enrollmentId));
        Assert.That(progress.LessonId, Is.EqualTo(lessonId));
        Assert.That(progress.IsCompleted, Is.False);
        Assert.That(progress.CompletedAt, Is.Null);
    }

    [Test]
    public void MarkCompleted_SetsCompletionFlagAndTimestamp()
    {
        var progress = LessonProgress.Create(Guid.NewGuid(), Guid.NewGuid());

        progress.MarkCompleted();

        Assert.That(progress.IsCompleted, Is.True);
        Assert.That(progress.CompletedAt, Is.Not.Null);
    }
}
