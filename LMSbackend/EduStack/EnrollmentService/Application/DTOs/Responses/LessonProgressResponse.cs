// LessonProgressResponse — Single lesson completion status.
// • Included in EnrollmentDetailResponse and ProgressResponse.
// • CompletedAt is nullable — null means the record exists but isn't complete yet.

namespace EnrollmentService.Application.DTOs.Responses;

public class LessonProgressResponse
{
    public Guid LessonProgressId { get; set; }   // the progress record's own ID
    public Guid LessonId { get; set; }            // which lesson (from Course Service)
    public bool IsCompleted { get; set; }         // true if student completed this lesson
    public DateTime? CompletedAt { get; set; }    // null if not completed yet
}