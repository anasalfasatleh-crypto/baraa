using System.ComponentModel.DataAnnotations;

namespace ResearchPlatform.Api.Models;

public class PostTestBatch
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime OpenDate { get; set; }

    [Required]
    public DateTime CloseDate { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
