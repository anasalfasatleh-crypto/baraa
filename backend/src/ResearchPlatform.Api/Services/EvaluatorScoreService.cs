using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class EvaluatorScoreService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EvaluatorScoreService> _logger;

    public EvaluatorScoreService(
        ApplicationDbContext context,
        ILogger<EvaluatorScoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<EvaluatorScore>> GetStudentScoresAsync(
        Guid studentId,
        Guid questionnaireId,
        Guid evaluatorId)
    {
        return await _context.EvaluatorScores
            .Where(s =>
                s.StudentId == studentId &&
                s.QuestionnaireId == questionnaireId &&
                s.EvaluatorId == evaluatorId)
            .Include(s => s.Question)
            .OrderBy(s => s.Question.OrderIndex)
            .ToListAsync();
    }

    public async Task<EvaluatorScore> SaveScoreAsync(
        Guid studentId,
        Guid questionId,
        Guid questionnaireId,
        Guid evaluatorId,
        decimal score)
    {
        // Find existing score
        var existingScore = await _context.EvaluatorScores
            .FirstOrDefaultAsync(s =>
                s.StudentId == studentId &&
                s.QuestionId == questionId &&
                s.QuestionnaireId == questionnaireId &&
                s.EvaluatorId == evaluatorId);

        if (existingScore != null)
        {
            // Check if finalized
            if (existingScore.IsFinalized)
            {
                throw new InvalidOperationException("Cannot update finalized score");
            }

            // Update score
            existingScore.Score = score;
            existingScore.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new score
            existingScore = new EvaluatorScore
            {
                StudentId = studentId,
                QuestionId = questionId,
                QuestionnaireId = questionnaireId,
                EvaluatorId = evaluatorId,
                Score = score,
                IsFinalized = false
            };

            _context.EvaluatorScores.Add(existingScore);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            $"Saved score for Student {studentId}, Question {questionId} by Evaluator {evaluatorId}: {score}");

        return existingScore;
    }

    public async Task FinalizeScoresAsync(
        Guid studentId,
        Guid questionnaireId,
        Guid evaluatorId)
    {
        var scores = await _context.EvaluatorScores
            .Where(s =>
                s.StudentId == studentId &&
                s.QuestionnaireId == questionnaireId &&
                s.EvaluatorId == evaluatorId)
            .ToListAsync();

        if (scores.Count == 0)
        {
            throw new InvalidOperationException("No scores to finalize");
        }

        // Check if any already finalized
        if (scores.Any(s => s.IsFinalized))
        {
            throw new InvalidOperationException("Some scores are already finalized");
        }

        // Finalize all scores
        foreach (var score in scores)
        {
            score.IsFinalized = true;
            score.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Recalculate combined scores
        await RecalculateCombinedScoresAsync(studentId, questionnaireId);

        _logger.LogInformation(
            $"Finalized scores for Student {studentId}, Questionnaire {questionnaireId} by Evaluator {evaluatorId}");
    }

    public async Task<List<CombinedScore>> GetCombinedScoresAsync(
        Guid studentId,
        Guid questionnaireId)
    {
        return await _context.CombinedScores
            .Where(s =>
                s.StudentId == studentId &&
                s.QuestionnaireId == questionnaireId)
            .Include(s => s.Question)
            .OrderBy(s => s.Question.OrderIndex)
            .ToListAsync();
    }

    private async Task RecalculateCombinedScoresAsync(Guid studentId, Guid questionnaireId)
    {
        // Get all finalized evaluator scores for this student and questionnaire
        var evaluatorScores = await _context.EvaluatorScores
            .Where(s =>
                s.StudentId == studentId &&
                s.QuestionnaireId == questionnaireId &&
                s.IsFinalized)
            .ToListAsync();

        if (evaluatorScores.Count == 0)
        {
            _logger.LogInformation($"No finalized scores to calculate for Student {studentId}, Questionnaire {questionnaireId}");
            return;
        }

        // Group by question and calculate average
        var questionGroups = evaluatorScores.GroupBy(s => s.QuestionId);

        foreach (var group in questionGroups)
        {
            var questionId = group.Key;
            var scores = group.ToList();
            var averageScore = scores.Average(s => s.Score);
            var evaluatorCount = scores.Count;

            // Find or create combined score
            var combinedScore = await _context.CombinedScores
                .FirstOrDefaultAsync(cs =>
                    cs.StudentId == studentId &&
                    cs.QuestionId == questionId &&
                    cs.QuestionnaireId == questionnaireId);

            if (combinedScore != null)
            {
                // Update existing
                combinedScore.AverageScore = averageScore;
                combinedScore.EvaluatorCount = evaluatorCount;
                combinedScore.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                combinedScore = new CombinedScore
                {
                    StudentId = studentId,
                    QuestionId = questionId,
                    QuestionnaireId = questionnaireId,
                    AverageScore = averageScore,
                    EvaluatorCount = evaluatorCount,
                    IsFinalized = false
                };

                _context.CombinedScores.Add(combinedScore);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            $"Recalculated combined scores for Student {studentId}, Questionnaire {questionnaireId}");
    }

    public async Task FinalizeCombinedScoresAsync(Guid studentId, Guid questionnaireId)
    {
        var combinedScores = await _context.CombinedScores
            .Where(s =>
                s.StudentId == studentId &&
                s.QuestionnaireId == questionnaireId)
            .ToListAsync();

        if (combinedScores.Count == 0)
        {
            throw new InvalidOperationException("No combined scores to finalize");
        }

        // Check if any already finalized
        if (combinedScores.Any(s => s.IsFinalized))
        {
            throw new InvalidOperationException("Some combined scores are already finalized");
        }

        // Finalize all combined scores
        foreach (var score in combinedScores)
        {
            score.IsFinalized = true;
            score.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            $"Finalized combined scores for Student {studentId}, Questionnaire {questionnaireId}");
    }
}
