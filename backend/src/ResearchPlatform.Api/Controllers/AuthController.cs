using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ResearchPlatform.Api.Auth;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.DTOs;
using ResearchPlatform.Api.Models;
using ResearchPlatform.Api.Models.Enums;
using ResearchPlatform.Api.Services;
using BCrypt.Net;

namespace ResearchPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;
    private readonly AuditService _auditService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ApplicationDbContext context,
        TokenService tokenService,
        AuditService auditService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _auditService = auditService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _auditService.LogAsync("login_failed", "User", null, null, new { email = request.Email });
            return Unauthorized(new { error = "Invalid credentials" });
        }

        if (user.Status == UserStatus.Inactive)
        {
            await _auditService.LogAsync("login_failed", "User", user.Id, null, new { reason = "inactive_account" });
            return Unauthorized(new { error = "Account is inactive" });
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = await _tokenService.GenerateRefreshToken(user.Id);

        // Set refresh token as HTTP-only cookie
        Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.ExpiresAt
        });

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("login_success", "User", user.Id);

        return Ok(new LoginResponse(
            accessToken,
            _jwtSettings.AccessTokenExpirationMinutes * 60,
            new UserInfo(user.Id, user.Email, user.Name, user.Role.ToString())
        ));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshTokenValue))
        {
            return Unauthorized(new { error = "No refresh token provided" });
        }

        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue && !rt.IsRevoked);

        if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new { error = "Invalid or expired refresh token" });
        }

        if (refreshToken.User.Status == UserStatus.Inactive)
        {
            return Unauthorized(new { error = "Account is inactive" });
        }

        // Generate new access token
        var accessToken = _tokenService.GenerateAccessToken(refreshToken.User);

        return Ok(new TokenResponse(
            accessToken,
            _jwtSettings.AccessTokenExpirationMinutes * 60
        ));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        if (Request.Cookies.TryGetValue("refreshToken", out var refreshTokenValue))
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue && rt.UserId == userId);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refreshToken");

        await _auditService.LogAsync("logout", "User", userId);

        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { error = "Current password is incorrect" });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("password_changed", "User", userId);

        return NoContent();
    }
}
