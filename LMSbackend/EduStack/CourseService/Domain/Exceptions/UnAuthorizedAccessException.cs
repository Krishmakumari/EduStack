// UnAuthorizedAccessException — Thrown when an instructor tries to modify another's course.
// • Extends System.UnauthorizedAccessException; middleware maps to 403 Forbidden.

namespace CourseService.Domain.Exceptions
{
    public class UnAuthorizedAccessException : UnauthorizedAccessException
    {
        public UnAuthorizedAccessException() : base("You are not authorized to modify this course") { }
    }
}
