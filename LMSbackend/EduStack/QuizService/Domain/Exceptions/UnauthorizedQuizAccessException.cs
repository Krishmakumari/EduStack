// UnauthorizedQuizAccessException — Thrown when a student attempts to submit an attempt they don't own.

namespace QuizService.Domain.Exceptions;

public class UnauthorizedQuizAccessException : Exception
{
    public UnauthorizedQuizAccessException() : base("You are not authorized to access this quiz attempt.")
    {
    }
}