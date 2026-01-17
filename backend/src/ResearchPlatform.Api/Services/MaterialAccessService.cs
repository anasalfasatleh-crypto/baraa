using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class MaterialAccessService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MaterialAccessService> _logger;

    public MaterialAccessService(
        ApplicationDbContext context,
        ILogger<MaterialAccessService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MaterialAccess> TrackAccessAsync(
        Guid userId,
        Guid materialId,
        int? durationSeconds = null,
        bool completed = false)
    {
        var access = new MaterialAccess
        {
            UserId = userId,
            MaterialId = materialId,
            AccessedAt = DateTime.UtcNow,
            DurationSeconds = durationSeconds,
            Completed = completed
        };

        _context.MaterialAccesses.Add(access);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Tracked material access: User {userId}, Material {materialId}");

        return access;
    }

    public async Task<List<MaterialAccess>> GetUserAccessHistoryAsync(Guid userId, Guid? materialId = null)
    {
        var query = _context.MaterialAccesses
            .Include(ma => ma.Material)
            .Where(ma => ma.UserId == userId);

        if (materialId.HasValue)
        {
            query = query.Where(ma => ma.MaterialId == materialId.Value);
        }

        return await query
            .OrderByDescending(ma => ma.AccessedAt)
            .ToListAsync();
    }

    public async Task<int> GetUserAccessCountAsync(Guid userId, Guid materialId)
    {
        return await _context.MaterialAccesses
            .Where(ma => ma.UserId == userId && ma.MaterialId == materialId)
            .CountAsync();
    }

    public async Task<int> GetTotalUserAccessCountAsync(Guid userId)
    {
        return await _context.MaterialAccesses
            .Where(ma => ma.UserId == userId)
            .Select(ma => ma.MaterialId)
            .Distinct()
            .CountAsync();
    }

    public async Task<bool> HasUserAccessedMaterialAsync(Guid userId, Guid materialId)
    {
        return await _context.MaterialAccesses
            .AnyAsync(ma => ma.UserId == userId && ma.MaterialId == materialId);
    }

    public async Task<Dictionary<Guid, int>> GetMaterialAccessCounts(Guid userId)
    {
        var accessCounts = await _context.MaterialAccesses
            .Where(ma => ma.UserId == userId)
            .GroupBy(ma => ma.MaterialId)
            .Select(g => new { MaterialId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MaterialId, x => x.Count);

        return accessCounts;
    }

    /// <summary>
    /// Gets total access count for a single material across all users (for admin use)
    /// </summary>
    public async Task<int> GetMaterialAccessCount(Guid materialId)
    {
        return await _context.MaterialAccesses
            .Where(ma => ma.MaterialId == materialId)
            .CountAsync();
    }

    /// <summary>
    /// Gets total access counts for multiple materials across all users (for admin use)
    /// </summary>
    public async Task<Dictionary<Guid, int>> GetMaterialAccessCounts(List<Guid> materialIds)
    {
        var accessCounts = await _context.MaterialAccesses
            .Where(ma => materialIds.Contains(ma.MaterialId))
            .GroupBy(ma => ma.MaterialId)
            .Select(g => new { MaterialId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MaterialId, x => x.Count);

        return accessCounts;
    }

    public async Task<int?> GetTotalTimeSpentAsync(Guid userId, Guid materialId)
    {
        var totalSeconds = await _context.MaterialAccesses
            .Where(ma => ma.UserId == userId && ma.MaterialId == materialId && ma.DurationSeconds.HasValue)
            .SumAsync(ma => ma.DurationSeconds);

        return totalSeconds;
    }
}
