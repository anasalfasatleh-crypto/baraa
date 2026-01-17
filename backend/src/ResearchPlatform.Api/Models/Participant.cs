using System.ComponentModel.DataAnnotations;

namespace ResearchPlatform.Api.Models;

public class Participant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(10)]
    public required string Code { get; set; }

    [Required]
    [MaxLength(255)]
    public required string LoginIdentifier { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(255)]
    public required string PasswordHash { get; set; }

    [Required]
    public bool MustChangePassword { get; set; } = false;

    [Required]
    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LockoutEnd { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<ParticipantRefreshToken> RefreshTokens { get; set; } = new List<ParticipantRefreshToken>();
}
