using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchPlatform.Api.Models;

public class EvaluatorAssignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EvaluatorId { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(EvaluatorId))]
    public User Evaluator { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public User Student { get; set; } = null!;
}
