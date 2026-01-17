using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchPlatform.Api.Auth;
using ResearchPlatform.Api.DTOs;
using ResearchPlatform.Api.Models.Enums;
using ResearchPlatform.Api.Services;

namespace ResearchPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/evaluator")]
[Authorize(Policy = AuthorizationPolicies.EvaluatorOnly)]
public class EvaluatorController : ControllerBase
{
    private readonly EvaluatorService _evaluatorService;
    private readonly EvaluatorScoreService _scoreService;
    private readonly QuestionnaireService _questionnaireService;
    private readonly AnswerService _answerService;

    public EvaluatorController(
        EvaluatorService evaluatorService,
        EvaluatorScoreService scoreService,
        QuestionnaireService questionnaireService,
        AnswerService answerService)
    {
        _evaluatorService = evaluatorService;
        _scoreService = scoreService;
        _questionnaireService = questionnaireService;
        _answerService = answerService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpGet("students")]
    public async Task<ActionResult<List<AssignedStudentDto>>> GetAssignedStudents()
    {
        var evaluatorId = GetCurrentUserId();

        var students = await _evaluatorService.GetAssignedStudentsAsync(evaluatorId);

        var result = students.Select(s => new AssignedStudentDto
        {
            Id = s.Id,
            Name = s.Name,
            Email = s.Email,
            Hospital = s.Hospital,
            Gender = s.Gender?.ToString()
        }).ToList();

        return Ok(result);
    }

    [HttpGet("students/{studentId}/responses")]
    public async Task<ActionResult<StudentResponsesDto>> GetStudentResponses(Guid studentId, [FromQuery] string? type = "pretest")
    {
        var evaluatorId = GetCurrentUserId();

        // Verify assignment
        var isAssigned = await _evaluatorService.IsStudentAssignedAsync(evaluatorId, studentId);
        if (!isAssigned)
        {
            return StatusCode(403, new { error = "You are not assigned to this student" });
        }

        // Parse questionnaire type
        if (!Enum.TryParse<QuestionnaireType>(type, true, out var questionnaireType))
        {
            return BadRequest(new { error = "Invalid questionnaire type" });
        }

        // Get active questionnaire
        var questionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(questionnaireType);
        if (questionnaire == null)
        {
            return NotFound(new { error = $"No active {type} questionnaire found" });
        }

        // Get student answers
        var answers = await _answerService.GetUserAnswersAsync(studentId, questionnaire.Id);
        if (!answers.Any() || !answers.First().IsSubmitted)
        {
            return NotFound(new { error = "Student has not submitted this questionnaire" });
        }

        // Get existing scores from this evaluator
        var existingScores = await _scoreService.GetStudentScoresAsync(studentId, questionnaire.Id, evaluatorId);
        var scoresDict = existingScores.ToDictionary(s => s.QuestionId, s => s);

        // Build response
        var questionResponses = questionnaire.Questions
            .OrderBy(q => q.OrderIndex)
            .Select(q =>
            {
                var answer = answers.FirstOrDefault(a => a.QuestionId == q.Id);
                var score = scoresDict.ContainsKey(q.Id) ? scoresDict[q.Id] : null;

                return new QuestionResponseDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    QuestionType = q.Type.ToString(),
                    Step = q.Step,
                    OrderIndex = q.OrderIndex,
                    IsRequired = q.IsRequired,
                    Answer = answer?.Value,
                    Score = score?.Score,
                    IsScoreFinalized = score?.IsFinalized ?? false
                };
            }).ToList();

        var response = new StudentResponsesDto
        {
            StudentId = studentId,
            QuestionnaireId = questionnaire.Id,
            QuestionnaireTitle = questionnaire.Title,
            QuestionnaireType = questionnaire.Type.ToString(),
            SubmittedAt = answers.First().SubmittedAt,
            Responses = questionResponses
        };

        return Ok(response);
    }

    [HttpPost("students/{studentId}/scores")]
    public async Task<ActionResult<SaveScoresResponseDto>> SaveScores(
        Guid studentId,
        [FromBody] SaveScoresRequestDto request)
    {
        var evaluatorId = GetCurrentUserId();

        // Verify assignment
        var isAssigned = await _evaluatorService.IsStudentAssignedAsync(evaluatorId, studentId);
        if (!isAssigned)
        {
            return StatusCode(403, new { error = "You are not assigned to this student" });
        }

        // Verify questionnaire exists
        var questionnaire = await _questionnaireService.GetQuestionnaireByIdAsync(request.QuestionnaireId);
        if (questionnaire == null)
        {
            return NotFound(new { error = "Questionnaire not found" });
        }

        try
        {
            // Save each score
            foreach (var scoreItem in request.Scores)
            {
                await _scoreService.SaveScoreAsync(
                    studentId,
                    scoreItem.QuestionId,
                    request.QuestionnaireId,
                    evaluatorId,
                    scoreItem.Score);
            }

            // Finalize if requested
            if (request.Finalize)
            {
                await _scoreService.FinalizeScoresAsync(studentId, request.QuestionnaireId, evaluatorId);
            }

            return Ok(new SaveScoresResponseDto
            {
                Success = true,
                Message = request.Finalize
                    ? "Scores saved and finalized successfully"
                    : "Scores saved successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
