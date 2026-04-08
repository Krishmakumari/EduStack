namespace LearningService.Domain.Exceptions;

public class ProgressNotFoundException : Exception
{
    public ProgressNotFoundException(Guid lessonId)
        : base($"Progress not found for lesson: {lessonId}")
    {
    }
}