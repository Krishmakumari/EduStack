// ILessonProgressRepository — Repository contract for LessonProgress persistence.
// • GetByEnrollmentAndLessonAsync: used for upsert logic — check if record exists first.

using EnrollmentService.Domain.Entities;

namespace EnrollmentService.Application.Interfaces;

public interface ILessonProgressRepository
{
    // Find existing progress for a specific lesson within an enrollment.
    // Returns null = first time (create new record). Returns record = update existing.
    Task<LessonProgress?> GetByEnrollmentAndLessonAsync(Guid enrollmentId, Guid lessonId);

    Task AddAsync(LessonProgress progress);
    Task SaveChangesAsync();
}