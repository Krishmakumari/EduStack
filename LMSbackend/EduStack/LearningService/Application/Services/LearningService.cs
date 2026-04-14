// LearningService — Core business logic for video progress tracking (Application Layer).
// • Tracks how many seconds a student has watched per lesson.
// • Cross-service: calls EnrollmentClient to verify enrollment BEFORE updating progress.
// • 80% rule: a lesson is "Completed" when watched/total >= 80%.
// • Upsert pattern: creates a new record on first watch, updates on re-watch.

using LearningService.Application.DTOs.Requests;
using LearningService.Application.DTOs.Responses;
using LearningService.Application.Interfaces;
using LearningService.Domain.Entities;
using LearningService.Domain.Exceptions;

namespace LearningService.Application.Services;

public class LearningService : ILearningService
{
    // Repository for LessonProgress records in this service's database.
    private readonly IProgressRepository _progressRepository;

    // HTTP client that calls Enrollment Service to check access rights.
    // This is the cross-service dependency — Learning Service does NOT own enrollment data.
    private readonly IEnrollmentClient _enrollmentClient;

    public LearningService(
        IProgressRepository progressRepository,
        IEnrollmentClient enrollmentClient)
    {
        _progressRepository = progressRepository;
        _enrollmentClient = enrollmentClient;
    }

    // ─── UpdateProgress ────────────────────────────────────────────────────────
    // Records how far a student has watched a lesson video.
    // Called frequently (e.g., every 30 seconds as video plays) to persist resume position.
    // GATE: Must be enrolled in the course — checked via HTTP call to Enrollment Service.
    public async Task UpdateProgressAsync(UpdateProgressRequest request, Guid userId)
    {
        // Step 1: Verify the user is actually enrolled in this course.
        // If not enrolled, throw — prevents non-enrolled users from recording progress.
        var isEnrolled = await _enrollmentClient
            .IsUserEnrolledAsync(userId, request.CourseId);

        if (!isEnrolled)
            throw new UnauthorizedLessonAccessException();  // → 403 Forbidden

        // Step 2: Find existing progress record for this user + course + lesson.
        // Uses a 3-key composite lookup matching the unique DB index.
        var progress = await _progressRepository
            .GetAsync(userId, request.CourseId, request.LessonId);

        if (progress == null)
        {
            // Step 3a: First time watching — create a new LessonProgress record.
            progress = new LessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CourseId = request.CourseId,
                LessonId = request.LessonId,
                WatchedSeconds = request.WatchedSeconds,  // current watch position
                LastAccessedAt = DateTime.UtcNow
            };

            // Determine completion: has the student watched at least 80% of the video?
            progress.IsCompleted = IsLessonCompleted(
                request.WatchedSeconds,
                request.TotalDurationSeconds);

            await _progressRepository.AddAsync(progress);  // INSERT
        }
        else
        {
            // Step 3b: Student is resuming or re-watching — update existing record.
            // WatchedSeconds is overwritten (not accumulated) — it's the current position.
            progress.WatchedSeconds = request.WatchedSeconds;
            progress.LastAccessedAt = DateTime.UtcNow;  // update resume timestamp

            // Re-evaluate completion on every update — student may have watched more.
            progress.IsCompleted = IsLessonCompleted(
                request.WatchedSeconds,
                request.TotalDurationSeconds);

            await _progressRepository.UpdateAsync(progress);  // UPDATE
        }
    }

    // ─── GetLessonProgress ────────────────────────────────────────────────────
    // Returns the current watch progress for one specific lesson.
    // Used to resume video at the correct position, and show completion status.
    public async Task<LessonProgressResponse> GetLessonProgressAsync(
        Guid courseId,
        Guid lessonId,
        Guid userId)
    {
        var progress = await _progressRepository
            .GetAsync(userId, courseId, lessonId);

        // If no record exists, the student has never watched this lesson.
        if (progress == null)
            throw new ProgressNotFoundException(lessonId);  // → 404 Not Found

        return new LessonProgressResponse
        {
            LessonId = lessonId,
            WatchedSeconds = progress.WatchedSeconds,
            IsCompleted = progress.IsCompleted,
            ProgressPercentage = 0  // placeholder — could calculate (watched/total * 100) if total were stored
        };
    }

    // ─── GetCourseProgress ────────────────────────────────────────────────────
    // Returns overall course completion stats by counting LessonProgress records.
    // CourseProgress is NOT stored in the DB — calculated in memory from lesson data.
    public async Task<CourseProgressResponse> GetCourseProgressAsync(
        Guid courseId,
        Guid userId)
    {
        // Load all lesson records for this user + course from DB.
        var lessons = await _progressRepository
            .GetByCourseAsync(userId, courseId);

        var total = lessons.Count;
        var completed = lessons.Count(l => l.IsCompleted);

        return new CourseProgressResponse
        {
            CourseId = courseId,
            TotalLessons = total,
            CompletedLessons = completed,
            // Division-by-zero guard: if no lessons tracked yet, return 0%.
            CompletionPercentage = total == 0
                ? 0
                : (double)completed / total * 100
        };
    }

    // ─── IsLessonCompleted ────────────────────────────────────────────────────
    // Business rule: a lesson is "completed" when the student has watched >= 80%.
    // 80% threshold: industry standard (Udemy, Coursera) — accounts for students
    // skipping intro/outro but penalizes skipping core content.
    private bool IsLessonCompleted(int watched, int total)
    {
        if (total == 0) return false;  // guard: no duration = can't determine completion

        var percentage = (double)watched / total * 100;
        return percentage >= 80;       // 80% = "effectively completed"
    }
}