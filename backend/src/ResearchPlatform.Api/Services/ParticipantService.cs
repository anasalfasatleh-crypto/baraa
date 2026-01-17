using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.DTOs;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class ParticipantService
{
    private readonly ApplicationDbContext _context;
    private readonly ParticipantCodeService _codeService;
    private readonly ParticipantTokenService _tokenService;

    public ParticipantService(
        ApplicationDbContext context,
        ParticipantCodeService codeService,
        ParticipantTokenService tokenService)
    {
        _context = context;
        _codeService = codeService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new participant with auto-generated code
    /// </summary>
    public async Task<Result<ParticipantRegisterResponse>> RegisterAsync(ParticipantRegisterRequest request)
    {
        // Check for duplicate login identifier (case-insensitive)
        var existingParticipant = await _context.Participants
            .FirstOrDefaultAsync(p => p.LoginIdentifier.ToLower() == request.LoginIdentifier.ToLower());

        if (existingParticipant != null)
        {
            return Result<ParticipantRegisterResponse>.Failure("DUPLICATE_LOGIN_IDENTIFIER");
        }

        // Generate unique code
        var code = await _codeService.GenerateNextCodeAsync();

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create participant
        var participant = new Participant
        {
            Code = code,
            LoginIdentifier = request.LoginIdentifier,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = passwordHash,
            MustChangePassword = false,
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(participant);
        var refreshToken = await _tokenService.GenerateRefreshToken(participant.Id);

        return Result<ParticipantRegisterResponse>.Success(new ParticipantRegisterResponse(
            participant.Id,
            participant.Code,
            participant.LoginIdentifier,
            accessToken,
            refreshToken.Token,
            _tokenService.GetSessionDurationSeconds()
        ));
    }

    /// <summary>
    /// Get participant profile by ID
    /// </summary>
    public async Task<Result<ParticipantProfile>> GetProfileAsync(Guid participantId)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
        {
            return Result<ParticipantProfile>.Failure("PARTICIPANT_NOT_FOUND");
        }

        return Result<ParticipantProfile>.Success(new ParticipantProfile(
            participant.Id,
            participant.Code,
            participant.LoginIdentifier,
            participant.PhoneNumber,
            participant.CreatedAt,
            participant.LastLoginAt
        ));
    }

    /// <summary>
    /// Get participant details (for admin)
    /// </summary>
    public async Task<Result<ParticipantDetails>> GetDetailsAsync(Guid participantId)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
        {
            return Result<ParticipantDetails>.Failure("PARTICIPANT_NOT_FOUND");
        }

        var isLocked = participant.LockoutEnd.HasValue && participant.LockoutEnd > DateTime.UtcNow;

        return Result<ParticipantDetails>.Success(new ParticipantDetails(
            participant.Id,
            participant.Code,
            participant.LoginIdentifier,
            participant.PhoneNumber,
            participant.CreatedAt,
            participant.UpdatedAt,
            participant.LastLoginAt,
            participant.FailedLoginAttempts,
            isLocked,
            participant.LockoutEnd,
            participant.MustChangePassword
        ));
    }

    /// <summary>
    /// Search participants by code, login identifier, or phone
    /// </summary>
    public async Task<List<ParticipantSummary>> SearchAsync(string query)
    {
        var lowerQuery = query.ToLower();

        var participants = await _context.Participants
            .Where(p =>
                p.Code.ToLower().Contains(lowerQuery) ||
                p.LoginIdentifier.ToLower().Contains(lowerQuery) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(query)))
            .OrderByDescending(p => p.CreatedAt)
            .Take(50)
            .Select(p => new ParticipantSummary(
                p.Id,
                p.Code,
                p.LoginIdentifier,
                p.CreatedAt))
            .ToListAsync();

        return participants;
    }

    /// <summary>
    /// Get paginated list of participants
    /// </summary>
    public async Task<ParticipantListResponse> GetListAsync(int page, int pageSize, string? search)
    {
        var query = _context.Participants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(p =>
                p.Code.ToLower().Contains(lowerSearch) ||
                p.LoginIdentifier.ToLower().Contains(lowerSearch) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ParticipantSummary(
                p.Id,
                p.Code,
                p.LoginIdentifier,
                p.CreatedAt))
            .ToListAsync();

        return new ParticipantListResponse(items, totalCount, page, pageSize, totalPages);
    }
}

/// <summary>
/// Simple Result type for operation results
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string Error { get; }

    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default!, error);
}
