using FluentValidation;
using ResearchPlatform.Api.DTOs;
using System.Text.RegularExpressions;

namespace ResearchPlatform.Api.Validators;

public class ParticipantRegisterRequestValidator : AbstractValidator<ParticipantRegisterRequest>
{
    public ParticipantRegisterRequestValidator()
    {
        RuleFor(x => x.LoginIdentifier)
            .NotEmpty().WithMessage("Login identifier is required")
            .MaximumLength(255).WithMessage("Login identifier must not exceed 255 characters")
            .Must(BeValidLoginIdentifier).WithMessage("Login identifier must be a valid email or username (3-50 alphanumeric characters, underscores allowed)");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Matches(@"^\+?[\d\s\-()]+$").WithMessage("Phone number format is invalid")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private bool BeValidLoginIdentifier(string loginIdentifier)
    {
        if (string.IsNullOrWhiteSpace(loginIdentifier))
            return false;

        // Check if it's an email
        if (loginIdentifier.Contains('@'))
        {
            // Basic email validation
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(loginIdentifier);
        }
        else
        {
            // Username validation: 3-50 alphanumeric characters, underscores allowed
            var usernameRegex = new Regex(@"^[a-zA-Z0-9_]{3,50}$");
            return usernameRegex.IsMatch(loginIdentifier);
        }
    }
}

public class ParticipantLoginRequestValidator : AbstractValidator<ParticipantLoginRequest>
{
    public ParticipantLoginRequestValidator()
    {
        RuleFor(x => x.LoginIdentifier)
            .NotEmpty().WithMessage("Login identifier is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

// Note: ChangePasswordRequestValidator is defined in AuthValidators.cs and shared

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
