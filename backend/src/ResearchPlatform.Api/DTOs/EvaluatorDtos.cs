namespace ResearchPlatform.Api.DTOs;

// Assigned Student
public class AssignedStudentDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Hospital { get; set; }
    public string? Gender { get; set; }
}

// Student Responses
public class QuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public required string QuestionText { get; set; }
    public required string QuestionType { get; set; }
    public int Step { get; set; }
    public int OrderIndex { get; set; }
    public bool IsRequired { get; set; }
    public string? Answer { get; set; }
    public decimal? Score { get; set; }
    public bool IsScoreFinalized { get; set; }
}

public class StudentResponsesDto
{
    public Guid StudentId { get; set; }
    public Guid QuestionnaireId { get; set; }
    public required string QuestionnaireTitle { get; set; }
    public required string QuestionnaireType { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<QuestionResponseDto> Responses { get; set; } = new();
}

// Save Scores
public class ScoreItemDto
{
    public Guid QuestionId { get; set; }
    public decimal Score { get; set; }
}

public class SaveScoresRequestDto
{
    public Guid QuestionnaireId { get; set; }
    public List<ScoreItemDto> Scores { get; set; } = new();
    public bool Finalize { get; set; }
}

public class SaveScoresResponseDto
{
    public bool Success { get; set; }
    public required string Message { get; set; }
}
