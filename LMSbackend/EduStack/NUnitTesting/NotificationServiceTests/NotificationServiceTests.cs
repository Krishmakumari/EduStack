// NotificationServiceTests — Unit tests for the NotificationService domain & DTOs.
// Note: NotificationAppService delegates to EmailService.SendEmailAsync (non-virtual),
// which cannot be mocked. We test the DTOs, entity, and enum structure instead.
// Full email sending is an integration test concern.

using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NUnitTesting.NotificationServiceTests;

[TestFixture]
public class NotificationDtoTests
{
    [Test]
    public void SendEmailDto_PropertiesSetCorrectly()
    {
        var dto = new SendEmailDto
        {
            ToEmail = "user@example.com",
            Subject = "Welcome!",
            Message = "Welcome to EduStack."
        };

        Assert.That(dto.ToEmail, Is.EqualTo("user@example.com"));
        Assert.That(dto.Subject, Is.EqualTo("Welcome!"));
        Assert.That(dto.Message, Is.EqualTo("Welcome to EduStack."));
    }

    [Test]
    public void SendEmailDto_DefaultValues_AreEmptyStrings()
    {
        var dto = new SendEmailDto();

        Assert.That(dto.ToEmail, Is.EqualTo(string.Empty));
        Assert.That(dto.Subject, Is.EqualTo(string.Empty));
        Assert.That(dto.Message, Is.EqualTo(string.Empty));
    }
}

[TestFixture]
public class NotificationEntityTests
{
    [Test]
    public void Notification_PropertiesSetCorrectly()
    {
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            ToEmail = "user@example.com",
            Subject = "OTP Code",
            Message = "Your OTP is 123456",
            Type = NotificationType.OTP,
            CreatedAt = DateTime.UtcNow
        };

        Assert.That(notification.NotificationId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(notification.ToEmail, Is.EqualTo("user@example.com"));
        Assert.That(notification.Subject, Is.EqualTo("OTP Code"));
        Assert.That(notification.Type, Is.EqualTo(NotificationType.OTP));
    }

    [Test]
    public void NotificationType_ContainsExpectedValues()
    {
        // Verify that the notification type enum has all expected values
        Assert.That(Enum.IsDefined(typeof(NotificationType), "OTP"), Is.True);
        Assert.That(Enum.IsDefined(typeof(NotificationType), "CourseEnrollment"), Is.True);
        Assert.That(Enum.IsDefined(typeof(NotificationType), "QuizResult"), Is.True);
        Assert.That(Enum.IsDefined(typeof(NotificationType), "CertificateGenerated"), Is.True);
    }
}
