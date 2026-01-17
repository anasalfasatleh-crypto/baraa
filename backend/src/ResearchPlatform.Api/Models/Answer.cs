using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchPlatform.Api.Models;

public class Answer
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

    public string? Value { get; set; } // Stores the answer value (text, number, selected option)

    public bool IsSubmitted { get; set; } = false; // Draft vs submitted

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Questionnaire Questionnaire { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
