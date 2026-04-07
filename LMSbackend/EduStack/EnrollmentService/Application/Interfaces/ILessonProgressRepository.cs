using EnrollmentService.Domain.Entities;

namespace EnrollmentService.Application.Interfaces;

public interface ILessonProgressRepository
{
    Task<LessonProgress?> GetByEnrollmentAndLessonAsync(Guid enrollmentId, Guid lessonId);
    Task AddAsync(LessonProgress progress);
    Task SaveChangesAsync();
}