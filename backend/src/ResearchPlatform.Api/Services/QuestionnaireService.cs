using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;
using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.Services;

public class QuestionnaireService
{
    private readonly ApplicationDbContext _context;

    public QuestionnaireService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Questionnaire?> GetActiveQuestionnaireAsync(QuestionnaireType type)
    {
        return await _context.Questionnaires
            .Include(q => q.Questions.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(q => q.Type == type && q.IsActive);
    }

    public async Task<Questionnaire?> GetQuestionnaireByIdAsync(Guid questionnaireId)
    {
        return await _context.Questionnaires
            .FirstOrDefaultAsync(q => q.Id == questionnaireId);
    }

    public async Task<Questionnaire?> GetQuestionnaireWithQuestionsAsync(Guid questionnaireId)
    {
        return await _context.Questionnaires
            .Include(q => q.Questions.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(q => q.Id == questionnaireId);
    }

    public async Task<List<Question>> GetQuestionsByStepAsync(Guid questionnaireId, int step)
    {
        return await _context.Questions
            .Where(q => q.QuestionnaireId == questionnaireId && q.Step == step)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();
    }

    public async Task<int> GetTotalStepsAsync(Guid questionnaireId)
    {
        return await _context.Questions
            .Where(q => q.QuestionnaireId == questionnaireId)
            .Select(q => q.Step)
            .Distinct()
            .CountAsync();
    }
}
