// MarkLessonCompleteRequest — Input for marking a lesson as completed.
// • Contains only LessonId — the enrollment ID comes from the route parameter.
// • Student's identity comes from the JWT, NOT the request body.

namespace EnrollmentService.Application.DTOs.Requests;

public class MarkLessonCompleteRequest
{
    public Guid LessonId { get; set; }   // which lesson to mark as complete
}