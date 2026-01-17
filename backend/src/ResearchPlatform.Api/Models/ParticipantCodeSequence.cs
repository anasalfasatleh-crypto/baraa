using System.ComponentModel.DataAnnotations;

namespace ResearchPlatform.Api.Models;

public class ParticipantCodeSequence
{
    [Key]
    public int Id { get; set; } = 1;

    [Required]
    [MaxLength(5)]
    public string CurrentPrefix { get; set; } = "A";

    [Required]
    public int CurrentNumber { get; set; } = 0;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
