using LearningService.Application.DTOs.Requests;
using LearningService.Application.DTOs.Responses;
using LearningService.Application.Interfaces;
using LearningService.Domain.Entities;
using LearningService.Domain.Exceptions;

namespace LearningService.Application.Services;

public class LearningService : ILearningService
{
    private readonly IProgressRepository _progressRepository;
    private readonly IEnrollmentClient _enrollmentClient;

    public LearningService(
        IProgressRepository progressRepository,
        IEnrollmentClient enrollmentClient)
    {
        _progressRepository = progressRepository;
        _enrollmentClient = enrollmentClient;
    }

    public async Task UpdateProgressAsync(UpdateProgressRequest request, Guid userId)
    {
        // 🔐 Check enrollment
        var isEnrolled = await _enrollmentClient
            .IsUserEnrolledAsync(userId, request.CourseId);

        if (!isEnrolled)
            throw new UnauthorizedLessonAccessException();

        var progress = await _progressRepository
            .GetAsync(userId, request.CourseId, request.LessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CourseId = request.CourseId,
                LessonId = request.LessonId,
                WatchedSeconds = request.WatchedSeconds,
                LastAccessedAt = DateTime.UtcNow
            };

            progress.IsCompleted = IsLessonCompleted(
                request.WatchedSeconds,
                request.TotalDurationSeconds);

            await _progressRepository.AddAsync(progress);
        }
        else
        {
            progress.WatchedSeconds = request.WatchedSeconds;
            progress.LastAccessedAt = DateTime.UtcNow;

            progress.IsCompleted = IsLessonCompleted(
                request.WatchedSeconds,
                request.TotalDurationSeconds);

            await _progressRepository.UpdateAsync(progress);
        }
    }

    public async Task<LessonProgressResponse> GetLessonProgressAsync(
        Guid courseId,
        Guid lessonId,
        Guid userId)
    {
        var progress = await _progressRepository
            .GetAsync(userId, courseId, lessonId);

        if (progress == null)
            throw new ProgressNotFoundException(lessonId);

        return new LessonProgressResponse
        {
            LessonId = lessonId,
            WatchedSeconds = progress.WatchedSeconds,
            IsCompleted = progress.IsCompleted,
            ProgressPercentage = 0 // calculated later if needed
        };
    }

    public async Task<CourseProgressResponse> GetCourseProgressAsync(
        Guid courseId,
        Guid userId)
    {
        var lessons = await _progressRepository
            .GetByCourseAsync(userId, courseId);

        var total = lessons.Count;
        var completed = lessons.Count(l => l.IsCompleted);

        return new CourseProgressResponse
        {
            CourseId = courseId,
            TotalLessons = total,
            CompletedLessons = completed,
            CompletionPercentage = total == 0
                ? 0
                : (double)completed / total * 100
        };
    }

    private bool IsLessonCompleted(int watched, int total)
    {
        if (total == 0) return false;

        var percentage = (double)watched / total * 100;
        return percentage >= 80;
    }
}