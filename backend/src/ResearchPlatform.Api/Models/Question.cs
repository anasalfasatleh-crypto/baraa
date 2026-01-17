using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.Models;

public class Question
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(Questionnaire))]
    public Guid QuestionnaireId { get; set; }

    [Required]
    public required string Text { get; set; }

    [Required]
    public QuestionType Type { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Options { get; set; } // JSON array for multiple choice/dropdown options

    [Required]
    public int OrderIndex { get; set; }

    [Required]
    public int Step { get; set; } // Which step/page this question appears on

    public bool IsRequired { get; set; } = true;

    public int? MinValue { get; set; } // For Likert scales

    public int? MaxValue { get; set; } // For Likert scales

    [MaxLength(50)]
    public string? MinLabel { get; set; } // Label for minimum value

    [MaxLength(50)]
    public string? MaxLabel { get; set; } // Label for maximum value

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Questionnaire Questionnaire { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
