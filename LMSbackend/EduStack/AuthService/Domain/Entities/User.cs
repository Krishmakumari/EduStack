
//->Represents a registered user (Student, Instructor, or Admin).       
//   Every other service in the system identifies users by the UserId    
//  that is generated here during registration.                         
                                                                           
//   WHY A PRIVATE CONSTRUCTOR + STATIC Create()?                              
//   This is the Factory Method pattern. It guarantees every new User always    
//   has a UUID, a lowercase email, and sensible defaults (not banned, not      
//   verified). You can never create a User in an invalid state.               
//                                                                             
//   WHY PasswordHash AND NOT Password?                                        
//   We NEVER store the real password. We store a one-way hash using BCrypt.   
//   Even if the database leaks, attackers cannot reverse the hash.            
//                                                                             
//   NAVIGATION PROPERTIES:                                                    
//   - RefreshTokens: A user can have multiple active sessions (phone + laptop)
//   - ExternalLogins: For future Google/GitHub OAuth support                  
//                                                                             


using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User
{
    // ── Primary Key ─────────────────────────────────────────────────────────
    // Generated as a new GUID during Create(). This ID is embedded in the JWT
    // token and used by ALL other microservices to identify this user.
    public Guid UserId { get; private set; }

    public string FullName { get; private set; } = default!;

    // Always stored in lowercase (see Create method) to avoid case-sensitivity
    // issues when searching by email during login.
    public string Email { get; private set; } = default!;

    // BCrypt hash of the user's password. Never the plaintext password.
    public string PasswordHash { get; private set; } = default!;

    // Controls what the user can do system-wide. The JWT includes this as a
    // claim, and every service reads it via [Authorize(Roles = "Instructor")].
    public UserRole Role { get; private set; }

    // Set to true after the user clicks the verification link in their email.
    // Login is blocked until this is true (see EmailNotVerifiedException).
    public bool IsEmailVerified { get; private set; }

    // Admin can ban misbehaving users. Login throws AccountBannedException.
    public bool IsBanned { get; private set; }

    // Future feature: TOTP-based two-factor authentication.
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }

    public string? ProfileImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Updated every login — useful for analytics ("last active 2 days ago").
    public DateTime? LastLoginAt { get; private set; }

    // ── Navigation Properties (EF Core relationships) ───────────────────────
    // One User → Many RefreshTokens (one per device/session)
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    // One User → Many ExternalLogins (Google, GitHub, etc.)
    public ICollection<ExternalLogin> ExternalLogins { get; private set; } = new List<ExternalLogin>();

    // Private constructor forces all creation through the factory method below.
    private User() { }

    /// <summary>
    /// Factory Method — the ONLY way to create a new User.
    /// Guarantees sensible defaults: GUID ID, lowercase email, not banned, not verified.
    /// </summary>
    public static User Create(string fullName, string email, string passwordHash, UserRole role)
    {
        return new User
        {
            UserId = Guid.NewGuid(),
            FullName = fullName,
            Email = email.ToLowerInvariant(),  // normalize for case-insensitive lookup
            PasswordHash = passwordHash,
            Role = role,
            IsEmailVerified = false,  // must verify via email link
            IsBanned = false,
            TwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ── Domain Methods ──────────────────────────────────────────────────────
    // Each method encapsulates a single state transition. If business rules
    // grow (e.g., "send an event when banned"), we only change it HERE.

    public void MarkEmailVerified() => IsEmailVerified = true;
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
    public void Ban() => IsBanned = true;
    public void Unban() => IsBanned = false;
    public void UpdatePasswordHash(string newHash) => PasswordHash = newHash;
    public void EnableTwoFactor(string secret) { TwoFactorEnabled = true; TwoFactorSecret = secret; }
    public void DisableTwoFactor() { TwoFactorEnabled = false; TwoFactorSecret = null; }
}