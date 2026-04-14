// ILearningService — Contract for lesson progress tracking operations.
// • UpdateProgressAsync: upsert watch position (called frequently by video player).
// • GetLessonProgressAsync: resume position + completion status for one lesson.
// • GetCourseProgressAsync: aggregate stats — total/completed/percentage for course.

using LearningService.Application.DTOs.Requests;
using LearningService.Application.DTOs.Responses;

namespace LearningService.Application.Interfaces;

public interface ILearningService
{
    // Update (or create) the watch progress for one lesson.
    // Gates on enrollment check via IEnrollmentClient before persisting.
    Task UpdateProgressAsync(UpdateProgressRequest request, Guid userId);

    // Get progress for one specific lesson — watchedSeconds, isCompleted.
    // Used to resume video at the correct position.
    Task<LessonProgressResponse> GetLessonProgressAsync(
        Guid courseId,
        Guid lessonId,
        Guid userId);

    // Get overall course progress — lesson counts and completion percentage.
    // Calculated in memory from all LessonProgress records for this user+course.
    Task<CourseProgressResponse> GetCourseProgressAsync(
        Guid courseId,
        Guid userId);
}