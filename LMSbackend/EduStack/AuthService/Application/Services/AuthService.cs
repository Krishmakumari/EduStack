// AuthService — Core business logic for authentication (Application Layer).
// • Contains all rules for register, login, token refresh, and password reset.
// • Separate from controller (Clean Architecture) for testability and reusability.
// • Security: BCrypt hashing, token rotation, vague responses to prevent email enumeration.

using AuthService.Application.DTOs.Requests;
using AuthService.Application.DTOs.Responses;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using AuthService.Infrastructure.Messaging;
using AuthService.Domain.Events;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IPasswordResetRepository _resetRepo;
    private readonly IJwtService _jwtService;
    private readonly RabbitMqPublisher _publisher;

    // Constructor Dependency Injection — .NET DI container creates and
    // injects the correct implementations (registered in Program.cs).
    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IPasswordResetRepository resetRepo,
        IJwtService jwtService,
        RabbitMqPublisher publisher)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _resetRepo = resetRepo;
        _jwtService = jwtService;
        _publisher = publisher;
    }

    // ─── Register ────────────────────────────────────────────────────────────
    // FLOW: Check duplicate → parse role → hash password → create User → save.
    //
    // WHY BCrypt.HashPassword()?
    // BCrypt automatically generates a random salt and appends it to the hash.
    // This means even if two users have the same password, their hashes differ.
    // BCrypt is also intentionally slow (configurable "work factor"), making
    // brute-force attacks impractical.
    public async Task<MessageResponse> RegisterAsync(RegisterRequest request)
    {
        // Step 1: Check if email is already registered.
        // We fail fast to avoid wasting resources on hashing.
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new DomainException("An account with this email already exists.");

        // Step 2: Validate the role string (e.g., "Student", "Instructor").
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            throw new DomainException("Invalid role specified.");

        // SECURITY: Block public registration as Admin.
        if (role == UserRole.Admin)
            throw new DomainException("Admin registration is not allowed.");

        // Step 3: Hash the plaintext password. NEVER store the raw password.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Step 4: Create the User entity via the factory method.
        // User.Create() sets sensible defaults (ID, lowercase email, etc.)
        var user = User.Create(request.FullName, request.Email, passwordHash, role);

        // Step 5: Auto-verify email (simplified flow — no email required).
        // In production, you'd send a verification email instead.
        user.MarkEmailVerified();

        // Step 6: Persist to database.
        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return new MessageResponse("Registration successful. You can now log in.");
    }

    // ─── Login ────────────────────────────────────────────────────────────────
    // FLOW: Find user → verify password → check banned → generate tokens → save.
    //
    // SECURITY: We throw the SAME error "Invalid email or password" for both
    // wrong email and wrong password. This prevents attackers from learning
    // which emails are registered (enumeration attack).
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Step 1: Find user by email. Returns null if not found.
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new InvalidCredentialsException();

        // Step 2: Compare the plaintext password against the stored BCrypt hash.
        // BCrypt.Verify() extracts the salt from the stored hash automatically.
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        // Step 3: Check if admin has banned this user.
        if (user.IsBanned) throw new AccountBannedException();

        // Step 4: Generate the JWT access token (short-lived, ~60 min).
        // This token carries the user's identity as claims.
        var accessToken = _jwtService.GenerateAccessToken(user);

        // Step 5: Generate a cryptographically random refresh token string.
        var rawRefreshToken = _jwtService.GenerateRefreshToken();

        // Step 6: Store the refresh token in the database.
        // "7" = valid for 7 days. After that, the user must re-login.
        var refreshToken = RefreshToken.Create(user.UserId, rawRefreshToken, 7);
        await _refreshTokenRepo.AddAsync(refreshToken);

        // Step 7: Record login timestamp for analytics.
        user.RecordLogin();
        await _userRepo.SaveChangesAsync();
        await _refreshTokenRepo.SaveChangesAsync();

        // Step 8: Return both tokens + user info to the frontend.
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    // ─── Refresh Token ────────────────────────────────────────────────────────
    // FLOW: Validate existing token → revoke it → issue a new pair.
    //
    // WHY TOKEN ROTATION?
    // If an attacker steals a refresh token, the real user's next refresh
    // will revoke the stolen token. Without rotation, the attacker could
    // use the stolen token indefinitely.
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Step 1: Look up the refresh token in the database.
        var existing = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken)
            ?? throw new DomainException("Invalid refresh token.");

        // Step 2: Check it's still valid (not revoked, not expired).
        if (!existing.IsActive)
            throw new DomainException("Refresh token has expired or been revoked.");

        // Step 3: Find the associated user.
        var user = await _userRepo.GetByIdAsync(existing.UserId)
            ?? throw new DomainException("User not found.");

        // Step 4: ROTATE — revoke the old token immediately.
        existing.Revoke();

        // Step 5: Generate a brand new refresh token.
        var newRawToken = _jwtService.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(user.UserId, newRawToken, 7, existing.DeviceInfo);

        // Step 6: Save the new token.
        await _refreshTokenRepo.AddAsync(newRefreshToken);
        await _refreshTokenRepo.SaveChangesAsync();

        // Step 7: Return fresh JWT + new refresh token.
        return new AuthResponse
        {
            AccessToken = _jwtService.GenerateAccessToken(user),
            RefreshToken = newRawToken,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    // ─── Forgot Password ──────────────────────────────────────────────────────
    // FLOW: Find user → generate OTP → store in DB → return OTP.
    //
    // SECURITY NOTE: We return the same message whether the email exists or not.
    // This prevents "email enumeration" — an attacker can't discover valid emails.
    // In production, the OTP would be emailed (not returned in the response).
    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user is null)
            return new MessageResponse("If this email exists, an OTP has been sent.");

        // PasswordReset.Create() generates a random 6-digit OTP with 15-min expiry.
        var reset = PasswordReset.Create(user.UserId);
        await _resetRepo.AddAsync(reset);
        await _resetRepo.SaveChangesAsync();

        // Step 3: Publish "OtpGeneratedEvent" to RabbitMQ.
        // NotificationService will pick this up and send an email.
        await _publisher.PublishAsync("otp_queue", new OtpGeneratedEvent
        {
            Email = request.Email,
            OtpCode = reset.OtpCode
        });

        // Vague response to prevent enumeration attacks, but enough for local dev testing.
        return new MessageResponse("If this email exists, an OTP has been sent to your email.");
    }

    // ─── Reset Password ───────────────────────────────────────────────────────
    // FLOW: Find user → find latest OTP → validate → update password hash.
    //
    // Three validation checks protect against:
    // 1. OTP expired (>15 min) → prevents delayed attacks
    // 2. OTP already used → prevents replay attacks
    // 3. OTP doesn't match → prevents brute-force guessing
    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new DomainException("Invalid request.");

        // We always check the LATEST reset request for this user.
        var reset = await _resetRepo.GetLatestByUserIdAsync(user.UserId)
            ?? throw new DomainException("No reset request found.");

        // IsValid = !IsUsed && !IsExpired (see PasswordReset entity).
        if (!reset.IsValid)
            throw new DomainException("OTP has expired or already been used.");

        if (reset.OtpCode != request.OtpCode)
            throw new DomainException("Invalid OTP.");

        // Hash the new password and update the user entity.
        user.UpdatePasswordHash(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

        // Mark OTP as used so it can't be reused.
        reset.MarkUsed();

        await _userRepo.SaveChangesAsync();
        await _resetRepo.SaveChangesAsync();

        return new MessageResponse("Password reset successfully. You can now log in.");
    }

    // ─── Verify Email ─────────────────────────────────────────────────────────
    // Placeholder for interface compatibility. Currently, users are auto-verified
    // during registration (see RegisterAsync above).
    public async Task<MessageResponse> VerifyEmailAsync(string token)
    {
        return await Task.FromResult(new MessageResponse("Email verification is not required."));
    }
}