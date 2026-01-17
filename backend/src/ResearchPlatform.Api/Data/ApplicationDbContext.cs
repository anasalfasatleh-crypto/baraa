using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Core entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    // Questionnaire entities
    public DbSet<Questionnaire> Questionnaires { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Answer> Answers { get; set; } = null!;
    public DbSet<StepTiming> StepTimings { get; set; } = null!;
    public DbSet<Score> Scores { get; set; } = null!;

    // Material entities
    public DbSet<Material> Materials { get; set; } = null!;
    public DbSet<MaterialAccess> MaterialAccesses { get; set; } = null!;

    // PostTest batch control
    public DbSet<PostTestBatch> PostTestBatches { get; set; } = null!;

    // Evaluator entities
    public DbSet<EvaluatorAssignment> EvaluatorAssignments { get; set; } = null!;
    public DbSet<EvaluatorScore> EvaluatorScores { get; set; } = null!;
    public DbSet<CombinedScore> CombinedScores { get; set; } = null!;

    // Participant entities
    public DbSet<Participant> Participants { get; set; } = null!;
    public DbSet<ParticipantRefreshToken> ParticipantRefreshTokens { get; set; } = null!;
    public DbSet<ParticipantCodeSequence> ParticipantCodeSequences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.Role, e.Status });
            entity.HasIndex(e => e.Hospital);

            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasConversion<string>();
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.Gender).HasConversion<string>();
        });

        // RefreshToken entity configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog entity configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Questionnaire entity configuration
        modelBuilder.Entity<Questionnaire>(entity =>
        {
            entity.ToTable("questionnaires");

            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);

            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
        });

        // Question entity configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("questions");

            entity.HasIndex(e => new { e.QuestionnaireId, e.OrderIndex });
            entity.HasIndex(e => new { e.QuestionnaireId, e.Step });

            entity.Property(e => e.Type).IsRequired().HasConversion<string>();

            entity.HasOne(e => e.Questionnaire)
                .WithMany(q => q.Questions)
                .HasForeignKey(e => e.QuestionnaireId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Answer entity configuration
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable("answers");

            entity.HasIndex(e => new { e.UserId, e.QuestionnaireId });
            entity.HasIndex(e => new { e.UserId, e.QuestionnaireId, e.QuestionId }).IsUnique();
            entity.HasIndex(e => e.IsSubmitted);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Answers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Questionnaire)
                .WithMany(q => q.Answers)
                .HasForeignKey(e => e.QuestionnaireId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StepTiming entity configuration
        modelBuilder.Entity<StepTiming>(entity =>
        {
            entity.ToTable("step_timings");

            entity.HasIndex(e => new { e.UserId, e.QuestionnaireId, e.Step });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Questionnaire)
                .WithMany()
                .HasForeignKey(e => e.QuestionnaireId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Score entity configuration
        modelBuilder.Entity<Score>(entity =>
        {
            entity.ToTable("scores");

            entity.HasIndex(e => new { e.UserId, e.QuestionnaireId });
            entity.HasIndex(e => new { e.UserId, e.QuestionnaireId, e.QuestionId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Questionnaire)
                .WithMany(q => q.Scores)
                .HasForeignKey(e => e.QuestionnaireId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Evaluator)
                .WithMany()
                .HasForeignKey(e => e.EvaluatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Material entity configuration
        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("materials");

            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.OrderIndex);

            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
        });

        // MaterialAccess entity configuration
        modelBuilder.Entity<MaterialAccess>(entity =>
        {
            entity.ToTable("material_accesses");

            entity.HasIndex(e => new { e.UserId, e.MaterialId });
            entity.HasIndex(e => e.AccessedAt);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.MaterialAccesses)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PostTestBatch entity configuration
        modelBuilder.Entity<PostTestBatch>(entity =>
        {
            entity.ToTable("post_test_batches");

            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.OpenDate, e.CloseDate });
        });

        // EvaluatorAssignment entity configuration
        modelBuilder.Entity<EvaluatorAssignment>(entity =>
        {
            entity.ToTable("evaluator_assignments");

            entity.HasIndex(e => new { e.EvaluatorId, e.StudentId });
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(e => e.Evaluator)
                .WithMany()
                .HasForeignKey(e => e.EvaluatorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EvaluatorScore entity configuration
        modelBuilder.Entity<EvaluatorScore>(entity =>
        {
            entity.ToTable("evaluator_scores");

            entity.HasIndex(e => new { e.StudentId, e.QuestionnaireId, e.QuestionId, e.EvaluatorId }).IsUnique();
            entity.HasIndex(e => e.IsFinalized);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Questionnaire)
                .WithMany()
                .HasForeignKey(e => e.QuestionnaireId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Evaluator)
                .WithMany()
                .HasForeignKey(e => e.EvaluatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CombinedScore entity configuration
        modelBuilder.Entity<CombinedScore>(entity =>
        {
            entity.ToTable("combined_scores");

            entity.HasIndex(e => new { e.StudentId, e.QuestionnaireId, e.QuestionId }).IsUnique();
            entity.HasIndex(e => e.IsFinalized);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Questionnaire)
                .WithMany()
                .HasForeignKey(e => e.QuestionnaireId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Participant entity configuration
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.ToTable("participants");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.LoginIdentifier).IsUnique();
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.LoginIdentifier).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
        });

        // ParticipantRefreshToken entity configuration
        modelBuilder.Entity<ParticipantRefreshToken>(entity =>
        {
            entity.ToTable("participant_refresh_tokens");

            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.ParticipantId);
            entity.HasIndex(e => e.ExpiresAt);

            entity.HasOne(e => e.Participant)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ParticipantCodeSequence entity configuration
        modelBuilder.Entity<ParticipantCodeSequence>(entity =>
        {
            entity.ToTable("participant_code_sequences");

            entity.Property(e => e.CurrentPrefix).IsRequired().HasMaxLength(5);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userEntries = ChangeTracker.Entries<User>();
        foreach (var entry in userEntries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        var participantEntries = ChangeTracker.Entries<Participant>();
        foreach (var entry in participantEntries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
