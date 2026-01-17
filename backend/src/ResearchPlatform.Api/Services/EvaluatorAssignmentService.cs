using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;
using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.Services;

public class EvaluatorAssignmentService
{
    private readonly ApplicationDbContext _context;
    private readonly EvaluatorService _evaluatorService;
    private readonly ILogger<EvaluatorAssignmentService> _logger;

    public EvaluatorAssignmentService(
        ApplicationDbContext context,
        EvaluatorService evaluatorService,
        ILogger<EvaluatorAssignmentService> logger)
    {
        _context = context;
        _evaluatorService = evaluatorService;
        _logger = logger;
    }

    public async Task<List<EvaluatorAssignmentInfo>> GetAllAssignmentsAsync()
    {
        var assignments = await _context.EvaluatorAssignments
            .Include(a => a.Evaluator)
            .Include(a => a.Student)
            .Where(a => a.IsActive)
            .OrderBy(a => a.Evaluator.Name)
            .ThenBy(a => a.Student.Name)
            .ToListAsync();

        return assignments.Select(a => new EvaluatorAssignmentInfo
        {
            Id = a.Id,
            EvaluatorId = a.EvaluatorId,
            EvaluatorName = a.Evaluator.Name,
            EvaluatorEmail = a.Evaluator.Email,
            StudentId = a.StudentId,
            StudentName = a.Student.Name,
            StudentEmail = a.Student.Email,
            AssignedAt = a.AssignedAt
        }).ToList();
    }

    public async Task<List<EvaluatorAssignmentInfo>> GetAssignmentsByEvaluatorAsync(Guid evaluatorId)
    {
        var assignments = await _context.EvaluatorAssignments
            .Include(a => a.Student)
            .Where(a => a.EvaluatorId == evaluatorId && a.IsActive)
            .OrderBy(a => a.Student.Name)
            .ToListAsync();

        return assignments.Select(a => new EvaluatorAssignmentInfo
        {
            Id = a.Id,
            EvaluatorId = a.EvaluatorId,
            EvaluatorName = string.Empty, // Not needed for this query
            EvaluatorEmail = string.Empty,
            StudentId = a.StudentId,
            StudentName = a.Student.Name,
            StudentEmail = a.Student.Email,
            AssignedAt = a.AssignedAt
        }).ToList();
    }

    public async Task<EvaluatorAssignment> CreateAssignmentAsync(Guid evaluatorId, Guid studentId)
    {
        // Validate evaluator role
        var evaluator = await _context.Users.FindAsync(evaluatorId);
        if (evaluator == null || evaluator.Role != Role.Evaluator || evaluator.Status != UserStatus.Active)
        {
            throw new InvalidOperationException("Invalid evaluator");
        }

        // Validate student role
        var student = await _context.Users.FindAsync(studentId);
        if (student == null || student.Role != Role.Student || student.Status != UserStatus.Active)
        {
            throw new InvalidOperationException("Invalid student");
        }

        return await _evaluatorService.CreateAssignmentAsync(evaluatorId, studentId);
    }

    public async Task DeleteAssignmentAsync(Guid assignmentId)
    {
        var assignment = await _context.EvaluatorAssignments.FindAsync(assignmentId);
        if (assignment == null)
        {
            throw new InvalidOperationException("Assignment not found");
        }

        await _evaluatorService.DeactivateAssignmentAsync(assignment.EvaluatorId, assignment.StudentId);

        _logger.LogInformation($"Deleted assignment: {assignmentId}");
    }

    public async Task<BulkAssignmentResult> BulkAssignStudentsAsync(Guid evaluatorId, List<Guid> studentIds)
    {
        var result = new BulkAssignmentResult();

        foreach (var studentId in studentIds)
        {
            try
            {
                await CreateAssignmentAsync(evaluatorId, studentId);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Student {studentId}: {ex.Message}");
                _logger.LogWarning($"Failed to assign student {studentId} to evaluator {evaluatorId}: {ex.Message}");
            }
        }

        _logger.LogInformation($"Bulk assignment: {result.SuccessCount} succeeded, {result.Errors.Count} failed");

        return result;
    }

    public async Task<Dictionary<Guid, int>> GetAssignmentCountsPerEvaluatorAsync()
    {
        return await _context.EvaluatorAssignments
            .Where(a => a.IsActive)
            .GroupBy(a => a.EvaluatorId)
            .Select(g => new
            {
                EvaluatorId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.EvaluatorId, x => x.Count);
    }

    public async Task<List<UnassignedStudent>> GetUnassignedStudentsAsync()
    {
        var assignedStudentIds = await _context.EvaluatorAssignments
            .Where(a => a.IsActive)
            .Select(a => a.StudentId)
            .Distinct()
            .ToListAsync();

        var unassignedStudents = await _context.Users
            .Where(u => u.Role == Role.Student &&
                        u.Status == UserStatus.Active &&
                        !assignedStudentIds.Contains(u.Id))
            .OrderBy(u => u.Name)
            .ToListAsync();

        return unassignedStudents.Select(s => new UnassignedStudent
        {
            Id = s.Id,
            Name = s.Name,
            Email = s.Email,
            Hospital = s.Hospital
        }).ToList();
    }
}

public class EvaluatorAssignmentInfo
{
    public Guid Id { get; set; }
    public Guid EvaluatorId { get; set; }
    public string EvaluatorName { get; set; } = string.Empty;
    public string EvaluatorEmail { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public class BulkAssignmentResult
{
    public int SuccessCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class UnassignedStudent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Hospital { get; set; }
}
