namespace LearningService.Application.Interfaces;

public interface IEnrollmentClient
{
    Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId);
}