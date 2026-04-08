using LearningService.Application.Interfaces;
using LearningService.Domain.Entities;
using LearningService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearningService.Infrastructure.Repositories;

public class ProgressRepository : IProgressRepository
{
    private readonly LearningDbContext _context;

    public ProgressRepository(LearningDbContext context)
    {
        _context = context;
    }

    public async Task<LessonProgress?> GetAsync(
        Guid userId,
        Guid courseId,
        Guid lessonId)
    {
        return await _context.LessonProgresses
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.CourseId == courseId &&
                x.LessonId == lessonId);
    }

    public async Task<List<LessonProgress>> GetByCourseAsync(
        Guid userId,
        Guid courseId)
    {
        return await _context.LessonProgresses
            .Where(x => x.UserId == userId && x.CourseId == courseId)
            .ToListAsync();
    }

    public async Task AddAsync(LessonProgress progress)
    {
        await _context.LessonProgresses.AddAsync(progress);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LessonProgress progress)
    {
        _context.LessonProgresses.Update(progress);
        await _context.SaveChangesAsync();
    }
}