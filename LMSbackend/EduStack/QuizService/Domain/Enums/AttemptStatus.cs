// AttemptStatus Enum — Tracks progress through the quiz lifecycle.
// • InProgress: Student started but hasn't submitted yet.
// • Passed/Failed: Terminal states calculated during submission based on PassingScore.

namespace QuizService.Domain.Enums;

public enum AttemptStatus
{
    InProgress,
    Passed,
    Failed
}