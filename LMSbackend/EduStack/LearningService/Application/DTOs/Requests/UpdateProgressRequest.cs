// UpdateProgressRequest — Input DTO for updating lesson watch progress.
// • Called frequently by the video player (e.g., every 30 seconds while watching).
// • WatchedSeconds: current watch position (not cumulative — overwritten on each update).
// • TotalDurationSeconds: used to calculate the 80% completion threshold.

namespace LearningService.Application.DTOs.Requests;

public class UpdateProgressRequest
{
    public Guid CourseId { get; set; }            // which course the lesson belongs to
    public Guid LessonId { get; set; }            // which lesson is being watched

    // Current watch position in seconds from the video player.
    // Overwritten on each call — not summed. Represents "furthest point watched".
    public int WatchedSeconds { get; set; }

    // Full duration of the video lesson in seconds.
    // Used to calculate: IsCompleted = (WatchedSeconds / TotalDurationSeconds >= 0.80).
    public int TotalDurationSeconds { get; set; }
}