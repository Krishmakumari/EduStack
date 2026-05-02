// IEnrollmentRepository — Repository contract for Enrollment persistence.
// • Two GetById variants: lightweight (fast) vs full with LessonProgresses (eager loaded).
// • GetByStudentAndCourseAsync: used for duplicate enrollment check before creation.

using EnrollmentService.Domain.Entities;

namespace EnrollmentService.Application.Interfaces;

public interface IEnrollmentRepository
{
    // Lightweight load — no LessonProgresses. Fast for ownership checks.
    Task<Enrollment?> GetByIdAsync(Guid enrollmentId);

    // Full load — includes LessonProgresses via .Include(). Use when progress data needed.
    Task<Enrollment?> GetByIdWithProgressAsync(Guid enrollmentId);

    // Composite lookup — returns null if not enrolled, existing record if already enrolled.
    Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);

    // All enrollments for a student, ordered by most recent.
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId);

    // All enrollments for a course (instructor view), ordered by most recent.
    Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId);

    // All enrollments across the system.
    Task<IEnumerable<Enrollment>> GetAllAsync();

    Task AddAsync(Enrollment enrollment);
    Task SaveChangesAsync();
}