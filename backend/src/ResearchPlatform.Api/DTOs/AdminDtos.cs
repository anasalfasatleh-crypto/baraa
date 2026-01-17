namespace ResearchPlatform.Api.DTOs;

// User Management DTOs
public class AdminUserDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public required string Status { get; set; }
    public string? Hospital { get; set; }
    public string? Gender { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public string? Hospital { get; set; }
    public string? Gender { get; set; }
}

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? Hospital { get; set; }
    public string? Gender { get; set; }
}

public class ResetPasswordRequest
{
    public required string NewPassword { get; set; }
}

public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public required string NewPassword { get; set; }
}

// Evaluator Assignment DTOs
public class CreateAssignmentRequest
{
    public Guid EvaluatorId { get; set; }
    public Guid StudentId { get; set; }
}

// Post-test Batch DTOs
public class PostTestBatchDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime CloseDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBatchRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime CloseDate { get; set; }
}

public class CloseBatchRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
}

// Material Management DTOs
public record AdminMaterialDto(
    Guid Id,
    string Title,
    string? Description,
    string Type,
    string? FileExtension,
    long? FileSizeBytes,
    int OrderIndex,
    bool IsActive,
    int AccessCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AdminMaterialDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string Type,
    string? FileExtension,
    long? FileSizeBytes,
    int OrderIndex,
    bool IsActive,
    string SignedUrl,
    int AccessCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public class UploadMaterialRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string Type { get; set; }
    public required IFormFile File { get; set; }
}

public class UpdateMaterialRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string Type { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
}
