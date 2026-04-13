// JwtService — Generates JWT access tokens and cryptographic refresh token strings.
// • JWT = HEADER.PAYLOAD.SIGNATURE (stateless auth, no session store needed).
// • Uses HMAC-SHA256 symmetric signing — same key in Auth Service and API Gateway.
// • Lives in Infrastructure because it depends on external JWT libraries.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services;

public class JwtService : IJwtService
{
    // Reads Jwt:Key, Jwt:Issuer, Jwt:Audience, Jwt:ExpiryMinutes from appsettings.json
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates a short-lived JWT access token containing the user's identity as claims.
    /// Every downstream service reads these claims to know WHO is making the request
    /// and WHAT ROLE they have, without calling the Auth DB.
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        // The secret key — same key is configured in the API Gateway and this service.
        // If they don't match, token validation fails with 401 Unauthorized.
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        // Claims = the data embedded inside the token's payload.
        // These travel with every HTTP request in the Authorization header.
        var claims = new[]
        {
            // "sub" (Subject) — the user's unique ID. All services use this to identify the user.
            new Claim(JwtRegisteredClaimNames.Sub,  user.UserId.ToString()),

            // "email" — used by the /me endpoint and for logging.
            new Claim(JwtRegisteredClaimNames.Email, user.Email),

            // "role" — used by [Authorize(Roles = "Instructor")] on controllers.
            // This is how we enforce role-based access control across ALL services.
            new Claim(ClaimTypes.Role,               user.Role.ToString()),

            // Custom claim — not a standard JWT claim, but convenient for display.
            new Claim("fullName",                    user.FullName),

            // "jti" (JWT ID) — a unique ID for THIS specific token.
            // Useful for token revocation lists (if we ever need them).
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        // Build the token with issuer, audience, claims, expiry, and signing credentials.
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],         // Who issued this token (e.g., "EduStack")
            audience: _config["Jwt:Audience"],      // Who can use this token (e.g., "EduStackClients")
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(    // Token expires after N minutes (from config)
                                    int.Parse(_config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        // Serialize the JwtSecurityToken object into the final "xxxxx.yyyyy.zzzzz" string.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure random string for the refresh token.
    /// Unlike the JWT, this is NOT self-contained — it's just a random key
    /// that maps to a database record. The DB stores who owns it and when it expires.
    /// </summary>
    public string GenerateRefreshToken()
    {
        // 64 bytes of cryptographic randomness → base64 encoded = ~88 characters.
        // RandomNumberGenerator is crypto-safe (unlike Random which is predictable).
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}