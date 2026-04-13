// ICourseRepository — Data access contract for Course entities.
// • Two GetById variants: lightweight (ownership check) vs full tree (detail view).
// • Lives in Application layer so business logic doesn't depend on EF Core.

using CourseService.Domain.Entities;

namespace CourseService.Application.Interfaces
{
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetAllAsync();
        Task<IEnumerable<Course>> GetByInstructorIdAsync(Guid instructorId);
        Task<Course?> GetByIdAsync(Guid courseId);
        Task<Course?> GetByIdWithSectionsAsync(Guid courseId);
        Task AddAsync(Course course);
        Task DeleteAsync(Course course);
        Task SaveChangesAsync();
    }
}
