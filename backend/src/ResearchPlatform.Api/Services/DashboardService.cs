using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardMetrics> GetDashboardMetricsAsync()
    {
        var metrics = new DashboardMetrics();

        // User counts by role
        metrics.TotalStudents = await _context.Users
            .Where(u => u.Role == Role.Student && u.Status == UserStatus.Active)
            .CountAsync();

        metrics.TotalEvaluators = await _context.Users
            .Where(u => u.Role == Role.Evaluator && u.Status == UserStatus.Active)
            .CountAsync();

        metrics.TotalAdmins = await _context.Users
            .Where(u => u.Role == Role.Admin && u.Status == UserStatus.Active)
            .CountAsync();

        // Get active questionnaires
        var pretestQuestionnaire = await _context.Questionnaires
            .FirstOrDefaultAsync(q => q.Type == QuestionnaireType.Pretest && q.IsActive);

        var posttestQuestionnaire = await _context.Questionnaires
            .FirstOrDefaultAsync(q => q.Type == QuestionnaireType.Posttest && q.IsActive);

        // Pre-test completion
        if (pretestQuestionnaire != null)
        {
            metrics.PretestCompletedCount = await _context.Answers
                .Where(a => a.QuestionnaireId == pretestQuestionnaire.Id && a.IsSubmitted)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();
        }

        // Post-test completion
        if (posttestQuestionnaire != null)
        {
            metrics.PosttestCompletedCount = await _context.Answers
                .Where(a => a.QuestionnaireId == posttestQuestionnaire.Id && a.IsSubmitted)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();
        }

        // Materials accessed
        metrics.MaterialsAccessedCount = await _context.MaterialAccesses
            .Select(ma => ma.UserId)
            .Distinct()
            .CountAsync();

        // Evaluator assignments
        metrics.TotalEvaluatorAssignments = await _context.EvaluatorAssignments
            .Where(ea => ea.IsActive)
            .CountAsync();

        // Scoring progress
        var totalStudentsWithSubmissions = await _context.Answers
            .Where(a => a.IsSubmitted)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync();

        if (totalStudentsWithSubmissions > 0)
        {
            var studentsWithScores = await _context.EvaluatorScores
                .Select(es => es.StudentId)
                .Distinct()
                .CountAsync();

            metrics.ScoringCompletionRate = (double)studentsWithScores / totalStudentsWithSubmissions * 100;
        }

        // Post-test batch status
        var currentBatch = await _context.PostTestBatches
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        if (currentBatch != null)
        {
            var now = DateTime.UtcNow;
            metrics.PosttestBatchOpen = currentBatch.OpenDate <= now && currentBatch.CloseDate >= now;
            metrics.CurrentBatchName = currentBatch.Name;
            metrics.CurrentBatchOpenDate = currentBatch.OpenDate;
            metrics.CurrentBatchCloseDate = currentBatch.CloseDate;
        }

        return metrics;
    }

    public async Task<List<CompletionTrend>> GetCompletionTrendsAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var pretestQuestionnaire = await _context.Questionnaires
            .FirstOrDefaultAsync(q => q.Type == QuestionnaireType.Pretest && q.IsActive);

        var posttestQuestionnaire = await _context.Questionnaires
            .FirstOrDefaultAsync(q => q.Type == QuestionnaireType.Posttest && q.IsActive);

        var trends = new List<CompletionTrend>();

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i).Date;
            var nextDate = date.AddDays(1);

            var trend = new CompletionTrend
            {
                Date = date
            };

            if (pretestQuestionnaire != null)
            {
                trend.PretestCount = await _context.Answers
                    .Where(a => a.QuestionnaireId == pretestQuestionnaire.Id &&
                                a.IsSubmitted &&
                                a.SubmittedAt.HasValue &&
                                a.SubmittedAt.Value >= date &&
                                a.SubmittedAt.Value < nextDate)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync();
            }

            if (posttestQuestionnaire != null)
            {
                trend.PosttestCount = await _context.Answers
                    .Where(a => a.QuestionnaireId == posttestQuestionnaire.Id &&
                                a.IsSubmitted &&
                                a.SubmittedAt.HasValue &&
                                a.SubmittedAt.Value >= date &&
                                a.SubmittedAt.Value < nextDate)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync();
            }

            trends.Add(trend);
        }

        return trends;
    }

    public async Task<List<HospitalDistribution>> GetHospitalDistributionAsync()
    {
        return await _context.Users
            .Where(u => u.Role == Role.Student && u.Status == UserStatus.Active)
            .GroupBy(u => u.Hospital ?? "Not Specified")
            .Select(g => new HospitalDistribution
            {
                Hospital = g.Key,
                StudentCount = g.Count()
            })
            .OrderByDescending(h => h.StudentCount)
            .ToListAsync();
    }
}

public class DashboardMetrics
{
    public int TotalStudents { get; set; }
    public int TotalEvaluators { get; set; }
    public int TotalAdmins { get; set; }
    public int PretestCompletedCount { get; set; }
    public int PosttestCompletedCount { get; set; }
    public int MaterialsAccessedCount { get; set; }
    public int TotalEvaluatorAssignments { get; set; }
    public double ScoringCompletionRate { get; set; }
    public bool PosttestBatchOpen { get; set; }
    public string? CurrentBatchName { get; set; }
    public DateTime? CurrentBatchOpenDate { get; set; }
    public DateTime? CurrentBatchCloseDate { get; set; }
}

public class CompletionTrend
{
    public DateTime Date { get; set; }
    public int PretestCount { get; set; }
    public int PosttestCount { get; set; }
}

public class HospitalDistribution
{
    public string Hospital { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}
