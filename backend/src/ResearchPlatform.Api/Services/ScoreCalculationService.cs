using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;
using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.Services;

public class ScoreCalculationService
{
    private readonly ApplicationDbContext _context;

    public ScoreCalculationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CalculateAutoScoresAsync(Guid userId, Guid questionnaireId)
    {
        var answers = await _context.Answers
            .Include(a => a.Question)
            .Where(a => a.UserId == userId && a.QuestionnaireId == questionnaireId && a.IsSubmitted)
            .ToListAsync();

        foreach (var answer in answers)
        {
            var autoScore = CalculateQuestionScore(answer);

            if (autoScore.HasValue)
            {
                var score = await _context.Scores
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.QuestionnaireId == questionnaireId && s.QuestionId == answer.QuestionId);

                if (score == null)
                {
                    score = new Score
                    {
                        UserId = userId,
                        QuestionnaireId = questionnaireId,
                        QuestionId = answer.QuestionId,
                        AutoScore = autoScore
                    };
                    _context.Scores.Add(score);
                }
                else
                {
                    score.AutoScore = autoScore;
                    score.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private decimal? CalculateQuestionScore(Answer answer)
    {
        // Auto-scoring logic based on question type
        return answer.Question.Type switch
        {
            QuestionType.LikertScale => CalculateLikertScore(answer),
            QuestionType.TrueFalse => CalculateTrueFalseScore(answer),
            QuestionType.MultipleChoice => null, // Requires manual scoring
            QuestionType.Dropdown => null, // Requires manual scoring
            QuestionType.TextField => null, // Requires manual scoring
            _ => null
        };
    }

    private decimal? CalculateLikertScore(Answer answer)
    {
        if (decimal.TryParse(answer.Value, out var value))
        {
            return value;
        }
        return null;
    }

    private decimal? CalculateTrueFalseScore(Answer answer)
    {
        // For true/false questions, convert to 1 or 0
        return answer.Value?.ToLower() switch
        {
            "true" => 1,
            "false" => 0,
            _ => null
        };
    }

    public async Task<decimal?> GetTotalScoreAsync(Guid userId, Guid questionnaireId)
    {
        return await _context.Scores
            .Where(s => s.UserId == userId && s.QuestionnaireId == questionnaireId)
            .SumAsync(s => s.ManualScore ?? s.AutoScore);
    }
}
