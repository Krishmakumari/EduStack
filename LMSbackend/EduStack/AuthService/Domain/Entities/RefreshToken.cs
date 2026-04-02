namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public static RefreshToken Create(Guid userId, string token, int expiryDays, string? deviceInfo = null)
    {
        return new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            DeviceInfo = deviceInfo,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke() => IsRevoked = true;
}