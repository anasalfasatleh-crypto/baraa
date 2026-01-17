using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class AnswerService
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _auditService;

    public AnswerService(ApplicationDbContext context, AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<Answer>> GetUserAnswersAsync(Guid userId, Guid questionnaireId)
    {
        return await _context.Answers
            .Include(a => a.Question)
            .Where(a => a.UserId == userId && a.QuestionnaireId == questionnaireId)
            .ToListAsync();
    }

    public async Task<bool> HasSubmittedAnswersAsync(Guid userId, Guid questionnaireId)
    {
        return await _context.Answers
            .AnyAsync(a => a.UserId == userId && a.QuestionnaireId == questionnaireId && a.IsSubmitted);
    }

    public async Task SaveAnswersAsync(Guid userId, Guid questionnaireId, Dictionary<Guid, string> answers)
    {
        var existingAnswers = await _context.Answers
            .Where(a => a.UserId == userId && a.QuestionnaireId == questionnaireId && !a.IsSubmitted)
            .ToDictionaryAsync(a => a.QuestionId);

        foreach (var (questionId, value) in answers)
        {
            if (existingAnswers.TryGetValue(questionId, out var existing))
            {
                // Update existing draft answer
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new draft answer
                var newAnswer = new Answer
                {
                    UserId = userId,
                    QuestionnaireId = questionnaireId,
                    QuestionId = questionId,
                    Value = value,
                    IsSubmitted = false
                };
                _context.Answers.Add(newAnswer);
            }
        }

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("answer_saved", "Answer", null, null, null, userId);
    }

    public async Task SubmitAnswersAsync(Guid userId, Guid questionnaireId)
    {
        // Check if already submitted
        if (await HasSubmittedAnswersAsync(userId, questionnaireId))
        {
            throw new InvalidOperationException("Answers have already been submitted");
        }

        var answers = await _context.Answers
            .Where(a => a.UserId == userId && a.QuestionnaireId == questionnaireId && !a.IsSubmitted)
            .ToListAsync();

        if (answers.Count == 0)
        {
            throw new InvalidOperationException("No answers to submit");
        }

        // Mark all answers as submitted
        foreach (var answer in answers)
        {
            answer.IsSubmitted = true;
            answer.SubmittedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("questionnaire_submitted", "Answer", questionnaireId, null, new { answersCount = answers.Count }, userId);
    }
}
