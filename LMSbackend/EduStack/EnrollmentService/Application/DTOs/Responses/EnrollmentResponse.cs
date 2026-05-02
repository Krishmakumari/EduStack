// EnrollmentResponse — Lightweight enrollment summary (no lesson data).
// • Used for listing: GetMyEnrollments and GetEnrollmentsByCourse.
// • Status is string (not enum) for JSON serialization readability.
// • CompletedAt is nullable — null if still Active or Cancelled.

namespace EnrollmentService.Application.DTOs.Responses;

public class EnrollmentResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;  // denormalized from JWT at enrollment
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;  // denormalized from request at enrollment
    public int TotalLessons { get; set; }
    public decimal PricePaid { get; set; }
    public string Status { get; set; } = default!;       // "Active", "Completed", or "Cancelled"
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }            // null until course completed
}