// EnrollRequest — Input DTO for course enrollment.
// • CourseTitle is passed in the request (denormalized) to avoid calling Course Service.
// • PricePaid captures the amount at time of enrollment — stored for payment history.

namespace EnrollmentService.Application.DTOs.Requests;

public class EnrollRequest
{
    public Guid CourseId { get; set; }            // which course to enroll in
    public string CourseTitle { get; set; } = default!;  // denormalized — stored on enrollment
    public int TotalLessons { get; set; }         // denormalized — needed for progress calculation
    public decimal PricePaid { get; set; }        // amount paid at enrollment time
}