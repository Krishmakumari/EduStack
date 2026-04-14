// EnrollmentClient — HTTP client for cross-service enrollment verification.
// • Calls Enrollment Service's check endpoint before allowing progress updates.
// • Uses typed HttpClient (IHttpClientFactory) for automatic connection pooling.
// • BaseAddress configured in Program.cs from appsettings["Services:EnrollmentService"].
// • Returns false (not true) on HTTP errors — fail-safe: deny access if service is unreachable.

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

    // IsUserEnrolledAsync — asks Enrollment Service if the user is enrolled in the course.
    // Returns true if Enrollment Service responds with 2xx + body=true.
    // Returns false if the call fails OR the response body is false.
    // WHY: Learning Service should NOT duplicate enrollment data — it queries the owner service.
    public async Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId)
    {
        var response = await _httpClient.GetAsync(
            $"api/enrollments/check?userId={userId}&courseId={courseId}");

        if (!response.IsSuccessStatusCode)
            return false;   // fail-safe: if Enrollment Service is down, deny access

        // Deserialize the boolean response body from Enrollment Service.
        var result = await response.Content.ReadFromJsonAsync<bool>();
        return result;
    }
}