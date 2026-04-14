// CourseProgressResponse — Output for overall course completion statistics.
// • Calculated in memory from LessonProgress records — NOT stored in DB.
// • CompletionPercentage = (CompletedLessons / TotalLessons) * 100.
// • TotalLessons = count of lessons student has interacted with (not total in course).

namespace LearningService.Application.DTOs.Responses;

public class CourseProgressResponse
{
    public Guid CourseId { get; set; }

    // Count of LessonProgress records for this user+course (engaged lessons only).
    public int TotalLessons { get; set; }

    // Count of those records where IsCompleted == true (>= 80% watched).
    public int CompletedLessons { get; set; }

    // e.g., 66.67 for "2 out of 3 engaged lessons completed".
    public double CompletionPercentage { get; set; }
}