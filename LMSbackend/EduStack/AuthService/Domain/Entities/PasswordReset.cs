// PasswordReset Entity — Secure OTP-based password recovery.
// • Generates a random 6-digit OTP with 15-min expiry.
// • IsValid = !IsUsed && !IsExpired — prevents replay and delayed attacks.
// • Separate table from User because a user can request multiple resets.

namespace AuthService.Domain.Entities;

public class PasswordReset
{
    public Guid ResetId { get; set; }
    public Guid UserId { get; set; }

    // 6-digit numeric string (e.g., "482917"). Generated via Random.Shared.
    public string OtpCode { get; set; } = default!;

    // Expires 15 minutes after creation — short window for security.
    public DateTime ExpiresAt { get; set; }

    // Flipped to true after successful password reset to prevent reuse.
    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;

    // ── Computed Properties ─────────────────────────────────────────────────
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    /// <summary>
    /// Creates a new OTP reset request. The OTP is a random 6-digit number.
    /// </summary>
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

    /// <summary>Marks this OTP as consumed so it cannot be reused.</summary>
    public void MarkUsed() => IsUsed = true;
}