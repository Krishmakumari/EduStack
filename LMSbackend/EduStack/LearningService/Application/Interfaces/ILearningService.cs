using LearningService.Application.DTOs.Requests;
using LearningService.Application.DTOs.Responses;

namespace LearningService.Application.Interfaces;

public interface ILearningService
{
    Task UpdateProgressAsync(UpdateProgressRequest request, Guid userId);

    Task<LessonProgressResponse> GetLessonProgressAsync(
        Guid courseId,
        Guid lessonId,
        Guid userId);

    Task<CourseProgressResponse> GetCourseProgressAsync(
        Guid courseId,
        Guid userId);
}