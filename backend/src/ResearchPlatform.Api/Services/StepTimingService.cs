using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class StepTimingService
{
    private readonly ApplicationDbContext _context;

    public StepTimingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task StartStepAsync(Guid userId, Guid questionnaireId, int step)
    {
        // Check if this step timing already exists
        var existingTiming = await _context.StepTimings
            .FirstOrDefaultAsync(st => st.UserId == userId && st.QuestionnaireId == questionnaireId && st.Step == step);

        if (existingTiming == null)
        {
            var stepTiming = new StepTiming
            {
                UserId = userId,
                QuestionnaireId = questionnaireId,
                Step = step,
                StartTime = DateTime.UtcNow
            };

            _context.StepTimings.Add(stepTiming);
            await _context.SaveChangesAsync();
        }
    }

    public async Task EndStepAsync(Guid userId, Guid questionnaireId, int step)
    {
        var stepTiming = await _context.StepTimings
            .FirstOrDefaultAsync(st => st.UserId == userId && st.QuestionnaireId == questionnaireId && st.Step == step && st.EndTime == null);

        if (stepTiming != null)
        {
            stepTiming.EndTime = DateTime.UtcNow;
            stepTiming.TimeSpentSeconds = (int)(stepTiming.EndTime.Value - stepTiming.StartTime).TotalSeconds;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<StepTiming>> GetUserStepTimingsAsync(Guid userId, Guid questionnaireId)
    {
        return await _context.StepTimings
            .Where(st => st.UserId == userId && st.QuestionnaireId == questionnaireId)
            .OrderBy(st => st.Step)
            .ToListAsync();
    }

    public async Task<int?> GetTotalTimeSpentAsync(Guid userId, Guid questionnaireId)
    {
        return await _context.StepTimings
            .Where(st => st.UserId == userId && st.QuestionnaireId == questionnaireId && st.TimeSpentSeconds.HasValue)
            .SumAsync(st => st.TimeSpentSeconds);
    }
}
