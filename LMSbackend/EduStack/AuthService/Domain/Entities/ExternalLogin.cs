namespace AuthService.Domain.Entities;

public class ExternalLogin
{
    public Guid ExternalLoginId { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = default!;
    public string ProviderKey { get; set; } = default!;
    public string? ProviderDisplayName { get; set; }
    public DateTime LinkedAt { get; set; }

    public User User { get; set; } = default!;

    public static ExternalLogin Create(Guid userId, string provider, string providerKey, string? displayName = null)
    {
        return new ExternalLogin
        {
            ExternalLoginId = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderKey = providerKey,
            ProviderDisplayName = displayName,
            LinkedAt = DateTime.UtcNow
        };
    }
}