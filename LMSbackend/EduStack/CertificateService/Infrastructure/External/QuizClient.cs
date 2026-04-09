public class QuizClient
{
    private readonly HttpClient _httpClient;

    public QuizClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> HasUserPassed(Guid userId, Guid courseId)
    {
        var response = await _httpClient.GetAsync(
            $"api/quiz/result?userId={userId}&courseId={courseId}");

        return response.IsSuccessStatusCode;
    }
}