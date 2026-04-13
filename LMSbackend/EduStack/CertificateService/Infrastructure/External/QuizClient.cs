// QuizClient — HTTP client for cross-service communication with Quiz Service (stub).
// • Future use: verify the student passed the quiz before issuing a certificate.
// • Currently NOT wired into CertificateService.GenerateCertificateAsync().
// • Uses HttpClient injected via DI — in production, register with AddHttpClient<QuizClient>()
//   for proper connection pooling and resilience (Polly retry policies).

public class QuizClient
{
    private readonly HttpClient _httpClient;

    public QuizClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // HasUserPassed — calls Quiz Service to check if the student passed.
    // Returns true if the response is 2xx (success), false otherwise.
    // This is a simple "fire and check status" pattern — no response body parsing.
    public async Task<bool> HasUserPassed(Guid userId, Guid courseId)
    {
        var response = await _httpClient.GetAsync(
            $"api/quiz/result?userId={userId}&courseId={courseId}");

        return response.IsSuccessStatusCode;
    }
}