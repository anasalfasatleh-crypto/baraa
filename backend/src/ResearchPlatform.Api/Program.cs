using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ResearchPlatform.Api.Auth;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Middleware;
using ResearchPlatform.Api.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Configure Storage settings
builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add authorization policies
builder.Services.AddAuthorizationPolicies();

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<QuestionnaireService>();
builder.Services.AddScoped<AnswerService>();
builder.Services.AddScoped<ScoreCalculationService>();
builder.Services.AddScoped<StepTimingService>();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddScoped<MaterialService>();
builder.Services.AddScoped<MaterialAccessService>();
builder.Services.AddScoped<PostTestBatchService>();
builder.Services.AddScoped<EvaluatorService>();
builder.Services.AddScoped<EvaluatorScoreService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<CsvImportService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<EvaluatorAssignmentService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<QuestionnaireBuilderService>();

// Add Participant services
builder.Services.AddScoped<ParticipantCodeService>();
builder.Services.AddScoped<ParticipantTokenService>();
builder.Services.AddScoped<ParticipantService>();
builder.Services.AddScoped<ParticipantAuthService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run migrations and seed database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Run migrations automatically
    await context.Database.MigrateAsync();

    // Seed database
    await SeedData.SeedDatabaseAsync(context);
}

// Configure the HTTP request pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Research Platform API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .AllowAnonymous();

app.Run();
