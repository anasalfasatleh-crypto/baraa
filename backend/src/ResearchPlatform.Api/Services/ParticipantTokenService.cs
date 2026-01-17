using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ResearchPlatform.Api.Auth;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class ParticipantTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _context;
    private const int ParticipantSessionMinutes = 1440; // 24 hours

    public ParticipantTokenService(IOptions<JwtSettings> jwtSettings, ApplicationDbContext context)
    {
        _jwtSettings = jwtSettings.Value;
        _context = context;
    }

    public string GenerateAccessToken(Participant participant)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, participant.Id.ToString()),
            new Claim("participant_code", participant.Code),
            new Claim("login_identifier", participant.LoginIdentifier),
            new Claim(ClaimTypes.Role, "Participant"),
            new Claim("user_type", "participant")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ParticipantSessionMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<ParticipantRefreshToken> GenerateRefreshToken(Guid participantId)
    {
        var refreshToken = new ParticipantRefreshToken
        {
            ParticipantId = participantId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.ParticipantRefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            // Verify this is a participant token
            var userTypeClaim = principal.FindFirst("user_type");
            if (userTypeClaim?.Value != "participant")
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task RevokeRefreshToken(string token)
    {
        var refreshToken = await _context.ParticipantRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.RevokedAt == null);

        if (refreshToken != null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ParticipantRefreshToken?> GetValidRefreshToken(string token)
    {
        return await _context.ParticipantRefreshTokens
            .Include(rt => rt.Participant)
            .FirstOrDefaultAsync(rt =>
                rt.Token == token &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// Returns the session duration in seconds (24 hours = 86400 seconds)
    /// </summary>
    public int GetSessionDurationSeconds()
    {
        return ParticipantSessionMinutes * 60;
    }
}
