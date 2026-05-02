// UserEntityTests — Unit tests for the User domain entity.
// Tests factory method, domain methods, and computed properties.

using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace NUnitTesting.AuthServiceTests;

[TestFixture]
public class UserEntityTests
{
    [Test]
    public void Create_SetsDefaultValues()
    {
        // Act
        var user = User.Create("John Doe", "John@Example.COM", "hash", UserRole.Student);

        // Assert
        Assert.That(user.UserId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(user.FullName, Is.EqualTo("John Doe"));
        Assert.That(user.Email, Is.EqualTo("john@example.com")); // lowercase
        Assert.That(user.PasswordHash, Is.EqualTo("hash"));
        Assert.That(user.Role, Is.EqualTo(UserRole.Student));
        Assert.That(user.IsEmailVerified, Is.False);
        Assert.That(user.IsBanned, Is.False);
        Assert.That(user.TwoFactorEnabled, Is.False);
    }

    [Test]
    public void MarkEmailVerified_SetsFlag()
    {
        var user = User.Create("Test", "t@t.com", "h", UserRole.Student);
        user.MarkEmailVerified();
        Assert.That(user.IsEmailVerified, Is.True);
    }

    [Test]
    public void Ban_And_Unban_TogglesFlag()
    {
        var user = User.Create("Test", "t@t.com", "h", UserRole.Student);

        user.Ban();
        Assert.That(user.IsBanned, Is.True);

        user.Unban();
        Assert.That(user.IsBanned, Is.False);
    }

    [Test]
    public void RecordLogin_SetsLastLoginAt()
    {
        var user = User.Create("Test", "t@t.com", "h", UserRole.Student);
        Assert.That(user.LastLoginAt, Is.Null);

        user.RecordLogin();
        Assert.That(user.LastLoginAt, Is.Not.Null);
    }

    [Test]
    public void UpdatePasswordHash_ChangesHash()
    {
        var user = User.Create("Test", "t@t.com", "old-hash", UserRole.Student);
        user.UpdatePasswordHash("new-hash");
        Assert.That(user.PasswordHash, Is.EqualTo("new-hash"));
    }

    [Test]
    public void UpdateRole_ChangesRole()
    {
        var user = User.Create("Test", "t@t.com", "h", UserRole.Student);
        user.UpdateRole(UserRole.Instructor);
        Assert.That(user.Role, Is.EqualTo(UserRole.Instructor));
    }

    [Test]
    public void EnableTwoFactor_SetsSecretAndFlag()
    {
        var user = User.Create("Test", "t@t.com", "h", UserRole.Student);
        user.EnableTwoFactor("secret123");

        Assert.That(user.TwoFactorEnabled, Is.True);
        Assert.That(user.TwoFactorSecret, Is.EqualTo("secret123"));
    }

    [Test]
    public void DisableTwoFactor_ClearsSecretAndFlag()
    {
        var user = User.Create("Test", "t@t.com", "h", UserRole.Student);
        user.EnableTwoFactor("secret123");
        user.DisableTwoFactor();

        Assert.That(user.TwoFactorEnabled, Is.False);
        Assert.That(user.TwoFactorSecret, Is.Null);
    }
}
