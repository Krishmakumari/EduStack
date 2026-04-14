// QuizSubmissionException — Business rule violation during submission (e.g. already submitted).

namespace QuizService.Domain.Exceptions;

public class QuizSubmissionException : Exception
{
    public QuizSubmissionException(string message) : base(message)
    {
    }
}