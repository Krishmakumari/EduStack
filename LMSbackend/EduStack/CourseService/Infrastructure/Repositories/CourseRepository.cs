// CourseRepository — EF Core implementation of ICourseRepository.
// • Two GetById variants: lightweight vs full eager-loaded tree.
// • Delete is synchronous (marks for deletion); actual SQL runs at SaveChangesAsync.

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

    // Returns all courses, newest first (for catalog display)
    public async Task<IEnumerable<Course>> GetAllAsync()
        => await _context.Courses
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    // Returns only this instructor's courses (for "My Courses" dashboard)
    public async Task<IEnumerable<Course>> GetByInstructorIdAsync(Guid instructorId)
        => await _context.Courses
            .Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    // Lightweight — no navigation properties loaded. Used for ownership checks.
    public async Task<Course?> GetByIdAsync(Guid courseId)
        => await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

    // Heavy — eager loads the full Course → Section → Lesson tree.
    // Used for detail view and publishing (needs Sections.Any() check).
    // We use AsSplitQuery() to avoid cartesian explosion for deep hierarchies.
    // OrderBy is removed from Include because it's handled in the Application Layer mapping.
    public async Task<Course?> GetByIdWithSectionsAsync(Guid courseId)
        => await _context.Courses
            .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

    public async Task AddAsync(Course course)
        => await _context.Courses.AddAsync(course);

    // Synchronous — just marks entity for deletion in EF change tracker.
    // Actual DELETE SQL runs when SaveChangesAsync() is called.
    public Task DeleteAsync(Course course)
    {
        _context.Courses.Remove(course);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}