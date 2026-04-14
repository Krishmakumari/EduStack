// NotificationException — Base exception for notification delivery failures.
// • Currently defined but not actively thrown in the codebase.
// • Intended for wrapping: SMTP failures, RabbitMQ connection errors, etc.
// • Production usage: wrap Email/RabbitMQ exceptions and add retry logic.

namespace NotificationService.Domain.Exceptions;

public class NotificationException : Exception
{
    public NotificationException(string message) : base(message)
    {
    }
}
