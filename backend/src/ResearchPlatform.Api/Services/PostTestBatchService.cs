using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class PostTestBatchService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PostTestBatchService> _logger;

    public PostTestBatchService(
        ApplicationDbContext context,
        ILogger<PostTestBatchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PostTestBatch?> GetCurrentOpenBatchAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.PostTestBatches
            .Where(b => b.IsActive && b.OpenDate <= now && b.CloseDate >= now)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsPostTestAvailableAsync()
    {
        var batch = await GetCurrentOpenBatchAsync();
        return batch != null;
    }

    public async Task<PostTestBatch> CreateBatchAsync(
        string name,
        string? description,
        DateTime openDate,
        DateTime closeDate)
    {
        var batch = new PostTestBatch
        {
            Name = name,
            Description = description,
            OpenDate = openDate,
            CloseDate = closeDate,
            IsActive = true
        };

        _context.PostTestBatches.Add(batch);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created post-test batch: {batch.Id} - {batch.Name}");

        return batch;
    }

    public async Task<PostTestBatch> UpdateBatchAsync(
        Guid id,
        string name,
        string? description,
        DateTime openDate,
        DateTime closeDate,
        bool isActive)
    {
        var batch = await _context.PostTestBatches.FindAsync(id);
        if (batch == null)
        {
            throw new InvalidOperationException("Batch not found");
        }

        batch.Name = name;
        batch.Description = description;
        batch.OpenDate = openDate;
        batch.CloseDate = closeDate;
        batch.IsActive = isActive;
        batch.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Updated post-test batch: {batch.Id} - {batch.Name}");

        return batch;
    }

    public async Task<List<PostTestBatch>> GetAllBatchesAsync()
    {
        return await _context.PostTestBatches
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteBatchAsync(Guid id)
    {
        var batch = await _context.PostTestBatches.FindAsync(id);
        if (batch == null)
        {
            throw new InvalidOperationException("Batch not found");
        }

        _context.PostTestBatches.Remove(batch);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Deleted post-test batch: {id}");
    }
}
