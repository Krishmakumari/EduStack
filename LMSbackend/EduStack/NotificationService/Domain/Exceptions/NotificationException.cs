namespace NotificationService.Domain.Exceptions;

public class NotificationException : Exception
{
    public NotificationException(string message) : base(message)
    {
    }
}
