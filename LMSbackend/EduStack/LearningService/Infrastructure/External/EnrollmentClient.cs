using LearningService.Application.Interfaces;
using System.Net.Http.Json;

namespace LearningService.Infrastructure.External;

public class EnrollmentClient : IEnrollmentClient
{
    private readonly HttpClient _httpClient;

    public EnrollmentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId)
    {
        var response = await _httpClient.GetAsync(
            $"api/enrollments/check?userId={userId}&courseId={courseId}");

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<bool>();
        return result;
    }
}