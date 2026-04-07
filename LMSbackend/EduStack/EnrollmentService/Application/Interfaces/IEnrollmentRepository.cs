using EnrollmentService.Domain.Entities;

namespace EnrollmentService.Application.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid enrollmentId);
    Task<Enrollment?> GetByIdWithProgressAsync(Guid enrollmentId);
    Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId);
    Task AddAsync(Enrollment enrollment);
    Task SaveChangesAsync();
}