// IProgressRepository — Repository contract for LessonProgress persistence.
// • GetAsync: 3-key composite lookup — finds one lesson's progress record.
// • GetByCourseAsync: loads all lesson records for a user in a course (for course stats).
// • UpdateAsync: separate method from AddAsync — EF Update() vs Add() semantics differ.

using LearningService.Domain.Entities;

namespace LearningService.Application.Interfaces;

public interface IProgressRepository
{
    // Find progress for one specific user+course+lesson. Returns null = hasn't started.
    Task<LessonProgress?> GetAsync(Guid userId, Guid courseId, Guid lessonId);

    // Get ALL lesson progress records for a user within a course.
    // Used by GetCourseProgressAsync to count total/completed lessons.
    Task<List<LessonProgress>> GetByCourseAsync(Guid userId, Guid courseId);

    // INSERT new record (first time watching a lesson).
    Task AddAsync(LessonProgress progress);

    // UPDATE existing record (student resuming or re-watching).
    Task UpdateAsync(LessonProgress progress);
}