namespace QuizService.Domain.Exceptions;

public class QuizNotFoundException : Exception
{
    public QuizNotFoundException(Guid quizId)
        : base($"Quiz with ID {quizId} was not found.")
    {
    }
}