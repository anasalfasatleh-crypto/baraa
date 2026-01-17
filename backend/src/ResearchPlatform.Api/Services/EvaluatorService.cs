using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class EvaluatorService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EvaluatorService> _logger;

    public EvaluatorService(
        ApplicationDbContext context,
        ILogger<EvaluatorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<User>> GetAssignedStudentsAsync(Guid evaluatorId)
    {
        return await _context.EvaluatorAssignments
            .Where(a => a.EvaluatorId == evaluatorId && a.IsActive)
            .Include(a => a.Student)
            .Select(a => a.Student)
            .ToListAsync();
    }

    public async Task<EvaluatorAssignment?> GetAssignmentAsync(Guid evaluatorId, Guid studentId)
    {
        return await _context.EvaluatorAssignments
            .FirstOrDefaultAsync(a =>
                a.EvaluatorId == evaluatorId &&
                a.StudentId == studentId &&
                a.IsActive);
    }

    public async Task<bool> IsStudentAssignedAsync(Guid evaluatorId, Guid studentId)
    {
        return await _context.EvaluatorAssignments
            .AnyAsync(a =>
                a.EvaluatorId == evaluatorId &&
                a.StudentId == studentId &&
                a.IsActive);
    }

    public async Task<EvaluatorAssignment> CreateAssignmentAsync(Guid evaluatorId, Guid studentId)
    {
        // Check if assignment already exists
        var existing = await _context.EvaluatorAssignments
            .FirstOrDefaultAsync(a =>
                a.EvaluatorId == evaluatorId &&
                a.StudentId == studentId);

        if (existing != null)
        {
            // Reactivate if inactive
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Reactivated assignment: Evaluator {evaluatorId} to Student {studentId}");
            }
            return existing;
        }

        // Create new assignment
        var assignment = new EvaluatorAssignment
        {
            EvaluatorId = evaluatorId,
            StudentId = studentId,
            IsActive = true
        };

        _context.EvaluatorAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created assignment: Evaluator {evaluatorId} to Student {studentId}");

        return assignment;
    }

    public async Task DeactivateAssignmentAsync(Guid evaluatorId, Guid studentId)
    {
        var assignment = await _context.EvaluatorAssignments
            .FirstOrDefaultAsync(a =>
                a.EvaluatorId == evaluatorId &&
                a.StudentId == studentId &&
                a.IsActive);

        if (assignment == null)
        {
            throw new InvalidOperationException("Assignment not found");
        }

        assignment.IsActive = false;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Deactivated assignment: Evaluator {evaluatorId} to Student {studentId}");
    }

    public async Task<List<EvaluatorAssignment>> GetAllAssignmentsAsync()
    {
        return await _context.EvaluatorAssignments
            .Include(a => a.Evaluator)
            .Include(a => a.Student)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();
    }
}
