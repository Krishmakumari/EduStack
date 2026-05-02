// LearningServiceTests — Unit tests for progress tracking and enrollment gating.

using Moq;
using LearningService.Application.DTOs.Requests;
using LearningService.Application.Interfaces;
using LearningService.Domain.Entities;
using LearningService.Domain.Exceptions;

namespace NUnitTesting.LearningServiceTests;

[TestFixture]
public class LearningServiceTests
{
    private Mock<IProgressRepository> _progressRepoMock;
    private Mock<IEnrollmentClient> _enrollmentClientMock;
    private LearningService.Application.Services.LearningService _learningService;

    [SetUp]
    public void Setup()
    {
        _progressRepoMock = new Mock<IProgressRepository>();
        _enrollmentClientMock = new Mock<IEnrollmentClient>();
        _learningService = new LearningService.Application.Services.LearningService(
            _progressRepoMock.Object, _enrollmentClientMock.Object);
    }

    [Test]
    public void UpdateProgressAsync_NotEnrolled_ThrowsUnauthorized()
    {
        _enrollmentClientMock.Setup(c => c.IsUserEnrolledAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(false);
        var request = new UpdateProgressRequest
        { CourseId = Guid.NewGuid(), LessonId = Guid.NewGuid(), WatchedSeconds = 100, TotalDurationSeconds = 200 };

        Assert.ThrowsAsync<UnauthorizedLessonAccessException>(
            async () => await _learningService.UpdateProgressAsync(request, Guid.NewGuid()));
    }

    [Test]
    public async Task UpdateProgressAsync_FirstWatch_CreatesNewRecord()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        _enrollmentClientMock.Setup(c => c.IsUserEnrolledAsync(userId, courseId)).ReturnsAsync(true);
        _progressRepoMock.Setup(r => r.GetAsync(userId, courseId, lessonId)).ReturnsAsync((LessonProgress?)null);
        _progressRepoMock.Setup(r => r.AddAsync(It.IsAny<LessonProgress>())).Returns(Task.CompletedTask);

        var request = new UpdateProgressRequest
        { CourseId = courseId, LessonId = lessonId, WatchedSeconds = 50, TotalDurationSeconds = 200 };

        await _learningService.UpdateProgressAsync(request, userId);

        _progressRepoMock.Verify(r => r.AddAsync(It.Is<LessonProgress>(p => p.WatchedSeconds == 50 && !p.IsCompleted)), Times.Once);
    }

    [Test]
    public async Task UpdateProgressAsync_80Percent_MarksCompleted()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        _enrollmentClientMock.Setup(c => c.IsUserEnrolledAsync(userId, courseId)).ReturnsAsync(true);
        _progressRepoMock.Setup(r => r.GetAsync(userId, courseId, lessonId)).ReturnsAsync((LessonProgress?)null);
        _progressRepoMock.Setup(r => r.AddAsync(It.IsAny<LessonProgress>())).Returns(Task.CompletedTask);

        var request = new UpdateProgressRequest
        { CourseId = courseId, LessonId = lessonId, WatchedSeconds = 160, TotalDurationSeconds = 200 };

        await _learningService.UpdateProgressAsync(request, userId);

        _progressRepoMock.Verify(r => r.AddAsync(It.Is<LessonProgress>(p => p.IsCompleted)), Times.Once);
    }

    [Test]
    public async Task UpdateProgressAsync_ExistingRecord_UpdatesInstead()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var existing = new LessonProgress { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId,
            LessonId = lessonId, WatchedSeconds = 30, IsCompleted = false, LastAccessedAt = DateTime.UtcNow.AddHours(-1) };
        _enrollmentClientMock.Setup(c => c.IsUserEnrolledAsync(userId, courseId)).ReturnsAsync(true);
        _progressRepoMock.Setup(r => r.GetAsync(userId, courseId, lessonId)).ReturnsAsync(existing);
        _progressRepoMock.Setup(r => r.UpdateAsync(It.IsAny<LessonProgress>())).Returns(Task.CompletedTask);

        var request = new UpdateProgressRequest
        { CourseId = courseId, LessonId = lessonId, WatchedSeconds = 180, TotalDurationSeconds = 200 };

        await _learningService.UpdateProgressAsync(request, userId);

        _progressRepoMock.Verify(r => r.UpdateAsync(It.IsAny<LessonProgress>()), Times.Once);
        _progressRepoMock.Verify(r => r.AddAsync(It.IsAny<LessonProgress>()), Times.Never);
    }

    [Test]
    public void GetLessonProgressAsync_NoProgress_ThrowsNotFound()
    {
        _progressRepoMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((LessonProgress?)null);

        Assert.ThrowsAsync<ProgressNotFoundException>(
            async () => await _learningService.GetLessonProgressAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
    }

    [Test]
    public async Task GetCourseProgressAsync_MixedProgress_CalculatesCorrectly()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessons = new List<LessonProgress>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, LessonId = Guid.NewGuid(), WatchedSeconds = 200, IsCompleted = true },
            new() { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, LessonId = Guid.NewGuid(), WatchedSeconds = 200, IsCompleted = true },
            new() { Id = Guid.NewGuid(), UserId = userId, CourseId = courseId, LessonId = Guid.NewGuid(), WatchedSeconds = 50, IsCompleted = false }
        };
        _progressRepoMock.Setup(r => r.GetByCourseAsync(userId, courseId)).ReturnsAsync(lessons);

        var result = await _learningService.GetCourseProgressAsync(courseId, userId);

        Assert.That(result.TotalLessons, Is.EqualTo(3));
        Assert.That(result.CompletedLessons, Is.EqualTo(2));
        Assert.That(result.CompletionPercentage, Is.EqualTo(200.0 / 3).Within(0.1));
    }

    [Test]
    public async Task GetCourseProgressAsync_NoLessons_ReturnsZero()
    {
        _progressRepoMock.Setup(r => r.GetByCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<LessonProgress>());

        var result = await _learningService.GetCourseProgressAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.That(result.TotalLessons, Is.EqualTo(0));
        Assert.That(result.CompletionPercentage, Is.EqualTo(0));
    }
}
