namespace CourseService.Domain.Exceptions
{
    public class CourseNotFoundException : DomainException
    {
        public CourseNotFoundException() : base("Course Not Found") { }
        
    }
    
}
