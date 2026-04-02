namespace AuthService.Domain.Entities;

public class PasswordReset
{
    public Guid ResetId { get; set; }
    public Guid UserId { get; set; }
    public string OtpCode { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    public static PasswordReset Create(Guid userId)
    {
        return new PasswordReset
        {
            ResetId = Guid.NewGuid(),
            UserId = userId,
            OtpCode = Random.Shared.Next(100000, 999999).ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkUsed() => IsUsed = true;
}