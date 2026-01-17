using FluentValidation;
using ResearchPlatform.Api.DTOs;

namespace ResearchPlatform.Api.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => role == "Student" || role == "Evaluator" || role == "Admin")
            .WithMessage("Role must be Student, Evaluator, or Admin");

        RuleFor(x => x.Hospital)
            .MaximumLength(100).WithMessage("Hospital must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Hospital));

        RuleFor(x => x.Gender)
            .MaximumLength(20).WithMessage("Gender must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Gender));
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Role)
            .Must(role => role == "Student" || role == "Evaluator" || role == "Admin")
            .WithMessage("Role must be Student, Evaluator, or Admin")
            .When(x => !string.IsNullOrEmpty(x.Role));

        RuleFor(x => x.Hospital)
            .MaximumLength(100).WithMessage("Hospital must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Hospital));

        RuleFor(x => x.Gender)
            .MaximumLength(20).WithMessage("Gender must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Gender));
    }
}

public class CreateBatchRequestValidator : AbstractValidator<CreateBatchRequest>
{
    public CreateBatchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.OpenDate)
            .NotEmpty().WithMessage("Open date is required");

        RuleFor(x => x.CloseDate)
            .NotEmpty().WithMessage("Close date is required")
            .GreaterThan(x => x.OpenDate).WithMessage("Close date must be after open date");
    }
}

public class CreateQuestionnaireRequestValidator : AbstractValidator<CreateQuestionnaireRequest>
{
    public CreateQuestionnaireRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid questionnaire type");
    }
}

public class UpdateQuestionnaireRequestValidator : AbstractValidator<UpdateQuestionnaireRequest>
{
    public UpdateQuestionnaireRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid questionnaire type");
    }
}

public class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(x => x.EvaluatorId)
            .NotEmpty().WithMessage("Evaluator ID is required");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
    }
}

public class UploadMaterialRequestValidator : AbstractValidator<UploadMaterialRequest>
{
    private static readonly string[] AllowedExtensions =
        { ".pdf", ".mp4", ".avi", ".mov", ".wmv", ".txt", ".doc", ".docx" };

    private const long MaxFileSize = 1_073_741_824; // 1GB

    public UploadMaterialRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(type => type == "Pdf" || type == "Video" || type == "Text")
            .WithMessage("Type must be Pdf, Video, or Text");

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required")
            .Must(f => f != null && f.Length <= MaxFileSize)
            .WithMessage("File size must not exceed 1GB")
            .Must(f => f != null && AllowedExtensions.Contains(
                Path.GetExtension(f.FileName).ToLowerInvariant()))
            .WithMessage($"File type not supported. Allowed: {string.Join(", ", AllowedExtensions)}");
    }
}

public class UpdateMaterialRequestValidator : AbstractValidator<UpdateMaterialRequest>
{
    public UpdateMaterialRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(type => type == "Pdf" || type == "Video" || type == "Text")
            .WithMessage("Type must be Pdf, Video, or Text");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Order index must be non-negative");
    }
}
