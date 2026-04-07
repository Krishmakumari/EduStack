using CourseService.Domain.Entities;

namespace CourseService.Application.Interfaces
{
    public interface ILessonRepository
    {
        Task<Lesson?> GetByIdAsync(Guid lessonId);
        Task AddAsync(Lesson lesson);
        Task DeleteAsync(Lesson lesson);
        Task SaveChangesAsync();
    }
}
