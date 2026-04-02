using AuthService.Application.DTOs.Requests;
using AuthService.Application.DTOs.Responses;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IPasswordResetRepository _resetRepo;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IPasswordResetRepository resetRepo,
        IJwtService jwtService)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _resetRepo = resetRepo;
        _jwtService = jwtService;
    }

    // ─── Register ────────────────────────────────────────────────────────────
    public async Task<MessageResponse> RegisterAsync(RegisterRequest request)
    {
        // 1. Check duplicate email
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new DomainException("An account with this email already exists.");

        // 2. Parse role
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            throw new DomainException("Invalid role specified.");

        // 3. Hash password & create user
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.FullName, request.Email, passwordHash, role);

        // 4. Auto-verify — no email needed
        user.MarkEmailVerified();

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return new MessageResponse("Registration successful. You can now log in.");
    }

    // ─── Login ────────────────────────────────────────────────────────────────
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // 1. Find user
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new InvalidCredentialsException();

        // 2. Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        // 3. Check if banned
        if (user.IsBanned) throw new AccountBannedException();

        // 4. Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var rawRefreshToken = _jwtService.GenerateRefreshToken();

        // 5. Persist refresh token & record login
        var refreshToken = RefreshToken.Create(user.UserId, rawRefreshToken, 7);
        await _refreshTokenRepo.AddAsync(refreshToken);

        user.RecordLogin();
        await _userRepo.SaveChangesAsync();
        await _refreshTokenRepo.SaveChangesAsync();

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
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var existing = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken)
            ?? throw new DomainException("Invalid refresh token.");

        if (!existing.IsActive)
            throw new DomainException("Refresh token has expired or been revoked.");

        var user = await _userRepo.GetByIdAsync(existing.UserId)
            ?? throw new DomainException("User not found.");

        // Rotate — revoke old, issue new
        existing.Revoke();
        var newRawToken = _jwtService.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(user.UserId, newRawToken, 7, existing.DeviceInfo);

        await _refreshTokenRepo.AddAsync(newRefreshToken);
        await _refreshTokenRepo.SaveChangesAsync();

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
    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user is null)
            return new MessageResponse("If this email exists, an OTP has been sent.");

        var reset = PasswordReset.Create(user.UserId);
        await _resetRepo.AddAsync(reset);
        await _resetRepo.SaveChangesAsync();

        // Return OTP directly in response (no email needed)
        return new MessageResponse($"Your OTP is: {reset.OtpCode}. It expires in 15 minutes.");
    }

    // ─── Reset Password ───────────────────────────────────────────────────────
    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new DomainException("Invalid request.");

        var reset = await _resetRepo.GetLatestByUserIdAsync(user.UserId)
            ?? throw new DomainException("No reset request found.");

        if (!reset.IsValid)
            throw new DomainException("OTP has expired or already been used.");

        if (reset.OtpCode != request.OtpCode)
            throw new DomainException("Invalid OTP.");

        user.UpdatePasswordHash(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        reset.MarkUsed();

        await _userRepo.SaveChangesAsync();
        await _resetRepo.SaveChangesAsync();

        return new MessageResponse("Password reset successfully. You can now log in.");
    }

    // ─── Verify Email ─────────────────────────────────────────────────────────
    public async Task<MessageResponse> VerifyEmailAsync(string token)
    {
        // Kept for interface compatibility — not used in current flow
        return await Task.FromResult(new MessageResponse("Email verification is not required."));
    }
}