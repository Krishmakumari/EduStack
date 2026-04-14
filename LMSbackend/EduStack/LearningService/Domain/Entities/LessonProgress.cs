// LessonProgress Entity — Tracks a student's video watch position per lesson.
// • One record per (UserId + CourseId + LessonId) combination.
// • WatchedSeconds = current furthest position (not cumulative — overwritten on update).
// • IsCompleted = true when WatchedSeconds >= 80% of TotalDurationSeconds.
// • Public setters (no DDD factory) — simple data record, no lifecycle rules.

namespace LearningService.Domain.Entities;

public class LessonProgress
{
    public Guid Id { get; set; }          // PK — unique per record

    public Guid UserId { get; set; }      // which student (from JWT)
    public Guid CourseId { get; set; }    // which course
    public Guid LessonId { get; set; }    // which lesson within the course

    // Current watch position in seconds. Overwritten each update (not summed).
    // Video player can use this to resume from where the student stopped.
    public int WatchedSeconds { get; set; } = 0;

    // Set to true when WatchedSeconds / TotalDurationSeconds >= 0.80 (80% rule).
    public bool IsCompleted { get; set; } = false;

    // When the student last accessed this lesson — useful for sorting "continue watching".
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
}