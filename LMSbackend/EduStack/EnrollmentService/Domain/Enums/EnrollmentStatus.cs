// EnrollmentStatus Enum — Lifecycle states for a student's course enrollment.
// • Stored as STRING in DB (HasConversion<string>()) for SQL readability.
// • Active: student is currently enrolled and learning.
// • Completed: student has finished the course (all lessons/requirements done).
// • Cancelled: student withdrew or was unenrolled (record kept for history).

namespace EnrollmentService.Domain.Enums;

public enum EnrollmentStatus
{
    Active,     // default state when enrollment is created
    Completed,  // set by MarkCompleted() domain method
    Cancelled   // set by Cancel() domain method
}