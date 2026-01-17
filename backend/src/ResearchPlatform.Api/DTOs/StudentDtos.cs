using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.DTOs;

// Student Status
public record StudentStatusDto(
    bool PretestCompleted,
    DateTime? PretestCompletedAt,
    bool PosttestCompleted,
    DateTime? PosttestCompletedAt,
    int MaterialsAccessed
);

// Questionnaire DTOs
public record QuestionDto(
    Guid Id,
    string Text,
    QuestionType Type,
    List<string>? Options,
    int OrderIndex,
    int Step,
    bool IsRequired,
    int? MinValue,
    int? MaxValue,
    string? MinLabel,
    string? MaxLabel
);

public record AnswerDto(
    Guid QuestionId,
    string? Value
);

public record QuestionnaireWithAnswersDto(
    Guid Id,
    string Title,
    string? Description,
    QuestionnaireType Type,
    List<QuestionDto> Questions,
    List<AnswerDto> Answers,
    int TotalSteps,
    bool IsSubmitted
);

// Save Answers
public record SaveAnswersRequest(
    Dictionary<Guid, string> Answers
);

public record SaveAnswersResponse(
    bool Success,
    string Message
);

// Submission
public record SubmissionResponseDto(
    bool Success,
    string Message,
    DateTime SubmittedAt
);

// Step Timing
public record StepTimingRequest(
    int Step,
    bool IsStart // true = start step, false = end step
);

// Materials
public record MaterialDto(
    Guid Id,
    string Title,
    string? Description,
    MaterialType Type,
    string? FileExtension,
    long? FileSizeBytes,
    int AccessCount
);

public record MaterialDetailDto(
    Guid Id,
    string Title,
    string? Description,
    MaterialType Type,
    string? FileExtension,
    long? FileSizeBytes,
    string SignedUrl,
    int AccessCount
);

public record TrackMaterialAccessRequest(
    int? DurationSeconds,
    bool Completed
);
