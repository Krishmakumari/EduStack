namespace AuthService.Domain.Entities;

public class EmailVerification
{
    public Guid VerificationId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    public static EmailVerification Create(Guid userId)
    {
        return new EmailVerification
        {
            VerificationId = Guid.NewGuid(),
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkUsed() => IsUsed = true;
}