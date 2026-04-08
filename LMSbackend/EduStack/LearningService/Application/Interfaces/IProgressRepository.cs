using LearningService.Domain.Entities;

namespace LearningService.Application.Interfaces;

public interface IProgressRepository
{
    Task<LessonProgress?> GetAsync(Guid userId, Guid courseId, Guid lessonId);
    Task<List<LessonProgress>> GetByCourseAsync(Guid userId, Guid courseId);

    Task AddAsync(LessonProgress progress);
    Task UpdateAsync(LessonProgress progress);
}