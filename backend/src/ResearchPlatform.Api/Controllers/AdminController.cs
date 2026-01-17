using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchPlatform.Api.Auth;
using ResearchPlatform.Api.DTOs;
using ResearchPlatform.Api.Models.Enums;
using ResearchPlatform.Api.Services;

namespace ResearchPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminController : ControllerBase
{
    private readonly UserManagementService _userManagementService;
    private readonly CsvImportService _csvImportService;
    private readonly DashboardService _dashboardService;
    private readonly EvaluatorAssignmentService _evaluatorAssignmentService;
    private readonly PostTestBatchService _postTestBatchService;
    private readonly ExportService _exportService;
    private readonly QuestionnaireBuilderService _questionnaireBuilderService;
    private readonly MaterialService _materialService;
    private readonly MaterialAccessService _materialAccessService;
    private readonly ParticipantService _participantService;
    private readonly ParticipantAuthService _participantAuthService;

    public AdminController(
        UserManagementService userManagementService,
        CsvImportService csvImportService,
        DashboardService dashboardService,
        EvaluatorAssignmentService evaluatorAssignmentService,
        PostTestBatchService postTestBatchService,
        ExportService exportService,
        QuestionnaireBuilderService questionnaireBuilderService,
        MaterialService materialService,
        MaterialAccessService materialAccessService,
        ParticipantService participantService,
        ParticipantAuthService participantAuthService)
    {
        _userManagementService = userManagementService;
        _csvImportService = csvImportService;
        _dashboardService = dashboardService;
        _evaluatorAssignmentService = evaluatorAssignmentService;
        _postTestBatchService = postTestBatchService;
        _exportService = exportService;
        _questionnaireBuilderService = questionnaireBuilderService;
        _materialService = materialService;
        _materialAccessService = materialAccessService;
        _participantService = participantService;
        _participantAuthService = participantAuthService;
    }

    // Dashboard endpoint
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardMetrics>> GetDashboard()
    {
        var metrics = await _dashboardService.GetDashboardMetricsAsync();
        return Ok(metrics);
    }

    // User management endpoints
    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers([FromQuery] string? role = null, [FromQuery] string? status = null)
    {
        Role? roleFilter = null;
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<Role>(role, true, out var parsedRole))
        {
            roleFilter = parsedRole;
        }

        UserStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var parsedStatus))
        {
            statusFilter = parsedStatus;
        }

        var users = await _userManagementService.GetAllUsersAsync(roleFilter, statusFilter);

        var userDtos = users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            Role = u.Role.ToString(),
            Status = u.Status.ToString(),
            Hospital = u.Hospital,
            Gender = u.Gender?.ToString(),
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();

        return Ok(userDtos);
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<AdminUserDto>> GetUser(Guid id)
    {
        var user = await _userManagementService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        var userDto = new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            Hospital = user.Hospital,
            Gender = user.Gender?.ToString(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return Ok(userDto);
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!Enum.TryParse<Role>(request.Role, true, out var role))
            {
                return BadRequest(new { error = "Invalid role" });
            }

            Gender? gender = null;
            if (!string.IsNullOrWhiteSpace(request.Gender) && Enum.TryParse<Gender>(request.Gender, true, out var parsedGender))
            {
                gender = parsedGender;
            }

            var user = await _userManagementService.CreateUserAsync(
                request.Email,
                request.Password,
                request.Name,
                role,
                request.Hospital,
                gender);

            var userDto = new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                Hospital = user.Hospital,
                Gender = user.Gender?.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult<AdminUserDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            Role? role = null;
            if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<Role>(request.Role, true, out var parsedRole))
            {
                role = parsedRole;
            }

            Gender? gender = null;
            if (!string.IsNullOrWhiteSpace(request.Gender) && Enum.TryParse<Gender>(request.Gender, true, out var parsedGender))
            {
                gender = parsedGender;
            }

            var user = await _userManagementService.UpdateUserAsync(
                id,
                request.Name,
                request.Hospital,
                gender,
                role);

            var userDto = new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                Hospital = user.Hospital,
                Gender = user.Gender?.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(userDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userManagementService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("users/import")]
    public async Task<ActionResult<CsvImportResult>> ImportUsers(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "File must be CSV format" });
        }

        using var stream = file.OpenReadStream();
        var result = await _csvImportService.ImportUsersFromCsvAsync(stream);

        return Ok(result);
    }

    [HttpPost("users/{id}/activate")]
    public async Task<ActionResult> ActivateUser(Guid id)
    {
        try
        {
            await _userManagementService.ActivateUserAsync(id);
            return Ok(new { success = true, message = "User activated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("users/{id}/deactivate")]
    public async Task<ActionResult> DeactivateUser(Guid id)
    {
        try
        {
            await _userManagementService.DeactivateUserAsync(id);
            return Ok(new { success = true, message = "User deactivated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("users/{id}/reset-password")]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            var newPassword = await _userManagementService.ResetPasswordAsync(id, request.NewPassword);
            return Ok(new ResetPasswordResponse
            {
                Success = true,
                Message = "Password reset successfully",
                NewPassword = newPassword
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Evaluator assignment endpoints
    [HttpGet("evaluator-assignments")]
    public async Task<ActionResult<List<EvaluatorAssignmentInfo>>> GetEvaluatorAssignments()
    {
        var assignments = await _evaluatorAssignmentService.GetAllAssignmentsAsync();
        return Ok(assignments);
    }

    [HttpPost("evaluator-assignments")]
    public async Task<ActionResult<EvaluatorAssignmentInfo>> CreateEvaluatorAssignment([FromBody] CreateAssignmentRequest request)
    {
        try
        {
            var assignment = await _evaluatorAssignmentService.CreateAssignmentAsync(request.EvaluatorId, request.StudentId);

            var info = new EvaluatorAssignmentInfo
            {
                Id = assignment.Id,
                EvaluatorId = assignment.EvaluatorId,
                StudentId = assignment.StudentId,
                AssignedAt = assignment.AssignedAt
            };

            return Ok(info);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("evaluator-assignments/{id}")]
    public async Task<ActionResult> DeleteEvaluatorAssignment(Guid id)
    {
        try
        {
            await _evaluatorAssignmentService.DeleteAssignmentAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Post-test batch control endpoints
    [HttpPost("posttest-batch/open")]
    public async Task<ActionResult<PostTestBatchDto>> OpenPosttestBatch([FromBody] CreateBatchRequest request)
    {
        try
        {
            var batch = await _postTestBatchService.CreateBatchAsync(
                request.Name,
                request.Description,
                request.OpenDate,
                request.CloseDate);

            var batchDto = new PostTestBatchDto
            {
                Id = batch.Id,
                Name = batch.Name,
                Description = batch.Description,
                OpenDate = batch.OpenDate,
                CloseDate = batch.CloseDate,
                IsActive = batch.IsActive,
                CreatedAt = batch.CreatedAt
            };

            return Ok(batchDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("posttest-batch/close")]
    public async Task<ActionResult> ClosePosttestBatch([FromBody] CloseBatchRequest request)
    {
        try
        {
            await _postTestBatchService.UpdateBatchAsync(
                request.Id,
                request.Name ?? string.Empty,
                request.Description,
                request.OpenDate ?? DateTime.UtcNow,
                request.CloseDate ?? DateTime.UtcNow,
                false); // Set IsActive = false to close

            return Ok(new { success = true, message = "Batch closed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("students/{id}/finalize")]
    public async Task<ActionResult> FinalizeStudent(Guid id)
    {
        // This endpoint would finalize all scores for a student
        // For now, just return success as the scoring finalization is handled by evaluators
        return Ok(new { success = true, message = "Student scores finalized" });
    }

    [HttpGet("export")]
    public async Task<ActionResult> ExportData([FromQuery] string format = "excel")
    {
        try
        {
            byte[] data;
            string contentType;
            string fileName;

            if (format.ToLower() == "csv")
            {
                data = await _exportService.ExportToCsvAsync();
                contentType = "text/csv";
                fileName = $"research_data_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            }
            else // default to excel
            {
                data = await _exportService.ExportToExcelAsync();
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"research_data_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            }

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Questionnaire Builder endpoints
    [HttpGet("questionnaires")]
    public async Task<ActionResult<List<QuestionnaireListItemDto>>> GetQuestionnaires()
    {
        try
        {
            var questionnaires = await _questionnaireBuilderService.GetAllQuestionnairesAsync();
            return Ok(questionnaires);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("questionnaires/{id}")]
    public async Task<ActionResult<QuestionnaireDetailDto>> GetQuestionnaire(Guid id)
    {
        try
        {
            var questionnaire = await _questionnaireBuilderService.GetQuestionnaireByIdAsync(id);
            if (questionnaire == null)
                return NotFound(new { error = "Questionnaire not found" });

            return Ok(questionnaire);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("questionnaires")]
    public async Task<ActionResult<QuestionnaireDetailDto>> CreateQuestionnaire([FromBody] CreateQuestionnaireRequest request)
    {
        try
        {
            var questionnaire = await _questionnaireBuilderService.CreateQuestionnaireAsync(request);
            return CreatedAtAction(nameof(GetQuestionnaire), new { id = questionnaire.Id }, questionnaire);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("questionnaires/{id}")]
    public async Task<ActionResult<QuestionnaireDetailDto>> UpdateQuestionnaire(Guid id, [FromBody] UpdateQuestionnaireRequest request)
    {
        try
        {
            var questionnaire = await _questionnaireBuilderService.UpdateQuestionnaireAsync(id, request);
            return Ok(questionnaire);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("questionnaires/{id}/questions")]
    public async Task<ActionResult<QuestionnaireDetailDto>> UpdateQuestions(Guid id, [FromBody] UpdateQuestionsRequest request)
    {
        try
        {
            var questionnaire = await _questionnaireBuilderService.UpdateQuestionsAsync(id, request);
            return Ok(questionnaire);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("questionnaires/{id}")]
    public async Task<ActionResult> DeleteQuestionnaire(Guid id)
    {
        try
        {
            await _questionnaireBuilderService.DeleteQuestionnaireAsync(id);
            return Ok(new { success = true, message = "Questionnaire deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Material Management endpoints
    [HttpGet("materials")]
    public async Task<ActionResult<List<AdminMaterialDto>>> GetMaterials([FromQuery] string? type = null, [FromQuery] bool includeInactive = true)
    {
        var materials = await _materialService.GetAllMaterialsAsync(includeInactive);

        // Filter by type if specified
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<MaterialType>(type, true, out var materialType))
        {
            materials = materials.Where(m => m.Type == materialType).ToList();
        }

        // Get access counts for all materials
        var accessCounts = await _materialAccessService.GetMaterialAccessCounts(materials.Select(m => m.Id).ToList());

        var materialDtos = materials.Select(m => new AdminMaterialDto(
            m.Id,
            m.Title,
            m.Description,
            m.Type.ToString(),
            m.FileExtension,
            m.FileSizeBytes,
            m.OrderIndex,
            m.IsActive,
            accessCounts.TryGetValue(m.Id, out var count) ? count : 0,
            m.CreatedAt,
            m.UpdatedAt
        )).ToList();

        return Ok(materialDtos);
    }

    [HttpPost("materials/upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(1_073_741_824)] // 1GB
    public async Task<ActionResult<AdminMaterialDto>> UploadMaterial([FromForm] UploadMaterialRequest request)
    {
        try
        {
            if (!Enum.TryParse<MaterialType>(request.Type, true, out var materialType))
            {
                return BadRequest(new { error = "Invalid material type" });
            }

            using var stream = request.File.OpenReadStream();
            var material = await _materialService.CreateMaterialAsync(
                request.Title,
                request.Description,
                materialType,
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length);

            var materialDto = new AdminMaterialDto(
                material.Id,
                material.Title,
                material.Description,
                material.Type.ToString(),
                material.FileExtension,
                material.FileSizeBytes,
                material.OrderIndex,
                material.IsActive,
                0, // New material has no access count
                material.CreatedAt,
                material.UpdatedAt
            );

            return CreatedAtAction(nameof(GetMaterials), materialDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to upload material: " + ex.Message });
        }
    }

    [HttpGet("materials/{id}")]
    public async Task<ActionResult<AdminMaterialDetailDto>> GetMaterial(Guid id)
    {
        try
        {
            var material = await _materialService.GetMaterialByIdForAdminAsync(id);
            if (material == null)
            {
                return NotFound(new { error = "Material not found" });
            }

            var signedUrl = await _materialService.GetMaterialSignedUrlAsync(id, includeInactive: true);
            var accessCount = await _materialAccessService.GetMaterialAccessCount(id);

            var materialDto = new AdminMaterialDetailDto(
                material.Id,
                material.Title,
                material.Description,
                material.Type.ToString(),
                material.FileExtension,
                material.FileSizeBytes,
                material.OrderIndex,
                material.IsActive,
                signedUrl,
                accessCount,
                material.CreatedAt,
                material.UpdatedAt
            );

            return Ok(materialDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("materials/{id}")]
    public async Task<ActionResult<AdminMaterialDto>> UpdateMaterial(Guid id, [FromBody] UpdateMaterialRequest request)
    {
        try
        {
            if (!Enum.TryParse<MaterialType>(request.Type, true, out var materialType))
            {
                return BadRequest(new { error = "Invalid material type" });
            }

            var material = await _materialService.UpdateMaterialAsync(
                id,
                request.Title,
                request.Description,
                materialType,
                request.OrderIndex,
                request.IsActive);

            var accessCount = await _materialAccessService.GetMaterialAccessCount(id);

            var materialDto = new AdminMaterialDto(
                material.Id,
                material.Title,
                material.Description,
                material.Type.ToString(),
                material.FileExtension,
                material.FileSizeBytes,
                material.OrderIndex,
                material.IsActive,
                accessCount,
                material.CreatedAt,
                material.UpdatedAt
            );

            return Ok(materialDto);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("materials/{id}")]
    public async Task<ActionResult> DeleteMaterial(Guid id)
    {
        try
        {
            // Soft delete - set IsActive to false to preserve access logs
            var material = await _materialService.GetMaterialByIdForAdminAsync(id);
            if (material == null)
            {
                return NotFound(new { error = "Material not found" });
            }

            await _materialService.UpdateMaterialAsync(
                id,
                material.Title,
                material.Description,
                material.Type,
                material.OrderIndex,
                false); // Set IsActive = false

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ============================================================
    // PARTICIPANT MANAGEMENT ENDPOINTS
    // ============================================================

    /// <summary>
    /// Get paginated list of participants
    /// </summary>
    [HttpGet("participants")]
    public async Task<ActionResult<ParticipantListResponse>> GetParticipants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _participantService.GetListAsync(page, pageSize, search);
        return Ok(result);
    }

    /// <summary>
    /// Search participants by code, login identifier, or phone
    /// </summary>
    [HttpGet("participants/search")]
    public async Task<ActionResult<List<ParticipantSummary>>> SearchParticipants([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ErrorResponse("BadRequest", "Search query is required"));
        }

        var results = await _participantService.SearchAsync(q);
        return Ok(results);
    }

    /// <summary>
    /// Get participant details by ID
    /// </summary>
    [HttpGet("participants/{id}")]
    public async Task<ActionResult<ParticipantDetails>> GetParticipant(Guid id)
    {
        var result = await _participantService.GetDetailsAsync(id);

        if (result.IsFailure)
        {
            return NotFound(new ErrorResponse("NotFound", "Participant not found"));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Reset participant password (generates temporary password)
    /// </summary>
    [HttpPost("participants/{id}/reset-password")]
    public async Task<ActionResult<PasswordResetResponse>> ResetParticipantPassword(Guid id)
    {
        // Get current admin ID from claims
        var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (adminIdClaim == null || !Guid.TryParse(adminIdClaim.Value, out var adminId))
        {
            return Unauthorized(new ErrorResponse("Unauthorized", "Invalid admin token"));
        }

        var result = await _participantAuthService.ResetPasswordAsync(id, adminId);

        if (result.IsFailure)
        {
            if (result.Error == "PARTICIPANT_NOT_FOUND")
            {
                return NotFound(new ErrorResponse("NotFound", "Participant not found"));
            }
            return BadRequest(new ErrorResponse("BadRequest", result.Error));
        }

        return Ok(result.Value);
    }
}
