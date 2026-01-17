using System.ComponentModel.DataAnnotations;

namespace ResearchPlatform.Api.Models;

public class ParticipantRefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ParticipantId { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Token { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    // Navigation property
    public Participant Participant { get; set; } = null!;
}
