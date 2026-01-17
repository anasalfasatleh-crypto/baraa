using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchPlatform.Api.Models;

public class StepTiming
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
    public int Step { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? TimeSpentSeconds { get; set; } // Calculated: EndTime - StartTime

    // Navigation properties
    public User User { get; set; } = null!;
    public Questionnaire Questionnaire { get; set; } = null!;
}
