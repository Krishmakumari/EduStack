// LessonRepository — EF Core implementation of ILessonRepository.
// • Straightforward CRUD — no eager loading needed (Lesson is a leaf entity).

using CourseService.Application.Interfaces;
using CourseService.Domain.Entities;
using CourseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly CourseDbContext _context;

    public LessonRepository(CourseDbContext context)
    {
        _context = context;
    }

    public async Task<Lesson?> GetByIdAsync(Guid lessonId)
        => await _context.Lessons
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);

    public async Task AddAsync(Lesson lesson)
        => await _context.Lessons.AddAsync(lesson);

    public Task DeleteAsync(Lesson lesson)
    {
        _context.Lessons.Remove(lesson);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}