namespace QuizService.Domain.Exceptions;

public class QuizSubmissionException : Exception
{
    public QuizSubmissionException(string message)
        : base(message)
    {
    }
}