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
