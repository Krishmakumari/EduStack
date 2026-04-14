// LessonProgressRepository — EF Core implementation for LessonProgress persistence.
// • GetByEnrollmentAndLessonAsync: used for upsert logic in MarkLessonCompleteAsync.
// • Composite lookup (EnrollmentId + LessonId) matches the unique DB index.

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

    // Find an existing progress record for a specific lesson within an enrollment.
    // Returns null if this lesson hasn't been interacted with yet (first time = create new).
    // Returns the record if it exists (may or may not be completed = check IsCompleted).
    public async Task<LessonProgress?> GetByEnrollmentAndLessonAsync(
        Guid enrollmentId, Guid lessonId)
        => await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId
                                   && p.LessonId == lessonId);

    // Marks entity as Added in EF change tracker — INSERT runs on SaveChangesAsync.
    public async Task AddAsync(LessonProgress progress)
        => await _context.LessonProgresses.AddAsync(progress);

    // Commits all pending changes (INSERT new records, UPDATE existing ones).
    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}