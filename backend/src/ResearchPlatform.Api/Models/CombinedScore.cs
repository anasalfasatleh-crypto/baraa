using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchPlatform.Api.Models;

public class CombinedScore
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid QuestionId { get; set; }

    [Required]
    public Guid QuestionnaireId { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal AverageScore { get; set; }

    [Required]
    public int EvaluatorCount { get; set; }

    [Required]
    public bool IsFinalized { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(StudentId))]
    public User Student { get; set; } = null!;

    [ForeignKey(nameof(QuestionId))]
    public Question Question { get; set; } = null!;

    [ForeignKey(nameof(QuestionnaireId))]
    public Questionnaire Questionnaire { get; set; } = null!;
}
