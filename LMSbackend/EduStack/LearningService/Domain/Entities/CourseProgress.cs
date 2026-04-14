// CourseProgress — View model for overall course completion statistics.
// • This is NOT stored in the database — it is calculated in memory.
// • Built by counting LessonProgress records for a given user + course.
// • CompletionPercentage is a C# computed property (expression-bodied getter),
//   not a DB column — no redundant storage needed.

namespace LearningService.Domain.Entities;

public class CourseProgress
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }

    public int TotalLessons { get; set; }       // count of LessonProgress records for this course
    public int CompletedLessons { get; set; }   // count where IsCompleted == true

    // Computed property — auto-calculates from TotalLessons and CompletedLessons.
    // Division-by-zero guard: returns 0 when TotalLessons == 0 (no lessons tracked yet).
    public double CompletionPercentage =>
        TotalLessons == 0 ? 0 : (double)CompletedLessons / TotalLessons * 100;
}