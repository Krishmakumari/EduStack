// IEnrollmentClient — Interface for cross-service enrollment verification.
// • Abstracts the HTTP call to Enrollment Service behind an interface.
// • Allows easy mocking in unit tests (inject a mock instead of real HTTP client).
// • EnrollmentClient class (Infrastructure layer) is the concrete implementation.

namespace LearningService.Application.Interfaces;

public interface IEnrollmentClient
{
    // Returns true if the user is enrolled in the specified course.
    // Returns false if not enrolled OR if Enrollment Service is unreachable (fail-safe).
    Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId);
}