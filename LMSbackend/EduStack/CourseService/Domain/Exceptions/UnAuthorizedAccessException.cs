namespace CourseService.Domain.Exceptions
{
    public class UnAuthorizedAccessException : UnauthorizedAccessException
    {
        public UnAuthorizedAccessException() : base("You are not authorized to modify this course") { }
    }
}
