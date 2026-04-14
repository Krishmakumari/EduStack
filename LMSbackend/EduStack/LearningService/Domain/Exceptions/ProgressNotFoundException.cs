// ProgressNotFoundException — Thrown when no LessonProgress record exists for a lesson.
// • Triggered by GetLessonProgressAsync when the student hasn't started this lesson yet.
// • Mapped to 404 Not Found in GlobalExceptionMiddleware.

namespace LearningService.Domain.Exceptions;

public class ProgressNotFoundException : Exception
{
    public ProgressNotFoundException(Guid lessonId)
        : base($"Progress not found for lesson: {lessonId}")
    {
    }
}