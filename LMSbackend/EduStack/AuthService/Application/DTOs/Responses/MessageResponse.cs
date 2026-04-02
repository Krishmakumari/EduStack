namespace AuthService.Application.DTOs.Responses;

public class MessageResponse
{
    public string Message { get; set; } = default!;
    public MessageResponse(string message) => Message = message;
}