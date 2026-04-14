// EnrollmentRepository — EF Core implementation for Enrollment persistence.
// • Two GetById variants: lightweight (no includes) and full (with LessonProgresses).
// • GetByStudentAndCourseAsync: used for duplicate enrollment check.
// • Ordered by EnrolledAt descending — most recent first in all list queries.

using EnrollmentService.Application.Interfaces;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnrollmentService.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly EnrollmentDbContext _context;

    public EnrollmentRepository(EnrollmentDbContext context)
    {
        _context = context;
    }

    // Lightweight load — no LessonProgresses included.
    // Used when you only need enrollment metadata (e.g., ownership check before delete).
    public async Task<Enrollment?> GetByIdAsync(Guid enrollmentId)
        => await _context.Enrollments
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

    // Full load — includes LessonProgresses via eager loading.
    // Required for: GetProgressAsync, MarkLessonCompleteAsync, GetEnrollmentByIdAsync.
    // The Include() tells EF Core to run a JOIN query and populate the collection.
    public async Task<Enrollment?> GetByIdWithProgressAsync(Guid enrollmentId)
        => await _context.Enrollments
            .Include(e => e.LessonProgresses)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

    // Duplicate enrollment check — used in EnrollAsync before creating a new enrollment.
    // Returns null if not enrolled (safe to proceed), or the existing enrollment (block).
    public async Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
        => await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                                   && e.CourseId == courseId);

    // "My Courses" list — all enrollments for one student, most recent first.
    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
        => await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)  // most recently enrolled first
            .ToListAsync();

    // Instructor view — all students in a course, most recent enrollment first.
    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
        => await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAt)  // most recently enrolled first
            .ToListAsync();

    // Marks entity as Added in EF change tracker — INSERT runs when SaveChangesAsync() is called.
    public async Task AddAsync(Enrollment enrollment)
        => await _context.Enrollments.AddAsync(enrollment);

    // Commits all pending changes to the database (INSERT/UPDATE/DELETE).
    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}