// UserRole Enum — Authorization levels across the entire system.
// • Stored as STRING in DB (not int) for readability.
// • Embedded in JWT "role" claim → read by [Authorize(Roles)] on all services.

namespace AuthService.Domain.Enums;

public enum UserRole
{
    Student,        // Can enroll, learn, take quizzes, get certificates
    Instructor,     // Can create courses, sections, lessons, quizzes
    Admin           // Full access — can manage all courses, ban users, view all payments
}