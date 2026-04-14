// LessonProgress Entity — Tracks whether a student has completed a specific lesson.
// • Created lazily — only when a student first marks a lesson complete (no pre-seeded records).
// • Composite unique index (EnrollmentId + LessonId) prevents duplicate completion records.
// • Private setters enforce state changes only through MarkCompleted() domain method.

namespace EnrollmentService.Domain.Entities;

public class LessonProgress
{
    public Guid LessonProgressId { get; private set; }  // PK

    public Guid EnrollmentId { get; private set; }       // FK → Enrollment (cascade delete)
    public Guid LessonId { get; private set; }           // from Course/Learning Service

    public bool IsCompleted { get; private set; }        // false until MarkCompleted() called
    public DateTime? CompletedAt { get; private set; }   // null until completed

    // Navigation property — EF Core uses this to link back to the parent Enrollment.
    public Enrollment Enrollment { get; private set; } = default!;

    // Private constructor — all creation goes through the Create() factory method.
    private LessonProgress() { }

    // Factory method — creates a LessonProgress record in the "not yet complete" state.
    // Caller must call MarkCompleted() to actually record completion.
    public static LessonProgress Create(Guid enrollmentId, Guid lessonId)
    {
        return new LessonProgress
        {
            LessonProgressId = Guid.NewGuid(),
            EnrollmentId = enrollmentId,
            LessonId = lessonId,
            IsCompleted = false  // explicitly starts as not complete
        };
    }

    // MarkCompleted — transitions lesson to completed state.
    // Sets both the flag and the timestamp atomically (both always change together).
    public void MarkCompleted()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }
}