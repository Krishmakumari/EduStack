// EnrollmentCompletedEvent — RabbitMQ message contract for the enrollment_queue.
// • Published by EnrollmentService when a student successfully enrolls in a course.
// • Email included directly (event-carried state transfer) — no Auth Service call needed.

namespace NotificationService.Domain.Events;

public class EnrollmentCompletedEvent
{
    public Guid UserId { get; set; }        // student who enrolled
    public string Email { get; set; }       // where to send the confirmation
    public string CourseTitle { get; set; }  // which course — included in email body
}
