// QuizNotFoundException — Thrown when attempting to start/submit a quiz that doesn't exist.

namespace QuizService.Domain.Exceptions;

public class QuizNotFoundException : Exception
{
    public QuizNotFoundException() : base("Quiz not found.")
    {
    }
}