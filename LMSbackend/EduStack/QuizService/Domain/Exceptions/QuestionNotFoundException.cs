// QuestionNotFoundException — Thrown when an answer references an invalid Question ID.

namespace QuizService.Domain.Exceptions;

public class QuestionNotFoundException : Exception
{
    public QuestionNotFoundException(Guid questionId)
        : base($"Question matching ID {questionId} not found in this quiz.")
    {
    }
}