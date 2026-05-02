// AdminServiceTests — Unit tests for AuthService.Application.Services.AdminService
// Tests admin operations: GetAllUsers, BanUser, UnbanUser, UpdateUserRole.

using Moq;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;

namespace NUnitTesting.AuthServiceTests;

[TestFixture]
public class AdminServiceTests
{
    private Mock<IUserRepository> _userRepoMock;
    private AuthService.Application.Services.AdminService _adminService;

    [SetUp]
    public void Setup()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _adminService = new AuthService.Application.Services.AdminService(_userRepoMock.Object);
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            User.Create("Alice", "alice@example.com", "hash1", UserRole.Student),
            User.Create("Bob", "bob@example.com", "hash2", UserRole.Instructor),
            User.Create("Charlie", "charlie@example.com", "hash3", UserRole.Admin)
        };

        _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = (await _adminService.GetAllUsersAsync()).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].FullName, Is.EqualTo("Alice"));
        Assert.That(result[1].Role, Is.EqualTo("Instructor"));
        Assert.That(result[2].Role, Is.EqualTo("Admin"));
    }

    [Test]
    public async Task BanUserAsync_ExistingUser_BansAndReturnsMessage()
    {
        // Arrange
        var user = User.Create("Bad Actor", "bad@example.com", "hash", UserRole.Student);

        _userRepoMock.Setup(r => r.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _adminService.BanUserAsync(user.UserId);

        // Assert
        Assert.That(result.Message, Does.Contain("banned"));
        Assert.That(user.IsBanned, Is.True);
    }

    [Test]
    public void BanUserAsync_NonExistentUser_ThrowsDomainException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        Assert.ThrowsAsync<DomainException>(
            async () => await _adminService.BanUserAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task UnbanUserAsync_ExistingUser_UnbansAndReturnsMessage()
    {
        // Arrange
        var user = User.Create("Reformed User", "reformed@example.com", "hash", UserRole.Student);
        user.Ban(); // first ban the user

        _userRepoMock.Setup(r => r.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _adminService.UnbanUserAsync(user.UserId);

        // Assert
        Assert.That(result.Message, Does.Contain("unbanned"));
        Assert.That(user.IsBanned, Is.False);
    }

    [Test]
    public async Task UpdateUserRoleAsync_ValidRole_UpdatesAndReturnsMessage()
    {
        // Arrange
        var user = User.Create("Student User", "student@example.com", "hash", UserRole.Student);

        _userRepoMock.Setup(r => r.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _adminService.UpdateUserRoleAsync(user.UserId, "Instructor");

        // Assert
        Assert.That(result.Message, Does.Contain("Instructor"));
        Assert.That(user.Role, Is.EqualTo(UserRole.Instructor));
    }

    [Test]
    public void UpdateUserRoleAsync_InvalidRole_ThrowsDomainException()
    {
        // Arrange
        var user = User.Create("User", "user@example.com", "hash", UserRole.Student);
        _userRepoMock.Setup(r => r.GetByIdAsync(user.UserId)).ReturnsAsync(user);

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomainException>(
            async () => await _adminService.UpdateUserRoleAsync(user.UserId, "SuperAdmin"));
        Assert.That(ex!.Message, Is.EqualTo("Invalid role specified."));
    }
}
