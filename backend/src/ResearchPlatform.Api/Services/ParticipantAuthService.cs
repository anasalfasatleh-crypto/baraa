using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.DTOs;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class ParticipantAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ParticipantTokenService _tokenService;
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 1;

    public ParticipantAuthService(
        ApplicationDbContext context,
        ParticipantTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Authenticate participant with login identifier and password
    /// </summary>
    public async Task<Result<ParticipantLoginResponse>> LoginAsync(ParticipantLoginRequest request)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.LoginIdentifier.ToLower() == request.LoginIdentifier.ToLower());

        if (participant == null)
        {
            return Result<ParticipantLoginResponse>.Failure("INVALID_CREDENTIALS");
        }

        // Check if account is locked
        if (IsAccountLocked(participant))
        {
            return Result<ParticipantLoginResponse>.Failure("ACCOUNT_LOCKED");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, participant.PasswordHash))
        {
            await HandleFailedLoginAsync(participant);
            return Result<ParticipantLoginResponse>.Failure("INVALID_CREDENTIALS");
        }

        // Successful login - reset failed attempts
        participant.FailedLoginAttempts = 0;
        participant.LockoutEnd = null;
        participant.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(participant);
        var refreshToken = await _tokenService.GenerateRefreshToken(participant.Id);

        var profile = new ParticipantProfile(
            participant.Id,
            participant.Code,
            participant.LoginIdentifier,
            participant.PhoneNumber,
            participant.CreatedAt,
            participant.LastLoginAt
        );

        return Result<ParticipantLoginResponse>.Success(new ParticipantLoginResponse(
            accessToken,
            refreshToken.Token,
            _tokenService.GetSessionDurationSeconds(),
            participant.MustChangePassword,
            profile
        ));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    public async Task<Result<ParticipantLoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        var token = await _tokenService.GetValidRefreshToken(refreshToken);

        if (token == null)
        {
            return Result<ParticipantLoginResponse>.Failure("INVALID_REFRESH_TOKEN");
        }

        var participant = token.Participant;

        // Revoke old refresh token
        await _tokenService.RevokeRefreshToken(refreshToken);

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(participant);
        var newRefreshToken = await _tokenService.GenerateRefreshToken(participant.Id);

        var profile = new ParticipantProfile(
            participant.Id,
            participant.Code,
            participant.LoginIdentifier,
            participant.PhoneNumber,
            participant.CreatedAt,
            participant.LastLoginAt
        );

        return Result<ParticipantLoginResponse>.Success(new ParticipantLoginResponse(
            newAccessToken,
            newRefreshToken.Token,
            _tokenService.GetSessionDurationSeconds(),
            participant.MustChangePassword,
            profile
        ));
    }

    /// <summary>
    /// Change participant password
    /// </summary>
    public async Task<Result<bool>> ChangePasswordAsync(Guid participantId, ChangePasswordRequest request)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
        {
            return Result<bool>.Failure("PARTICIPANT_NOT_FOUND");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, participant.PasswordHash))
        {
            return Result<bool>.Failure("INVALID_CURRENT_PASSWORD");
        }

        // Update password
        participant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        participant.MustChangePassword = false;
        participant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Reset participant password (admin action)
    /// </summary>
    public async Task<Result<PasswordResetResponse>> ResetPasswordAsync(Guid participantId, Guid adminId)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
        {
            return Result<PasswordResetResponse>.Failure("PARTICIPANT_NOT_FOUND");
        }

        // Generate temporary password
        var tempPassword = GenerateTemporaryPassword();

        // Update participant
        participant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        participant.MustChangePassword = true;
        participant.FailedLoginAttempts = 0;
        participant.LockoutEnd = null;
        participant.UpdatedAt = DateTime.UtcNow;

        // Log audit
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = adminId,
            Action = "ParticipantPasswordReset",
            EntityType = "Participant",
            EntityId = participantId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                ParticipantCode = participant.Code,
                ResetBy = adminId
            }),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Result<PasswordResetResponse>.Success(new PasswordResetResponse(
            tempPassword,
            "Password reset successful. Participant must change password on next login."
        ));
    }

    /// <summary>
    /// Logout - revoke refresh token
    /// </summary>
    public async Task LogoutAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshToken(refreshToken);
    }

    /// <summary>
    /// Get lockout info for a login identifier
    /// </summary>
    public async Task<AccountLockedResponse?> GetLockoutInfoAsync(string loginIdentifier)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.LoginIdentifier.ToLower() == loginIdentifier.ToLower());

        if (participant == null || !IsAccountLocked(participant))
        {
            return null;
        }

        var remainingSeconds = (int)(participant.LockoutEnd!.Value - DateTime.UtcNow).TotalSeconds;
        if (remainingSeconds < 0) remainingSeconds = 0;

        return new AccountLockedResponse(
            "Account is locked due to too many failed login attempts",
            participant.LockoutEnd.Value,
            remainingSeconds
        );
    }

    // ============================================================
    // PRIVATE METHODS
    // ============================================================

    private bool IsAccountLocked(Participant participant)
    {
        return participant.LockoutEnd.HasValue && participant.LockoutEnd > DateTime.UtcNow;
    }

    private async Task HandleFailedLoginAsync(Participant participant)
    {
        participant.FailedLoginAttempts++;

        if (participant.FailedLoginAttempts >= MaxFailedAttempts)
        {
            participant.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
        }

        await _context.SaveChangesAsync();
    }

    private string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
