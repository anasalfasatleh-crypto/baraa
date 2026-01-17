using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;
using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.Services;

public class MaterialService
{
    private readonly ApplicationDbContext _context;
    private readonly StorageService _storageService;
    private readonly ILogger<MaterialService> _logger;

    public MaterialService(
        ApplicationDbContext context,
        StorageService storageService,
        ILogger<MaterialService> logger)
    {
        _context = context;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<List<Material>> GetActiveMaterialsAsync()
    {
        return await _context.Materials
            .Where(m => m.IsActive)
            .OrderBy(m => m.OrderIndex)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Material>> GetAllMaterialsAsync(bool includeInactive = true)
    {
        var query = _context.Materials.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(m => m.IsActive);
        }

        return await query
            .OrderBy(m => m.OrderIndex)
            .ThenByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Material?> GetMaterialByIdForAdminAsync(Guid id)
    {
        return await _context.Materials.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Material?> GetMaterialByIdAsync(Guid id)
    {
        return await _context.Materials
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
    }

    public async Task<string> GetMaterialSignedUrlAsync(Guid materialId, int expirySeconds = 3600, bool includeInactive = false)
    {
        var material = includeInactive
            ? await GetMaterialByIdForAdminAsync(materialId)
            : await GetMaterialByIdAsync(materialId);
        if (material == null)
        {
            throw new InvalidOperationException("Material not found");
        }

        return await _storageService.GetPresignedUrlAsync(material.StorageKey, expirySeconds);
    }

    public async Task<Material> CreateMaterialAsync(
        string title,
        string? description,
        MaterialType type,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSizeBytes)
    {
        var fileExtension = Path.GetExtension(fileName);
        var storageKey = $"materials/{Guid.NewGuid()}{fileExtension}";

        await _storageService.UploadFileAsync(fileStream, storageKey, contentType);

        var material = new Material
        {
            Title = title,
            Description = description,
            Type = type,
            StorageKey = storageKey,
            FileExtension = fileExtension,
            FileSizeBytes = fileSizeBytes,
            IsActive = true
        };

        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created material: {material.Id} - {material.Title}");

        return material;
    }

    public async Task<Material> UpdateMaterialAsync(
        Guid id,
        string title,
        string? description,
        MaterialType type,
        int orderIndex,
        bool isActive)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            throw new InvalidOperationException("Material not found");
        }

        material.Title = title;
        material.Description = description;
        material.Type = type;
        material.OrderIndex = orderIndex;
        material.IsActive = isActive;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Updated material: {material.Id} - {material.Title}");

        return material;
    }

    public async Task DeleteMaterialAsync(Guid id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            throw new InvalidOperationException("Material not found");
        }

        // Delete from storage
        await _storageService.DeleteFileAsync(material.StorageKey);

        // Delete from database (cascade will delete MaterialAccesses)
        _context.Materials.Remove(material);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Deleted material: {id}");
    }

    public async Task<int> GetUserMaterialAccessCountAsync(Guid userId)
    {
        return await _context.MaterialAccesses
            .Where(ma => ma.UserId == userId)
            .Select(ma => ma.MaterialId)
            .Distinct()
            .CountAsync();
    }
}
