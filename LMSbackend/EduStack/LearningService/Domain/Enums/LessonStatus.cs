// LessonStatus Enum — Fine-grained status for a lesson's watch state.
// • Currently defined but not actively used in the LessonProgress entity
//   (which uses the simpler IsCompleted bool + 80% rule instead).
// • Could replace the bool in future for richer status display (e.g., "In Progress" badge).

namespace LearningService.Domain.Enums;

public enum LessonStatus
{
    NotStarted = 0,   // student has not watched any part of the lesson
    InProgress = 1,   // student has watched some but < 80%
    Completed  = 2    // student has watched >= 80% of the lesson
}