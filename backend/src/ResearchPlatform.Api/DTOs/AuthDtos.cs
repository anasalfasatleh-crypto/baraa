using System.ComponentModel.DataAnnotations;

namespace ResearchPlatform.Api.DTOs;

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password
);

public record LoginResponse(
    string AccessToken,
    int ExpiresIn,
    UserInfo User
);

public record TokenResponse(
    string AccessToken,
    int ExpiresIn
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required][MinLength(8)] string NewPassword
);

public record UserInfo(
    Guid Id,
    string Email,
    string Name,
    string Role
);
