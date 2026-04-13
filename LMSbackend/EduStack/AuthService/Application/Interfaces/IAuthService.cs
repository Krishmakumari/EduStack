// IAuthService — Contract (interface) for all authentication operations.
// • Dependency Inversion Principle: controller depends on this, not the concrete class.
// • Enables mocking for unit tests and swapping implementations.
// • All methods async because they involve database I/O.

using AuthService.Application.DTOs.Requests;
using AuthService.Application.DTOs.Responses;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<MessageResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<MessageResponse> VerifyEmailAsync(string token);
}