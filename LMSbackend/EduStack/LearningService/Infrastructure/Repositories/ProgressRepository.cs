// ProgressRepository — EF Core implementation for LessonProgress persistence.
// • GetAsync: 3-key composite lookup matching the unique DB index.
// • GetByCourseAsync: loads all lessons for a user+course (used for course progress calc).
// • AddAsync and UpdateAsync both call SaveChangesAsync internally (atomic per call).

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

    // Finds the LessonProgress record for one specific user + course + lesson.
    // Returns null if the student has never started this lesson (first watch).
    // The 3-key filter matches the unique composite DB index for fast lookups.
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

    // Loads ALL lesson progress records for one user in one course.
    // Used by GetCourseProgressAsync to count total/completed lessons.
    public async Task<List<LessonProgress>> GetByCourseAsync(
        Guid userId,
        Guid courseId)
    {
        return await _context.LessonProgresses
            .Where(x => x.UserId == userId && x.CourseId == courseId)
            .ToListAsync();
    }

    // Adds a new LessonProgress record (first time watching a lesson).
    // Saves immediately — each video update is committed atomically.
    public async Task AddAsync(LessonProgress progress)
    {
        await _context.LessonProgresses.AddAsync(progress);
        await _context.SaveChangesAsync();
    }

    // Updates an existing LessonProgress record (student resuming/re-watching).
    // EF Update() marks all properties as modified — doesn't diff individual fields.
    public async Task UpdateAsync(LessonProgress progress)
    {
        _context.LessonProgresses.Update(progress);
        await _context.SaveChangesAsync();
    }
}