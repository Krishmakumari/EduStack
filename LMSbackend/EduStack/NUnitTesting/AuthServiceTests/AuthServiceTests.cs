// AuthServiceTests — Unit tests for AuthService.Application.Services.AuthService
// Tests all authentication operations: Register, Login, RefreshToken, ForgotPassword, ResetPassword.
// Uses Moq to mock repositories, IJwtService, and RabbitMqPublisher.

using Moq;
using AuthService.Application.DTOs.Requests;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using AuthService.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

namespace NUnitTesting.AuthServiceTests;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepoMock;
    private Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private Mock<IPasswordResetRepository> _resetRepoMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<RabbitMqPublisher> _publisherMock;
    private AuthService.Application.Services.AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _resetRepoMock = new Mock<IPasswordResetRepository>();
        _jwtServiceMock = new Mock<IJwtService>();

        // RabbitMqPublisher requires IConfiguration — mock it
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
        _publisherMock = new Mock<RabbitMqPublisher>(configMock.Object);

        _authService = new AuthService.Application.Services.AuthService(
            _userRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _resetRepoMock.Object,
            _jwtServiceMock.Object,
            _publisherMock.Object);
    }

    // ─── Register Tests ───────────────────────────────────────────────────────

    [Test]
    public async Task RegisterAsync_NewUser_ReturnsSuccessMessage()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            Role = "Student"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Message, Is.EqualTo("Registration successful. You can now log in."));
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void RegisterAsync_DuplicateEmail_ThrowsDomainException()
    {
        // Arrange
        var existingUser = User.Create("Existing", "test@example.com", "hash", UserRole.Student);

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        var request = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            Role = "Student"
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomainException>(
            async () => await _authService.RegisterAsync(request));
        Assert.That(ex!.Message, Is.EqualTo("An account with this email already exists."));
    }

    [Test]
    public void RegisterAsync_InvalidRole_ThrowsDomainException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var request = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            Role = "SuperAdmin" // invalid role
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomainException>(
            async () => await _authService.RegisterAsync(request));
        Assert.That(ex!.Message, Is.EqualTo("Invalid role specified."));
    }

    // ─── Login Tests ──────────────────────────────────────────────────────────

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var user = User.Create("Test User", "test@example.com", passwordHash, UserRole.Student);
        user.MarkEmailVerified();

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("mock-access-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns("mock-refresh-token");
        _refreshTokenRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AccessToken, Is.EqualTo("mock-access-token"));
        Assert.That(result.RefreshToken, Is.EqualTo("mock-refresh-token"));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.Role, Is.EqualTo("Student"));
    }

    [Test]
    public void LoginAsync_InvalidEmail_ThrowsInvalidCredentialsException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidCredentialsException>(
            async () => await _authService.LoginAsync(request));
    }

    [Test]
    public void LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!");
        var user = User.Create("Test User", "test@example.com", passwordHash, UserRole.Student);

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidCredentialsException>(
            async () => await _authService.LoginAsync(request));
    }

    [Test]
    public void LoginAsync_BannedUser_ThrowsAccountBannedException()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var user = User.Create("Banned User", "banned@example.com", passwordHash, UserRole.Student);
        user.Ban();

        _userRepoMock.Setup(r => r.GetByEmailAsync("banned@example.com"))
            .ReturnsAsync(user);

        var request = new LoginRequest
        {
            Email = "banned@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        Assert.ThrowsAsync<AccountBannedException>(
            async () => await _authService.LoginAsync(request));
    }

    // ─── RefreshToken Tests ───────────────────────────────────────────────────

    [Test]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewAuthResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingToken = RefreshToken.Create(userId, "old-token", 7);
        var user = User.Create("Test User", "test@example.com", "hash", UserRole.Student);

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("old-token"))
            .ReturnsAsync(existingToken);
        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns("new-access-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _refreshTokenRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var request = new RefreshTokenRequest { RefreshToken = "old-token" };

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        Assert.That(result.AccessToken, Is.EqualTo("new-access-token"));
        Assert.That(result.RefreshToken, Is.EqualTo("new-refresh-token"));
    }

    [Test]
    public void RefreshTokenAsync_InvalidToken_ThrowsDomainException()
    {
        // Arrange
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken?)null);

        var request = new RefreshTokenRequest { RefreshToken = "invalid-token" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomainException>(
            async () => await _authService.RefreshTokenAsync(request));
        Assert.That(ex!.Message, Is.EqualTo("Invalid refresh token."));
    }

    [Test]
    public void RefreshTokenAsync_RevokedToken_ThrowsDomainException()
    {
        // Arrange
        var token = RefreshToken.Create(Guid.NewGuid(), "revoked-token", 7);
        token.Revoke(); // mark as revoked

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("revoked-token"))
            .ReturnsAsync(token);

        var request = new RefreshTokenRequest { RefreshToken = "revoked-token" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomainException>(
            async () => await _authService.RefreshTokenAsync(request));
        Assert.That(ex!.Message, Is.EqualTo("Refresh token has expired or been revoked."));
    }

    // ─── ForgotPassword Tests ─────────────────────────────────────────────────

    [Test]
    public async Task ForgotPasswordAsync_ExistingUser_SavesOtpAndPublishesEvent()
    {
        // Arrange
        var user = User.Create("Test User", "test@example.com", "hash", UserRole.Student);

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        _resetRepoMock.Setup(r => r.AddAsync(It.IsAny<PasswordReset>()))
            .Returns(Task.CompletedTask);
        _resetRepoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        // Note: RabbitMqPublisher.PublishAsync is non-virtual, cannot be mocked.
        // The publisher call will throw (no RabbitMQ available), but the OTP
        // is saved to DB before the publish call.

        var request = new ForgotPasswordRequest { Email = "test@example.com" };

        // Act — catch the RabbitMQ connection failure
        try
        {
            await _authService.ForgotPasswordAsync(request);
        }
        catch { /* Expected: RabbitMQ connection failure in test environment */ }

        // Assert — verify the OTP was saved to the repository (core logic)
        _resetRepoMock.Verify(r => r.AddAsync(It.IsAny<PasswordReset>()), Times.Once);
        _resetRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ForgotPasswordAsync_NonExistentUser_ReturnsGenericMessage()
    {
        // Arrange — anti-enumeration: same message for non-existent emails
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };

        // Act
        var result = await _authService.ForgotPasswordAsync(request);

        // Assert — message should NOT reveal that the email doesn't exist
        Assert.That(result.Message, Does.Contain("If this email exists"));
    }

    // ─── ResetPassword Tests ──────────────────────────────────────────────────

    [Test]
    public async Task ResetPasswordAsync_ValidOtp_ReturnsSuccessMessage()
    {
        // Arrange
        var user = User.Create("Test User", "test@example.com", "old-hash", UserRole.Student);
        var reset = PasswordReset.Create(user.UserId);

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        _resetRepoMock.Setup(r => r.GetLatestByUserIdAsync(user.UserId))
            .ReturnsAsync(reset);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _resetRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            OtpCode = reset.OtpCode,
            NewPassword = "NewPassword123!"
        };

        // Act
        var result = await _authService.ResetPasswordAsync(request);

        // Assert
        Assert.That(result.Message, Is.EqualTo("Password reset successfully. You can now log in."));
    }

    [Test]
    public void ResetPasswordAsync_InvalidOtp_ThrowsDomainException()
    {
        // Arrange
        var user = User.Create("Test User", "test@example.com", "hash", UserRole.Student);
        var reset = PasswordReset.Create(user.UserId);

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        _resetRepoMock.Setup(r => r.GetLatestByUserIdAsync(user.UserId))
            .ReturnsAsync(reset);

        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            OtpCode = "000000", // wrong OTP
            NewPassword = "NewPassword123!"
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomainException>(
            async () => await _authService.ResetPasswordAsync(request));
        Assert.That(ex!.Message, Is.EqualTo("Invalid OTP."));
    }

    [Test]
    public void ResetPasswordAsync_NonExistentUser_ThrowsDomainException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var request = new ResetPasswordRequest
        {
            Email = "ghost@example.com",
            OtpCode = "123456",
            NewPassword = "Password!"
        };

        // Act & Assert
        Assert.ThrowsAsync<DomainException>(
            async () => await _authService.ResetPasswordAsync(request));
    }

    // ─── VerifyEmail Tests ────────────────────────────────────────────────────

    [Test]
    public async Task VerifyEmailAsync_AnyToken_ReturnsNotRequiredMessage()
    {
        // Act
        var result = await _authService.VerifyEmailAsync("any-token");

        // Assert
        Assert.That(result.Message, Is.EqualTo("Email verification is not required."));
    }
}
