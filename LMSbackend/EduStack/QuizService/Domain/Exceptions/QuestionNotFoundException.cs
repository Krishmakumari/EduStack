namespace QuizService.Domain.Exceptions;

public class QuestionNotFoundException : Exception
{
    public QuestionNotFoundException(Guid questionId)
        : base($"Question with ID {questionId} was not found.")
    {
    }
}