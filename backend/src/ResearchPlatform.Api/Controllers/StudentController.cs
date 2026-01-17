using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchPlatform.Api.Auth;
using ResearchPlatform.Api.DTOs;
using ResearchPlatform.Api.Models.Enums;
using ResearchPlatform.Api.Services;
using System.Text.Json;

namespace ResearchPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/student")]
[Authorize(Policy = AuthorizationPolicies.StudentOnly)]
public class StudentController : ControllerBase
{
    private readonly QuestionnaireService _questionnaireService;
    private readonly AnswerService _answerService;
    private readonly ScoreCalculationService _scoreService;
    private readonly StepTimingService _stepTimingService;
    private readonly MaterialService _materialService;
    private readonly MaterialAccessService _materialAccessService;
    private readonly PostTestBatchService _postTestBatchService;

    public StudentController(
        QuestionnaireService questionnaireService,
        AnswerService answerService,
        ScoreCalculationService scoreService,
        StepTimingService stepTimingService,
        MaterialService materialService,
        MaterialAccessService materialAccessService,
        PostTestBatchService postTestBatchService)
    {
        _questionnaireService = questionnaireService;
        _answerService = answerService;
        _scoreService = scoreService;
        _stepTimingService = stepTimingService;
        _materialService = materialService;
        _materialAccessService = materialAccessService;
        _postTestBatchService = postTestBatchService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpGet("status")]
    public async Task<ActionResult<StudentStatusDto>> GetStatus()
    {
        var userId = GetCurrentUserId();

        // Get active questionnaires
        var pretestQuestionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Pretest);
        var posttestQuestionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Posttest);

        bool pretestCompleted = false;
        DateTime? pretestCompletedAt = null;
        bool posttestCompleted = false;
        DateTime? posttestCompletedAt = null;

        if (pretestQuestionnaire != null)
        {
            pretestCompleted = await _answerService.HasSubmittedAnswersAsync(userId, pretestQuestionnaire.Id);
            if (pretestCompleted)
            {
                var answers = await _answerService.GetUserAnswersAsync(userId, pretestQuestionnaire.Id);
                pretestCompletedAt = answers.FirstOrDefault()?.SubmittedAt;
            }
        }

        if (posttestQuestionnaire != null)
        {
            posttestCompleted = await _answerService.HasSubmittedAnswersAsync(userId, posttestQuestionnaire.Id);
            if (posttestCompleted)
            {
                var answers = await _answerService.GetUserAnswersAsync(userId, posttestQuestionnaire.Id);
                posttestCompletedAt = answers.FirstOrDefault()?.SubmittedAt;
            }
        }

        // Get materials accessed count
        var materialsAccessed = await _materialAccessService.GetTotalUserAccessCountAsync(userId);

        return Ok(new StudentStatusDto(
            pretestCompleted,
            pretestCompletedAt,
            posttestCompleted,
            posttestCompletedAt,
            materialsAccessed
        ));
    }

    [HttpGet("pretest")]
    public async Task<ActionResult<QuestionnaireWithAnswersDto>> GetPretest()
    {
        var userId = GetCurrentUserId();

        var questionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Pretest);
        if (questionnaire == null)
        {
            return NotFound(new { error = "No active pretest questionnaire found" });
        }

        // Check if already submitted
        var isSubmitted = await _answerService.HasSubmittedAnswersAsync(userId, questionnaire.Id);

        // Get existing answers
        var userAnswers = await _answerService.GetUserAnswersAsync(userId, questionnaire.Id);
        var totalSteps = await _questionnaireService.GetTotalStepsAsync(questionnaire.Id);

        var questions = questionnaire.Questions.Select(q => new QuestionDto(
            q.Id,
            q.Text,
            q.Type,
            string.IsNullOrEmpty(q.Options) ? null : JsonSerializer.Deserialize<List<string>>(q.Options),
            q.OrderIndex,
            q.Step,
            q.IsRequired,
            q.MinValue,
            q.MaxValue,
            q.MinLabel,
            q.MaxLabel
        )).ToList();

        var answers = userAnswers.Select(a => new AnswerDto(a.QuestionId, a.Value)).ToList();

        return Ok(new QuestionnaireWithAnswersDto(
            questionnaire.Id,
            questionnaire.Title,
            questionnaire.Description,
            questionnaire.Type,
            questions,
            answers,
            totalSteps,
            isSubmitted
        ));
    }

    [HttpPost("pretest/save")]
    public async Task<ActionResult<SaveAnswersResponse>> SavePretestAnswers([FromBody] SaveAnswersRequest request)
    {
        var userId = GetCurrentUserId();

        var questionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Pretest);
        if (questionnaire == null)
        {
            return NotFound(new { error = "No active pretest questionnaire found" });
        }

        // Check if already submitted
        if (await _answerService.HasSubmittedAnswersAsync(userId, questionnaire.Id))
        {
            return BadRequest(new { error = "Cannot save answers - questionnaire already submitted" });
        }

        await _answerService.SaveAnswersAsync(userId, questionnaire.Id, request.Answers);

        return Ok(new SaveAnswersResponse(true, "Answers saved successfully"));
    }

    [HttpPost("pretest/submit")]
    public async Task<ActionResult<SubmissionResponseDto>> SubmitPretest()
    {
        var userId = GetCurrentUserId();

        var questionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Pretest);
        if (questionnaire == null)
        {
            return NotFound(new { error = "No active pretest questionnaire found" });
        }

        await _answerService.SubmitAnswersAsync(userId, questionnaire.Id);

        // Calculate auto-scores
        await _scoreService.CalculateAutoScoresAsync(userId, questionnaire.Id);

        return Ok(new SubmissionResponseDto(
            true,
            "Pretest submitted successfully",
            DateTime.UtcNow
        ));
    }

    [HttpPost("step-timing")]
    public async Task<IActionResult> RecordStepTiming([FromBody] StepTimingRequest request)
    {
        var userId = GetCurrentUserId();

        var questionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Pretest);
        if (questionnaire == null)
        {
            return NotFound(new { error = "No active questionnaire found" });
        }

        if (request.IsStart)
        {
            await _stepTimingService.StartStepAsync(userId, questionnaire.Id, request.Step);
        }
        else
        {
            await _stepTimingService.EndStepAsync(userId, questionnaire.Id, request.Step);
        }

        return NoContent();
    }

    [HttpGet("materials")]
    public async Task<ActionResult<List<MaterialDto>>> GetMaterials()
    {
        var userId = GetCurrentUserId();

        // Check if pretest is completed
        var pretestQuestionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Pretest);
        if (pretestQuestionnaire == null || !await _answerService.HasSubmittedAnswersAsync(userId, pretestQuestionnaire.Id))
        {
            return StatusCode(403, new { error = "Pre-test must be completed to access materials" });
        }

        var materials = await _materialService.GetActiveMaterialsAsync();
        var accessCounts = await _materialAccessService.GetMaterialAccessCounts(userId);

        var materialDtos = materials.Select(m => new MaterialDto(
            m.Id,
            m.Title,
            m.Description,
            m.Type,
            m.FileExtension,
            m.FileSizeBytes,
            accessCounts.GetValueOrDefault(m.Id, 0)
        )).ToList();

        return Ok(materialDtos);
    }

    [HttpGet("materials/{id}")]
    public async Task<ActionResult<MaterialDetailDto>> GetMaterialDetail(Guid id)
    {
        var userId = GetCurrentUserId();

        // Check if pretest is completed
        var pretestQuestionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Pretest);
        if (pretestQuestionnaire == null || !await _answerService.HasSubmittedAnswersAsync(userId, pretestQuestionnaire.Id))
        {
            return StatusCode(403, new { error = "Pre-test must be completed to access materials" });
        }

        var material = await _materialService.GetMaterialByIdAsync(id);
        if (material == null)
        {
            return NotFound(new { error = "Material not found" });
        }

        // Generate signed URL for accessing the material
        var signedUrl = await _materialService.GetMaterialSignedUrlAsync(id, expirySeconds: 3600);
        var accessCount = await _materialAccessService.GetUserAccessCountAsync(userId, id);

        return Ok(new MaterialDetailDto(
            material.Id,
            material.Title,
            material.Description,
            material.Type,
            material.FileExtension,
            material.FileSizeBytes,
            signedUrl,
            accessCount
        ));
    }

    [HttpPost("materials/{id}/track")]
    public async Task<IActionResult> TrackMaterialAccess(Guid id, [FromBody] TrackMaterialAccessRequest request)
    {
        var userId = GetCurrentUserId();

        // Check if material exists
        var material = await _materialService.GetMaterialByIdAsync(id);
        if (material == null)
        {
            return NotFound(new { error = "Material not found" });
        }

        await _materialAccessService.TrackAccessAsync(
            userId,
            id,
            request.DurationSeconds,
            request.Completed
        );

        return NoContent();
    }

    [HttpGet("posttest")]
    public async Task<ActionResult<QuestionnaireWithAnswersDto>> GetPosttest()
    {
        var userId = GetCurrentUserId();

        // Check if post-test batch is open
        var isAvailable = await _postTestBatchService.IsPostTestAvailableAsync();
        if (!isAvailable)
        {
            return StatusCode(403, new { error = "Post-test is not currently available" });
        }

        // Check if pretest is completed
        var pretestQuestionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Pretest);
        if (pretestQuestionnaire == null || !await _answerService.HasSubmittedAnswersAsync(userId, pretestQuestionnaire.Id))
        {
            return StatusCode(403, new { error = "Pre-test must be completed before accessing post-test" });
        }

        var questionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Posttest);
        if (questionnaire == null)
        {
            return NotFound(new { error = "No active posttest questionnaire found" });
        }

        // Check if already submitted
        var isSubmitted = await _answerService.HasSubmittedAnswersAsync(userId, questionnaire.Id);

        // Get existing answers
        var userAnswers = await _answerService.GetUserAnswersAsync(userId, questionnaire.Id);
        var totalSteps = await _questionnaireService.GetTotalStepsAsync(questionnaire.Id);

        var questions = questionnaire.Questions.Select(q => new QuestionDto(
            q.Id,
            q.Text,
            q.Type,
            string.IsNullOrEmpty(q.Options) ? null : JsonSerializer.Deserialize<List<string>>(q.Options),
            q.OrderIndex,
            q.Step,
            q.IsRequired,
            q.MinValue,
            q.MaxValue,
            q.MinLabel,
            q.MaxLabel
        )).ToList();

        var answers = userAnswers.Select(a => new AnswerDto(a.QuestionId, a.Value)).ToList();

        return Ok(new QuestionnaireWithAnswersDto(
            questionnaire.Id,
            questionnaire.Title,
            questionnaire.Description,
            questionnaire.Type,
            questions,
            answers,
            totalSteps,
            isSubmitted
        ));
    }

    [HttpPost("posttest/save")]
    public async Task<ActionResult<SaveAnswersResponse>> SavePosttestAnswers([FromBody] SaveAnswersRequest request)
    {
        var userId = GetCurrentUserId();

        // Check if post-test batch is open
        var isAvailable = await _postTestBatchService.IsPostTestAvailableAsync();
        if (!isAvailable)
        {
            return StatusCode(403, new { error = "Post-test is not currently available" });
        }

        var questionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Posttest);
        if (questionnaire == null)
        {
            return NotFound(new { error = "No active posttest questionnaire found" });
        }

        // Check if already submitted
        if (await _answerService.HasSubmittedAnswersAsync(userId, questionnaire.Id))
        {
            return BadRequest(new { error = "Cannot save answers - questionnaire already submitted" });
        }

        await _answerService.SaveAnswersAsync(userId, questionnaire.Id, request.Answers);

        return Ok(new SaveAnswersResponse(true, "Answers saved successfully"));
    }

    [HttpPost("posttest/submit")]
    public async Task<ActionResult<SubmissionResponseDto>> SubmitPosttest()
    {
        var userId = GetCurrentUserId();

        // Check if post-test batch is open
        var isAvailable = await _postTestBatchService.IsPostTestAvailableAsync();
        if (!isAvailable)
        {
            return StatusCode(403, new { error = "Post-test is not currently available" });
        }

        var questionnaire = await _questionnaireService.GetActiveQuestionnaireAsync(QuestionnaireType.Posttest);
        if (questionnaire == null)
        {
            return NotFound(new { error = "No active posttest questionnaire found" });
        }

        await _answerService.SubmitAnswersAsync(userId, questionnaire.Id);

        // Calculate auto-scores
        await _scoreService.CalculateAutoScoresAsync(userId, questionnaire.Id);

        return Ok(new SubmissionResponseDto(
            true,
            "Posttest submitted successfully",
            DateTime.UtcNow
        ));
    }
}
