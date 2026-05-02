// IEnrollmentService — Contract for enrollment and progress operations (Application Layer).
// • Dependency Inversion: controller depends on this interface, not the concrete class.
// • studentId is always passed separately (from JWT) — not inferred from DTOs.
// • Two response types: EnrollmentResponse (lightweight list) vs EnrollmentDetailResponse (full).

using EnrollmentService.Application.DTOs.Requests;
using EnrollmentService.Application.DTOs.Responses;

namespace EnrollmentService.Application.Interfaces;

public interface IEnrollmentService
{
    // Enroll a student — checks for duplicates, creates enrollment in Active status.
    Task<EnrollmentResponse> EnrollAsync(Guid studentId, string studentName, string studentEmail, EnrollRequest request);

    // List all enrollments for the JWT student (lightweight response).
    Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(Guid studentId);

    // Get full enrollment detail with lesson progress — validates ownership.
    Task<EnrollmentDetailResponse> GetEnrollmentByIdAsync(Guid studentId, Guid enrollmentId);

    // Get all enrollments for a course — Instructor/Admin view.
    Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByCourseAsync(Guid courseId);

    // Mark a lesson as complete — upsert logic, idempotency guard, returns updated progress.
    Task<ProgressResponse> MarkLessonCompleteAsync(Guid studentId, Guid enrollmentId, Guid lessonId);

    // Get progress stats: total, completed, percentage — validates ownership.
    Task<ProgressResponse> GetProgressAsync(Guid studentId, Guid enrollmentId);

    // Cross-service helper: checks if a user is enrolled in a course.
    Task<bool> IsUserEnrolledAsync(Guid studentId, Guid courseId);

    // Get all enrollments across the system (Admin only).
    Task<IEnumerable<EnrollmentResponse>> GetAllEnrollmentsAsync();

    // Repair/Sync total lessons — used for existing records or when course length changes.
    Task SyncTotalLessonsAsync(Guid studentId, Guid enrollmentId, int totalLessons);
}