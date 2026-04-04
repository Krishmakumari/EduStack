namespace CourseService.Domain.Exceptions
{
    public class UnAuthorizedAccessException:DomainException
    {
        public UnAuthorizedAccessException():base("You are not authorized to modify this course") { }
    }
}
