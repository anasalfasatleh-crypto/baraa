using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchPlatform.Api.Models;

public class MaterialAccess
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid MaterialId { get; set; }

    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

    public int? DurationSeconds { get; set; } // Time spent on material

    public bool Completed { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(MaterialId))]
    public Material Material { get; set; } = null!;
}
