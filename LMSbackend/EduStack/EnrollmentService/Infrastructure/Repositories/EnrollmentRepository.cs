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

    public async Task<Enrollment?> GetByIdAsync(Guid enrollmentId)
        => await _context.Enrollments
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

    public async Task<Enrollment?> GetByIdWithProgressAsync(Guid enrollmentId)
        => await _context.Enrollments
            .Include(e => e.LessonProgresses)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

    public async Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
        => await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                                   && e.CourseId == courseId);

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
        => await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
        => await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

    public async Task AddAsync(Enrollment enrollment)
        => await _context.Enrollments.AddAsync(enrollment);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}