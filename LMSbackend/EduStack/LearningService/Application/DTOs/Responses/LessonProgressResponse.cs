// LessonProgressResponse — Output for one lesson's watch progress.
// • ProgressPercentage is currently always 0 — placeholder for future calculation.
//   (Would need TotalDurationSeconds to be stored in the DB to calculate here.)

namespace LearningService.Application.DTOs.Responses;

public class LessonProgressResponse
{
    public Guid LessonId { get; set; }

    public int WatchedSeconds { get; set; }   // how far the student has watched
    public bool IsCompleted { get; set; }     // true if >= 80% has been watched

    // Placeholder — can be calculated as (WatchedSeconds / TotalDurationSeconds * 100)
    // if TotalDurationSeconds is stored on the LessonProgress entity in a future update.
    public double ProgressPercentage { get; set; }
}