namespace QuizService.Domain.Exceptions;

public class UnauthorizedQuizAccessException : Exception
{
    public UnauthorizedQuizAccessException()
        : base("User is not authorized to take this quiz.")
    {
    }
}