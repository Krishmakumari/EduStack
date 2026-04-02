using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User
{
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsBanned { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<ExternalLogin> ExternalLogins { get; private set; } = new List<ExternalLogin>();

    private User() { }

    public static User Create(string fullName, string email, string passwordHash, UserRole role)
    {
        return new User
        {
            UserId = Guid.NewGuid(),
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            IsEmailVerified = false,
            IsBanned = false,
            TwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkEmailVerified() => IsEmailVerified = true;
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
    public void Ban() => IsBanned = true;
    public void Unban() => IsBanned = false;
    public void UpdatePasswordHash(string newHash) => PasswordHash = newHash;
    public void EnableTwoFactor(string secret) { TwoFactorEnabled = true; TwoFactorSecret = secret; }
    public void DisableTwoFactor() { TwoFactorEnabled = false; TwoFactorSecret = null; }
}