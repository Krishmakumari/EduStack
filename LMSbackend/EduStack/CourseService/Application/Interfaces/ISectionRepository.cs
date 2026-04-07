using CourseService.Domain.Entities;

namespace CourseService.Application.Interfaces
{
    public interface ISectionRepository
    {
        Task<Section?> GetByIdAsync(Guid sectionId);
        Task AddAsync(Section section);
        Task DeleteAsync(Section section);
        Task SaveChangesAsync();
    }
}
