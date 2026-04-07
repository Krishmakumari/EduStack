using EnrollmentService.Application.DTOs.Requests;
using EnrollmentService.Application.DTOs.Responses;

namespace EnrollmentService.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponse> EnrollAsync(Guid studentId, string studentName, EnrollRequest request);
    Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(Guid studentId);
    Task<EnrollmentDetailResponse> GetEnrollmentByIdAsync(Guid studentId, Guid enrollmentId);
    Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByCourseAsync(Guid courseId);
    Task<ProgressResponse> MarkLessonCompleteAsync(Guid studentId, Guid enrollmentId, Guid lessonId);
    Task<ProgressResponse> GetProgressAsync(Guid studentId, Guid enrollmentId);
}