// SectionRepository — EF Core implementation of ISectionRepository.
// • GetByIdAsync eager-loads Lessons so responses include nested lesson data.

using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using CourseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Repositories;

public class SectionRepository : ISectionRepository
{
    private readonly CourseDbContext _context;

    public SectionRepository(CourseDbContext context)
    {
        _context = context;
    }

    public async Task<Section?> GetByIdAsync(Guid sectionId)
        => await _context.Sections
            .Include(s => s.Lessons)
            .FirstOrDefaultAsync(s => s.SectionId == sectionId);

    public async Task AddAsync(Section section)
        => await _context.Sections.AddAsync(section);

    public Task DeleteAsync(Section section)
    {
        _context.Sections.Remove(section);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}