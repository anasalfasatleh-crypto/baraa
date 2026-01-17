using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.DTOs;

// Questionnaire Builder DTOs for Admin
public record QuestionnaireListItemDto(
    Guid Id,
    string Title,
    string? Description,
    QuestionnaireType Type,
    bool IsActive,
    int QuestionCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateQuestionnaireRequest(
    string Title,
    string? Description,
    QuestionnaireType Type
);

public record UpdateQuestionnaireRequest(
    string Title,
    string? Description,
    QuestionnaireType Type,
    bool IsActive
);

public record QuestionnaireDetailDto(
    Guid Id,
    string Title,
    string? Description,
    QuestionnaireType Type,
    bool IsActive,
    List<QuestionBuilderDto> Questions,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record QuestionBuilderDto(
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

public record CreateQuestionRequest(
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

public record UpdateQuestionRequest(
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

public record UpdateQuestionsRequest(
    List<QuestionUpdateItem> Questions
);

public record QuestionUpdateItem(
    Guid? Id, // null for new questions
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
