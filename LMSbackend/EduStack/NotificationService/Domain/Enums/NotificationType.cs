// NotificationType Enum — Categorizes what triggered the notification.
// • Used on the Notification entity for filtering, reporting, and history queries.
// • Maps to the 3 RabbitMQ queue events + OTP (planned for Auth Service).

namespace NotificationService.Domain.Enums;

public enum NotificationType
{
    OTP                  = 1,   // one-time password email (Auth Service — planned)
    CourseEnrollment     = 2,   // student enrolled in a course (enrollment_queue)
    CertificateGenerated = 3,   // student earned a certificate (certificate_queue)
    QuizResult           = 4    // quiz graded (quiz_queue)
}