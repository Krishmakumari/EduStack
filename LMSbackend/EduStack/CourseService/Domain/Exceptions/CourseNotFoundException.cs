// CourseNotFoundException — Thrown when a course ID doesn't exist in the database.
// • Extends DomainException; middleware maps it to 404 Not Found.

namespace CourseService.Domain.Exceptions
{
    public class CourseNotFoundException : DomainException
    {
        public CourseNotFoundException() : base("Course Not Found") { }
        
    }
    
}
