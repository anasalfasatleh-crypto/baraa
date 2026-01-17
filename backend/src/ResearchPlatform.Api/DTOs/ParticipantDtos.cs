namespace ResearchPlatform.Api.DTOs;

// ============================================================
// REQUEST DTOs
// ============================================================

public record ParticipantRegisterRequest(
    string LoginIdentifier,
    string Password,
    string? PhoneNumber
);

public record ParticipantLoginRequest(
    string LoginIdentifier,
    string Password
);

public record RefreshTokenRequest(
    string RefreshToken
);

// Note: ChangePasswordRequest is defined in AuthDtos.cs and shared

// ============================================================
// RESPONSE DTOs
// ============================================================

public record ParticipantRegisterResponse(
    Guid Id,
    string Code,
    string LoginIdentifier,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);

public record ParticipantLoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    bool MustChangePassword,
    ParticipantProfile Participant
);

public record ParticipantProfile(
    Guid Id,
    string Code,
    string LoginIdentifier,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record ParticipantDetails(
    Guid Id,
    string Code,
    string LoginIdentifier,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastLoginAt,
    int FailedLoginAttempts,
    bool IsLocked,
    DateTime? LockoutEnd,
    bool MustChangePassword
);

public record ParticipantSummary(
    Guid Id,
    string Code,
    string LoginIdentifier,
    DateTime CreatedAt
);

public record ParticipantListResponse(
    List<ParticipantSummary> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record PasswordResetResponse(
    string TemporaryPassword,
    string Message
);

public record AccountLockedResponse(
    string Message,
    DateTime LockoutEnd,
    int RemainingSeconds
);

public record SuccessResponse(
    string Message
);

public record ErrorResponse(
    string Error,
    string Message
);

public record ValidationErrorResponse(
    string Error,
    string Message,
    Dictionary<string, List<string>> Errors
);
