namespace LearningService.Domain.Exceptions;

public class UnauthorizedLessonAccessException : Exception
{
    public UnauthorizedLessonAccessException()
        : base("User is not enrolled in this course")
    {
    }
}