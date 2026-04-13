// RefreshToken Entity — Long-lived token for silent JWT renewal.
// • Lets users get new JWTs without re-entering their password.
// • Supports Token Rotation: old token is revoked when a new one is issued.
// • IsActive = !IsRevoked && !IsExpired — two ways a token becomes invalid.
// • Valid for 7 days; stored in DB (unlike JWT which is stateless).

namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }

    // The actual random token string (64 cryptographic bytes, base64-encoded).
    // This is what the client sends back to get a new JWT.
    public string Token { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }

    // Set to true when the token is used (rotation) or on explicit logout.
    public bool IsRevoked { get; set; }

    // Optional: tracks which device created this token (e.g., "Chrome on Windows").
    // Useful for "Active Sessions" UI where users can revoke specific devices.
    public string? DeviceInfo { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation property — links back to the owning User.
    public User User { get; set; } = default!;

    // ── Computed Properties ─────────────────────────────────────────────────
    // These are NOT stored in the database. EF Core ignores them.
    // They encapsulate the "is this token still usable?" logic in one place.
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Factory Method — creates a new refresh token for a user.
    /// expiryDays is typically 7 (one week of validity).
    /// </summary>
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

    /// <summary>Marks this token as revoked so it can never be used again.</summary>
    public void Revoke() => IsRevoked = true;
}