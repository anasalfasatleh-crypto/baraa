using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.DTOs;
using ResearchPlatform.Api.Models;
using ResearchPlatform.Api.Models.Enums;
using System.Text.Json;

namespace ResearchPlatform.Api.Services;

public class QuestionnaireBuilderService
{
    private readonly ApplicationDbContext _context;

    public QuestionnaireBuilderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestionnaireListItemDto>> GetAllQuestionnairesAsync()
    {
        var questionnaires = await _context.Questionnaires
            .Include(q => q.Questions)
            .OrderByDescending(q => q.UpdatedAt)
            .ToListAsync();

        return questionnaires.Select(q => new QuestionnaireListItemDto(
            q.Id,
            q.Title,
            q.Description,
            q.Type,
            q.IsActive,
            q.Questions.Count,
            q.CreatedAt,
            q.UpdatedAt
        )).ToList();
    }

    public async Task<QuestionnaireDetailDto?> GetQuestionnaireByIdAsync(Guid id)
    {
        var questionnaire = await _context.Questionnaires
            .Include(q => q.Questions.OrderBy(qu => qu.OrderIndex))
            .FirstOrDefaultAsync(q => q.Id == id);

        if (questionnaire == null)
            return null;

        var questions = questionnaire.Questions
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuestionBuilderDto(
                q.Id,
                q.Text,
                q.Type,
                ParseOptions(q.Options),
                q.OrderIndex,
                q.Step,
                q.IsRequired,
                q.MinValue,
                q.MaxValue,
                q.MinLabel,
                q.MaxLabel
            )).ToList();

        return new QuestionnaireDetailDto(
            questionnaire.Id,
            questionnaire.Title,
            questionnaire.Description,
            questionnaire.Type,
            questionnaire.IsActive,
            questions,
            questionnaire.CreatedAt,
            questionnaire.UpdatedAt
        );
    }

    public async Task<QuestionnaireDetailDto> CreateQuestionnaireAsync(CreateQuestionnaireRequest request)
    {
        var questionnaire = new Questionnaire
        {
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            IsActive = false, // Start as inactive
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Questionnaires.Add(questionnaire);
        await _context.SaveChangesAsync();

        return new QuestionnaireDetailDto(
            questionnaire.Id,
            questionnaire.Title,
            questionnaire.Description,
            questionnaire.Type,
            questionnaire.IsActive,
            new List<QuestionBuilderDto>(),
            questionnaire.CreatedAt,
            questionnaire.UpdatedAt
        );
    }

    public async Task<QuestionnaireDetailDto> UpdateQuestionnaireAsync(Guid id, UpdateQuestionnaireRequest request)
    {
        var questionnaire = await _context.Questionnaires
            .Include(q => q.Questions.OrderBy(qu => qu.OrderIndex))
            .FirstOrDefaultAsync(q => q.Id == id);

        if (questionnaire == null)
            throw new InvalidOperationException("Questionnaire not found");

        questionnaire.Title = request.Title;
        questionnaire.Description = request.Description;
        questionnaire.Type = request.Type;
        questionnaire.IsActive = request.IsActive;
        questionnaire.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var questions = questionnaire.Questions
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuestionBuilderDto(
                q.Id,
                q.Text,
                q.Type,
                ParseOptions(q.Options),
                q.OrderIndex,
                q.Step,
                q.IsRequired,
                q.MinValue,
                q.MaxValue,
                q.MinLabel,
                q.MaxLabel
            )).ToList();

        return new QuestionnaireDetailDto(
            questionnaire.Id,
            questionnaire.Title,
            questionnaire.Description,
            questionnaire.Type,
            questionnaire.IsActive,
            questions,
            questionnaire.CreatedAt,
            questionnaire.UpdatedAt
        );
    }

    public async Task<QuestionnaireDetailDto> UpdateQuestionsAsync(Guid questionnaireId, UpdateQuestionsRequest request)
    {
        var questionnaire = await _context.Questionnaires
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == questionnaireId);

        if (questionnaire == null)
            throw new InvalidOperationException("Questionnaire not found");

        // Get existing question IDs
        var existingQuestionIds = questionnaire.Questions.Select(q => q.Id).ToHashSet();
        var requestQuestionIds = request.Questions
            .Where(q => q.Id.HasValue)
            .Select(q => q.Id!.Value)
            .ToHashSet();

        // Remove questions that are no longer in the request
        var questionsToRemove = questionnaire.Questions
            .Where(q => !requestQuestionIds.Contains(q.Id))
            .ToList();

        foreach (var question in questionsToRemove)
        {
            _context.Questions.Remove(question);
        }

        // Update or create questions
        foreach (var questionItem in request.Questions)
        {
            if (questionItem.Id.HasValue)
            {
                // Update existing question
                var existingQuestion = questionnaire.Questions.FirstOrDefault(q => q.Id == questionItem.Id.Value);
                if (existingQuestion != null)
                {
                    existingQuestion.Text = questionItem.Text;
                    existingQuestion.Type = questionItem.Type;
                    existingQuestion.Options = SerializeOptions(questionItem.Options);
                    existingQuestion.OrderIndex = questionItem.OrderIndex;
                    existingQuestion.Step = questionItem.Step;
                    existingQuestion.IsRequired = questionItem.IsRequired;
                    existingQuestion.MinValue = questionItem.MinValue;
                    existingQuestion.MaxValue = questionItem.MaxValue;
                    existingQuestion.MinLabel = questionItem.MinLabel;
                    existingQuestion.MaxLabel = questionItem.MaxLabel;
                }
            }
            else
            {
                // Create new question
                var newQuestion = new Question
                {
                    QuestionnaireId = questionnaireId,
                    Text = questionItem.Text,
                    Type = questionItem.Type,
                    Options = SerializeOptions(questionItem.Options),
                    OrderIndex = questionItem.OrderIndex,
                    Step = questionItem.Step,
                    IsRequired = questionItem.IsRequired,
                    MinValue = questionItem.MinValue,
                    MaxValue = questionItem.MaxValue,
                    MinLabel = questionItem.MinLabel,
                    MaxLabel = questionItem.MaxLabel,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Questions.Add(newQuestion);
            }
        }

        questionnaire.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload questionnaire with updated questions
        await _context.Entry(questionnaire).Collection(q => q.Questions).LoadAsync();

        var questions = questionnaire.Questions
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuestionBuilderDto(
                q.Id,
                q.Text,
                q.Type,
                ParseOptions(q.Options),
                q.OrderIndex,
                q.Step,
                q.IsRequired,
                q.MinValue,
                q.MaxValue,
                q.MinLabel,
                q.MaxLabel
            )).ToList();

        return new QuestionnaireDetailDto(
            questionnaire.Id,
            questionnaire.Title,
            questionnaire.Description,
            questionnaire.Type,
            questionnaire.IsActive,
            questions,
            questionnaire.CreatedAt,
            questionnaire.UpdatedAt
        );
    }

    public async Task DeleteQuestionnaireAsync(Guid id)
    {
        var questionnaire = await _context.Questionnaires
            .Include(q => q.Questions)
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (questionnaire == null)
            throw new InvalidOperationException("Questionnaire not found");

        // Check if questionnaire has any submitted answers
        if (questionnaire.Answers.Any(a => a.IsSubmitted))
        {
            throw new InvalidOperationException("Cannot delete questionnaire with submitted answers. Set IsActive to false instead.");
        }

        _context.Questionnaires.Remove(questionnaire);
        await _context.SaveChangesAsync();
    }

    private List<string>? ParseOptions(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson);
        }
        catch
        {
            return null;
        }
    }

    private string? SerializeOptions(List<string>? options)
    {
        if (options == null || options.Count == 0)
            return null;

        return JsonSerializer.Serialize(options);
    }
}
