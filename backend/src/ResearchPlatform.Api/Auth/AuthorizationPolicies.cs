using Microsoft.AspNetCore.Authorization;

namespace ResearchPlatform.Api.Auth;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string EvaluatorOnly = "EvaluatorOnly";
    public const string StudentOnly = "StudentOnly";
    public const string AdminOrEvaluator = "AdminOrEvaluator";
    public const string ParticipantOnly = "ParticipantOnly";

    public static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminOnly, policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy(EvaluatorOnly, policy =>
                policy.RequireRole("Evaluator"));

            options.AddPolicy(StudentOnly, policy =>
                policy.RequireRole("Student"));

            options.AddPolicy(AdminOrEvaluator, policy =>
                policy.RequireRole("Admin", "Evaluator"));

            options.AddPolicy(ParticipantOnly, policy =>
                policy.RequireRole("Participant"));
        });
    }
}
