using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using CourseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly CourseDbContext _context;

    public CourseRepository(CourseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
        => await _context.Courses
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Course>> GetByInstructorIdAsync(Guid instructorId)
        => await _context.Courses
            .Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Course?> GetByIdAsync(Guid courseId)
        => await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

    public async Task<Course?> GetByIdWithSectionsAsync(Guid courseId)
        => await _context.Courses
            .Include(c => c.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.Order))
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

    public async Task AddAsync(Course course)
        => await _context.Courses.AddAsync(course);

    public Task DeleteAsync(Course course)
    {
        _context.Courses.Remove(course);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}