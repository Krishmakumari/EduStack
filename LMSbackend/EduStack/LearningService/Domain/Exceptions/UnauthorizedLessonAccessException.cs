// UnauthorizedLessonAccessException — Thrown when a user tries to update progress
// for a course they are NOT enrolled in.
// • Triggered by EnrollmentClient.IsUserEnrolledAsync() returning false.
// • Mapped to 403 Forbidden in GlobalExceptionMiddleware.
// • Note: 403 (not 401) — user IS authenticated, just not authorized for this course.

namespace LearningService.Domain.Exceptions;

public class UnauthorizedLessonAccessException : Exception
{
    public UnauthorizedLessonAccessException()
        : base("User is not enrolled in this course")
    {
    }
}