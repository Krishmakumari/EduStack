// AuthController — HTTP entry point for all authentication endpoints.
// • Intentionally THIN: no business logic, just delegates to IAuthService.
// • Route: api/auth (→ gateway routes /gateway/auth/... here).
// • Endpoints: register, login, refresh-token, forgot/reset-password, verify-email, me.

using AuthService.Application.DTOs.Requests;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

// [ApiController] → Enables automatic model validation (returns 400 if request body is malformed)
// [Route("api/auth")] → Base path for all endpoints in this controller
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Injected via Dependency Injection (registered in Program.cs).
    // The controller depends on the INTERFACE, not the implementation.
    // This is the Dependency Inversion Principle (D in SOLID).
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ─── POST api/auth/register ─────────────────────────────────────────────
    // Public endpoint — no [Authorize] needed.
    // Creates a new user with a hashed password.
    // Returns a success message; the user can then login.
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    // ─── POST api/auth/login ────────────────────────────────────────────────
    // Public endpoint. Validates email + password.
    // On success: returns JWT access token + refresh token.
    // On failure: throws InvalidCredentialsException → 401 Unauthorized.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    // ─── POST api/auth/refresh-token ────────────────────────────────────────
    // Called when the JWT expires (typically after 60 min).
    // The frontend sends the refresh token from the previous login.
    // The old refresh token is REVOKED and a new pair (JWT + refresh) is issued.
    // This is called "Token Rotation" — a security best practice.
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(result);
    }

    // ─── POST api/auth/forgot-password ──────────────────────────────────────
    // User provides their email. System generates a 6-digit OTP.
    // Note: Response is intentionally vague ("If this email exists...") to
    // prevent attackers from discovering which emails are registered.
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    // ─── POST api/auth/reset-password ───────────────────────────────────────
    // User provides email + OTP + new password. System validates the OTP,
    // hashes the new password, and updates the User record.
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return Ok(result);
    }

    // ─── GET api/auth/verify-email?token=xxx ────────────────────────────────
    // Email verification endpoint. In the current flow, users are auto-verified
    // at registration, so this is kept for interface compatibility.
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var result = await _authService.VerifyEmailAsync(token);
        return Ok(result);
    }

    // ─── GET api/auth/me ────────────────────────────────────────────────────
    // PROTECTED endpoint — requires a valid JWT.
    // Reads user info directly from JWT claims — NO database call needed.
    // This is why JWT is powerful: the user's identity travels with the token.
    //
    // The claims were embedded in the token during login (see JwtService).
    // Here we just extract them using ClaimTypes.
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // "sub" claim = UserId (set in JwtService as JwtRegisteredClaimNames.Sub)
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var fullName = User.FindFirst("fullName")?.Value;

        return Ok(new { userId, email, role, fullName });
    }
}