using EnrollmentService.Application.Interfaces;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnrollmentService.Infrastructure.Repositories;

public class LessonProgressRepository : ILessonProgressRepository
{
    private readonly EnrollmentDbContext _context;

    public LessonProgressRepository(EnrollmentDbContext context)
    {
        _context = context;
    }

    public async Task<LessonProgress?> GetByEnrollmentAndLessonAsync(
        Guid enrollmentId, Guid lessonId)
        => await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId
                                   && p.LessonId == lessonId);

    public async Task AddAsync(LessonProgress progress)
        => await _context.LessonProgresses.AddAsync(progress);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}