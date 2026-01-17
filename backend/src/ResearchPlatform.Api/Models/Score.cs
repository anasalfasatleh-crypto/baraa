using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchPlatform.Api.Models;

public class Score
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    [ForeignKey(nameof(Questionnaire))]
    public Guid QuestionnaireId { get; set; }

    [Required]
    [ForeignKey(nameof(Question))]
    public Guid QuestionId { get; set; }

    public decimal? AutoScore { get; set; } // Automatically calculated score

    public decimal? ManualScore { get; set; } // Evaluator-assigned score

    [ForeignKey(nameof(Evaluator))]
    public Guid? EvaluatedBy { get; set; } // Evaluator who scored this

    public DateTime? EvaluatedAt { get; set; }

    [MaxLength(500)]
    public string? EvaluatorNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Questionnaire Questionnaire { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public User? Evaluator { get; set; }
}
