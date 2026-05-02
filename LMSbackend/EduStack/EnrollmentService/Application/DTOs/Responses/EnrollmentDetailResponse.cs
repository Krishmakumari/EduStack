// EnrollmentDetailResponse — Full enrollment including all lesson progress records.
// • Used by GetEnrollmentByIdAsync — the detail view (student clicks on a course).
// • Extends EnrollmentResponse data with LessonProgresses list.

namespace EnrollmentService.Application.DTOs.Responses;

public class EnrollmentDetailResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;
    public int TotalLessons { get; set; }
    public decimal PricePaid { get; set; }
    public string Status { get; set; } = default!;
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // List of all lessons the student has interacted with — only completed ones present.
    // Empty list = no lessons marked complete yet (lazy creation of progress records).
    public List<LessonProgressResponse> LessonProgresses { get; set; } = new();
}