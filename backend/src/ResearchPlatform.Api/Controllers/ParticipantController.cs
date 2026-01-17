using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchPlatform.Api.DTOs;
using ResearchPlatform.Api.Services;

namespace ResearchPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/participants")]
public class ParticipantController : ControllerBase
{
    private readonly ParticipantService _participantService;
    private readonly ParticipantAuthService _authService;

    public ParticipantController(
        ParticipantService participantService,
        ParticipantAuthService authService)
    {
        _participantService = participantService;
        _authService = authService;
    }

    // ============================================================
    // PUBLIC ENDPOINTS (No authentication required)
    // ============================================================

    /// <summary>
    /// Register a new participant
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] ParticipantRegisterRequest request)
    {
        var result = await _participantService.RegisterAsync(request);

        if (result.IsFailure)
        {
            if (result.Error == "DUPLICATE_LOGIN_IDENTIFIER")
            {
                return Conflict(new ErrorResponse("Conflict", "Login identifier already exists"));
            }
            return BadRequest(new ErrorResponse("BadRequest", result.Error));
        }

        return Created($"/api/v1/participants/{result.Value.Id}", result.Value);
    }

    /// <summary>
    /// Authenticate participant
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] ParticipantLoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.IsFailure)
        {
            if (result.Error == "ACCOUNT_LOCKED")
            {
                var lockoutInfo = await _authService.GetLockoutInfoAsync(request.LoginIdentifier);
                if (lockoutInfo != null)
                {
                    return StatusCode(423, lockoutInfo);
                }
            }
            return Unauthorized(new ErrorResponse("Unauthorized", "Invalid credentials"));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ErrorResponse("Unauthorized", "Invalid or expired refresh token"));
        }

        return Ok(result.Value);
    }

    // ============================================================
    // AUTHENTICATED PARTICIPANT ENDPOINTS
    // ============================================================

    /// <summary>
    /// Get current participant profile
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetProfile()
    {
        var participantId = GetCurrentParticipantId();
        if (participantId == null)
        {
            return Unauthorized(new ErrorResponse("Unauthorized", "Invalid token"));
        }

        var result = await _participantService.GetProfileAsync(participantId.Value);

        if (result.IsFailure)
        {
            return NotFound(new ErrorResponse("NotFound", "Participant not found"));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Change participant password
    /// </summary>
    [HttpPost("me/change-password")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var participantId = GetCurrentParticipantId();
        if (participantId == null)
        {
            return Unauthorized(new ErrorResponse("Unauthorized", "Invalid token"));
        }

        var result = await _authService.ChangePasswordAsync(participantId.Value, request);

        if (result.IsFailure)
        {
            if (result.Error == "INVALID_CURRENT_PASSWORD")
            {
                return Unauthorized(new ErrorResponse("Unauthorized", "Current password is incorrect"));
            }
            return BadRequest(new ErrorResponse("BadRequest", result.Error));
        }

        return Ok(new SuccessResponse("Password changed successfully"));
    }

    /// <summary>
    /// Logout participant
    /// </summary>
    [HttpPost("logout")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new SuccessResponse("Logged out successfully"));
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    private Guid? GetCurrentParticipantId()
    {
        var participantIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (participantIdClaim == null || !Guid.TryParse(participantIdClaim.Value, out var participantId))
        {
            return null;
        }
        return participantId;
    }
}
