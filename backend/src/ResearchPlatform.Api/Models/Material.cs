using System.ComponentModel.DataAnnotations;
using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.Models;

public class Material
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public MaterialType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public required string StorageKey { get; set; } // S3/MinIO key

    [MaxLength(100)]
    public string? FileExtension { get; set; }

    public long? FileSizeBytes { get; set; }

    public int OrderIndex { get; set; } = 0;

    [Required]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<MaterialAccess> MaterialAccesses { get; set; } = new List<MaterialAccess>();
}
