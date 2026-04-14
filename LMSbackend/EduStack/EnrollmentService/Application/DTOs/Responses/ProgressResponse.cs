// ProgressResponse — Progress statistics for one enrollment.
// • Used by GetProgressAsync and MarkLessonCompleteAsync.
// • ProgressPercentage = (CompletedLessons / TotalLessons) * 100, rounded to 2 decimal places.
// • TotalLessons = count of LessonProgress records (only created when lessons are interacted with).

namespace EnrollmentService.Application.DTOs.Responses;

public class ProgressResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }

    public int TotalLessons { get; set; }       // how many lessons have been interacted with
    public int CompletedLessons { get; set; }   // how many of those are marked complete

    // e.g. 66.67 for "2 out of 3 lessons complete"
    public double ProgressPercentage { get; set; }

    public string Status { get; set; } = default!;  // enrollment status (Active/Completed)

    // Individual lesson completion records (for detailed progress display).
    public List<LessonProgressResponse> LessonProgresses { get; set; } = new();
}