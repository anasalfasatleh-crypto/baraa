using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Models;
using ResearchPlatform.Api.Models.Enums;
using System.Text.Json;

namespace ResearchPlatform.Api.Data;

public static class SeedData
{
    public static async Task SeedDatabaseAsync(ApplicationDbContext context)
    {
        // Check if database already has data
        if (await context.Users.AnyAsync())
        {
            return; // Database already seeded
        }

        Console.WriteLine("Seeding database with demo data...");

        // Create demo users
        var adminUser = new User
        {
            Email = "admin@research.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Name = "Admin User",
            Role = Role.Admin,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var evaluator1 = new User
        {
            Email = "evaluator1@research.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("eval123"),
            Name = "Dr. Sarah Johnson",
            Role = Role.Evaluator,
            Status = UserStatus.Active,
            Hospital = "City General Hospital",
            Department = "ICU",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var evaluator2 = new User
        {
            Email = "evaluator2@research.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("eval123"),
            Name = "Dr. Michael Chen",
            Role = Role.Evaluator,
            Status = UserStatus.Active,
            Hospital = "University Medical Center",
            Department = "Critical Care",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var student1 = new User
        {
            Email = "student1@nursing.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
            Name = "Emily Rodriguez",
            Role = Role.Student,
            Status = UserStatus.Active,
            Hospital = "City General Hospital",
            Department = "ICU",
            Gender = Gender.Female,
            ExperienceYears = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var student2 = new User
        {
            Email = "student2@nursing.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
            Name = "James Wilson",
            Role = Role.Student,
            Status = UserStatus.Active,
            Hospital = "University Medical Center",
            Department = "CCU",
            Gender = Gender.Male,
            ExperienceYears = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(adminUser, evaluator1, evaluator2, student1, student2);
        await context.SaveChangesAsync();

        // Create demo questionnaire (Pre-test)
        var pretest = new Questionnaire
        {
            Title = "ICU Delirium Knowledge Assessment - Pre-test",
            Description = "Assess baseline knowledge of ICU delirium before training",
            Type = QuestionnaireType.Pretest,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Questionnaires.Add(pretest);
        await context.SaveChangesAsync();

        // Add sample questions
        var questions = new List<Question>
        {
            new Question
            {
                QuestionnaireId = pretest.Id,
                Text = "How familiar are you with ICU delirium?",
                Type = QuestionType.LikertScale,
                OrderIndex = 1,
                Step = 1,
                IsRequired = true,
                MinValue = 1,
                MaxValue = 5,
                MinLabel = "Not familiar at all",
                MaxLabel = "Very familiar",
                CreatedAt = DateTime.UtcNow
            },
            new Question
            {
                QuestionnaireId = pretest.Id,
                Text = "Delirium is a common complication in ICU patients.",
                Type = QuestionType.TrueFalse,
                OrderIndex = 2,
                Step = 1,
                IsRequired = true,
                CreatedAt = DateTime.UtcNow
            },
            new Question
            {
                QuestionnaireId = pretest.Id,
                Text = "Which of the following are risk factors for ICU delirium?",
                Type = QuestionType.MultipleChoice,
                Options = JsonSerializer.Serialize(new[] { "Advanced age", "Sedation", "Sleep deprivation", "All of the above" }),
                OrderIndex = 3,
                Step = 2,
                IsRequired = true,
                CreatedAt = DateTime.UtcNow
            },
            new Question
            {
                QuestionnaireId = pretest.Id,
                Text = "What is the CAM-ICU assessment tool used for?",
                Type = QuestionType.TextField,
                OrderIndex = 4,
                Step = 2,
                IsRequired = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();

        // Create demo post-test questionnaire
        var posttest = new Questionnaire
        {
            Title = "ICU Delirium Knowledge Assessment - Post-test",
            Description = "Assess knowledge after training",
            Type = QuestionnaireType.Posttest,
            IsActive = false, // Not active by default
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Questionnaires.Add(posttest);
        await context.SaveChangesAsync();

        // Create evaluator assignments
        var assignment1 = new EvaluatorAssignment
        {
            EvaluatorId = evaluator1.Id,
            StudentId = student1.Id,
            IsActive = true,
            AssignedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var assignment2 = new EvaluatorAssignment
        {
            EvaluatorId = evaluator2.Id,
            StudentId = student2.Id,
            IsActive = true,
            AssignedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.EvaluatorAssignments.AddRange(assignment1, assignment2);
        await context.SaveChangesAsync();

        // Initialize participant code sequence if not exists
        if (!await context.ParticipantCodeSequences.AnyAsync())
        {
            var codeSequence = new ParticipantCodeSequence
            {
                Id = 1,
                CurrentPrefix = "A",
                CurrentNumber = 0,
                UpdatedAt = DateTime.UtcNow
            };
            context.ParticipantCodeSequences.Add(codeSequence);
            await context.SaveChangesAsync();
            Console.WriteLine("Participant code sequence initialized (starting at A1)");
        }

        Console.WriteLine("Database seeding completed successfully!");
        Console.WriteLine("\nDemo accounts:");
        Console.WriteLine("  Admin: admin@research.edu / admin123");
        Console.WriteLine("  Evaluator 1: evaluator1@research.edu / eval123");
        Console.WriteLine("  Evaluator 2: evaluator2@research.edu / eval123");
        Console.WriteLine("  Student 1: student1@nursing.edu / student123");
        Console.WriteLine("  Student 2: student2@nursing.edu / student123");
    }
}
