using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models.Enums;
using System.Text;

namespace ResearchPlatform.Api.Services;

public class ExportService
{
    private readonly ApplicationDbContext _context;

    public ExportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportToExcelAsync()
    {
        var data = await GatherExportDataAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Research Data");

        // Add headers
        AddHeaders(worksheet);

        // Add data rows
        for (int i = 0; i < data.Count; i++)
        {
            AddDataRow(worksheet, i + 2, data[i]);
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Save to memory stream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToCsvAsync()
    {
        var data = await GatherExportDataAsync();

        var csv = new StringBuilder();

        // Add headers
        csv.AppendLine(GetCsvHeaders());

        // Add data rows
        foreach (var row in data)
        {
            csv.AppendLine(GetCsvRow(row));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private async Task<List<ExportDataRow>> GatherExportDataAsync()
    {
        var students = await _context.Users
            .Where(u => u.Role == Role.Student)
            .Include(u => u.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q.Questionnaire)
            .ToListAsync();

        var exportData = new List<ExportDataRow>();

        foreach (var student in students)
        {
            var row = new ExportDataRow
            {
                StudentId = student.Id.ToString(),
                StudentName = student.Name,
                StudentEmail = student.Email,
                Hospital = student.Hospital ?? "",
                Gender = student.Gender?.ToString() ?? "",
                Status = student.Status.ToString(),
                CreatedAt = student.CreatedAt
            };

            // Get pre-test data
            var pretestAnswers = student.Answers
                .Where(a => a.Question.Questionnaire.Type == QuestionnaireType.Pretest)
                .ToList();

            row.PretestSubmittedAt = pretestAnswers.Any()
                ? pretestAnswers.Max(a => a.SubmittedAt)
                : null;

            row.PretestCompleted = pretestAnswers.Any(a => a.SubmittedAt.HasValue);

            // Get post-test data
            var posttestAnswers = student.Answers
                .Where(a => a.Question.Questionnaire.Type == QuestionnaireType.Posttest)
                .ToList();

            row.PosttestSubmittedAt = posttestAnswers.Any()
                ? posttestAnswers.Max(a => a.SubmittedAt)
                : null;

            row.PosttestCompleted = posttestAnswers.Any(a => a.SubmittedAt.HasValue);

            // Get material access data
            var materialAccesses = await _context.MaterialAccesses
                .Where(ma => ma.UserId == student.Id)
                .ToListAsync();
            row.MaterialsAccessedCount = materialAccesses.Count;
            row.FirstMaterialAccessedAt = materialAccesses.Any()
                ? materialAccesses.Min(ma => ma.AccessedAt)
                : null;
            row.LastMaterialAccessedAt = materialAccesses.Any()
                ? materialAccesses.Max(ma => ma.AccessedAt)
                : null;

            // Get timing data
            var stepTimings = await _context.StepTimings
                .Where(st => st.UserId == student.Id)
                .ToListAsync();
            row.TotalTimeSpentSeconds = stepTimings.Sum(st => st.TimeSpentSeconds ?? 0);

            // Get evaluator scores
            var evaluatorScores = await _context.EvaluatorScores
                .Where(es => es.StudentId == student.Id)
                .ToListAsync();

            row.HasEvaluatorScores = evaluatorScores.Any();
            row.EvaluatorScoresFinalized = evaluatorScores.Any() && evaluatorScores.All(es => es.IsFinalized);

            // Get combined scores
            var combinedScores = await _context.CombinedScores
                .Where(cs => cs.StudentId == student.Id)
                .ToListAsync();

            row.HasCombinedScores = combinedScores.Any();
            row.AverageCombinedScore = combinedScores.Any()
                ? combinedScores.Average(cs => cs.AverageScore)
                : null;

            // Get individual question answers for pre-test
            for (int i = 1; i <= 50; i++) // Assuming max 50 questions
            {
                var pretestAnswer = pretestAnswers.FirstOrDefault(a => a.Question.OrderIndex == i);
                if (pretestAnswer != null)
                {
                    row.PretestAnswers[$"Q{i}_PreTest"] = pretestAnswer.Value ?? "";
                }
            }

            // Get individual question answers for post-test
            for (int i = 1; i <= 50; i++)
            {
                var posttestAnswer = posttestAnswers.FirstOrDefault(a => a.Question.OrderIndex == i);
                if (posttestAnswer != null)
                {
                    row.PosttestAnswers[$"Q{i}_PostTest"] = posttestAnswer.Value ?? "";
                }
            }

            // Get individual evaluator scores
            var groupedScores = evaluatorScores
                .GroupBy(es => es.QuestionId)
                .ToList();

            foreach (var group in groupedScores)
            {
                var question = await _context.Questions.FindAsync(group.Key);
                if (question != null)
                {
                    var scores = group.ToList();
                    for (int i = 0; i < scores.Count; i++)
                    {
                        row.EvaluatorScoreDetails[$"Q{question.OrderIndex}_Evaluator{i + 1}"] = scores[i].Score;
                    }
                }
            }

            // Get combined scores by question
            foreach (var combinedScore in combinedScores)
            {
                var question = await _context.Questions.FindAsync(combinedScore.QuestionId);
                if (question != null)
                {
                    row.CombinedScoreDetails[$"Q{question.OrderIndex}_Combined"] = combinedScore.AverageScore;
                    row.CombinedScoreDetails[$"Q{question.OrderIndex}_EvaluatorCount"] = combinedScore.EvaluatorCount;
                }
            }

            exportData.Add(row);
        }

        return exportData;
    }

    private void AddHeaders(IXLWorksheet worksheet)
    {
        int col = 1;

        // Student demographics
        worksheet.Cell(1, col++).Value = "Student_ID";
        worksheet.Cell(1, col++).Value = "Student_Name";
        worksheet.Cell(1, col++).Value = "Student_Email";
        worksheet.Cell(1, col++).Value = "Hospital";
        worksheet.Cell(1, col++).Value = "Gender";
        worksheet.Cell(1, col++).Value = "Status";
        worksheet.Cell(1, col++).Value = "Created_At";

        // Pre-test data
        worksheet.Cell(1, col++).Value = "PreTest_Completed";
        worksheet.Cell(1, col++).Value = "PreTest_Submitted_At";

        // Post-test data
        worksheet.Cell(1, col++).Value = "PostTest_Completed";
        worksheet.Cell(1, col++).Value = "PostTest_Submitted_At";

        // Material access
        worksheet.Cell(1, col++).Value = "Materials_Accessed_Count";
        worksheet.Cell(1, col++).Value = "First_Material_Accessed_At";
        worksheet.Cell(1, col++).Value = "Last_Material_Accessed_At";

        // Timing
        worksheet.Cell(1, col++).Value = "Total_Time_Spent_Seconds";

        // Scoring
        worksheet.Cell(1, col++).Value = "Has_Evaluator_Scores";
        worksheet.Cell(1, col++).Value = "Evaluator_Scores_Finalized";
        worksheet.Cell(1, col++).Value = "Has_Combined_Scores";
        worksheet.Cell(1, col++).Value = "Average_Combined_Score";

        // Pre-test answers (Q1-Q50)
        for (int i = 1; i <= 50; i++)
        {
            worksheet.Cell(1, col++).Value = $"Q{i}_PreTest";
        }

        // Post-test answers (Q1-Q50)
        for (int i = 1; i <= 50; i++)
        {
            worksheet.Cell(1, col++).Value = $"Q{i}_PostTest";
        }

        // Evaluator scores (Q1-Q50, up to 3 evaluators)
        for (int i = 1; i <= 50; i++)
        {
            for (int e = 1; e <= 3; e++)
            {
                worksheet.Cell(1, col++).Value = $"Q{i}_Evaluator{e}";
            }
        }

        // Combined scores (Q1-Q50)
        for (int i = 1; i <= 50; i++)
        {
            worksheet.Cell(1, col++).Value = $"Q{i}_Combined";
            worksheet.Cell(1, col++).Value = $"Q{i}_EvaluatorCount";
        }

        // Format header row
        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
    }

    private void AddDataRow(IXLWorksheet worksheet, int row, ExportDataRow data)
    {
        int col = 1;

        // Student demographics
        worksheet.Cell(row, col++).Value = data.StudentId;
        worksheet.Cell(row, col++).Value = data.StudentName;
        worksheet.Cell(row, col++).Value = data.StudentEmail;
        worksheet.Cell(row, col++).Value = data.Hospital;
        worksheet.Cell(row, col++).Value = data.Gender;
        worksheet.Cell(row, col++).Value = data.Status;
        worksheet.Cell(row, col++).Value = data.CreatedAt;

        // Pre-test data
        worksheet.Cell(row, col++).Value = data.PretestCompleted;
        worksheet.Cell(row, col++).Value = data.PretestSubmittedAt;

        // Post-test data
        worksheet.Cell(row, col++).Value = data.PosttestCompleted;
        worksheet.Cell(row, col++).Value = data.PosttestSubmittedAt;

        // Material access
        worksheet.Cell(row, col++).Value = data.MaterialsAccessedCount;
        worksheet.Cell(row, col++).Value = data.FirstMaterialAccessedAt;
        worksheet.Cell(row, col++).Value = data.LastMaterialAccessedAt;

        // Timing
        worksheet.Cell(row, col++).Value = data.TotalTimeSpentSeconds;

        // Scoring
        worksheet.Cell(row, col++).Value = data.HasEvaluatorScores;
        worksheet.Cell(row, col++).Value = data.EvaluatorScoresFinalized;
        worksheet.Cell(row, col++).Value = data.HasCombinedScores;
        worksheet.Cell(row, col++).Value = data.AverageCombinedScore;

        // Pre-test answers
        for (int i = 1; i <= 50; i++)
        {
            var key = $"Q{i}_PreTest";
            worksheet.Cell(row, col++).Value = data.PretestAnswers.GetValueOrDefault(key, "");
        }

        // Post-test answers
        for (int i = 1; i <= 50; i++)
        {
            var key = $"Q{i}_PostTest";
            worksheet.Cell(row, col++).Value = data.PosttestAnswers.GetValueOrDefault(key, "");
        }

        // Evaluator scores
        for (int i = 1; i <= 50; i++)
        {
            for (int e = 1; e <= 3; e++)
            {
                var key = $"Q{i}_Evaluator{e}";
                worksheet.Cell(row, col++).Value = data.EvaluatorScoreDetails.GetValueOrDefault(key, null);
            }
        }

        // Combined scores
        for (int i = 1; i <= 50; i++)
        {
            var scoreKey = $"Q{i}_Combined";
            var countKey = $"Q{i}_EvaluatorCount";
            var score = data.CombinedScoreDetails.GetValueOrDefault(scoreKey, null);
            var count = data.CombinedScoreDetails.GetValueOrDefault(countKey, null);

            if (score is decimal scoreDecimal)
                worksheet.Cell(row, col++).Value = scoreDecimal;
            else
                worksheet.Cell(row, col++).Value = "";

            if (count is int countInt)
                worksheet.Cell(row, col++).Value = countInt;
            else
                worksheet.Cell(row, col++).Value = "";
        }
    }

    private string GetCsvHeaders()
    {
        var headers = new List<string>
        {
            "Student_ID", "Student_Name", "Student_Email", "Hospital", "Gender", "Status", "Created_At",
            "PreTest_Completed", "PreTest_Submitted_At",
            "PostTest_Completed", "PostTest_Submitted_At",
            "Materials_Accessed_Count", "First_Material_Accessed_At", "Last_Material_Accessed_At",
            "Total_Time_Spent_Seconds",
            "Has_Evaluator_Scores", "Evaluator_Scores_Finalized", "Has_Combined_Scores", "Average_Combined_Score"
        };

        // Add pre-test answer columns
        for (int i = 1; i <= 50; i++)
        {
            headers.Add($"Q{i}_PreTest");
        }

        // Add post-test answer columns
        for (int i = 1; i <= 50; i++)
        {
            headers.Add($"Q{i}_PostTest");
        }

        // Add evaluator score columns
        for (int i = 1; i <= 50; i++)
        {
            for (int e = 1; e <= 3; e++)
            {
                headers.Add($"Q{i}_Evaluator{e}");
            }
        }

        // Add combined score columns
        for (int i = 1; i <= 50; i++)
        {
            headers.Add($"Q{i}_Combined");
            headers.Add($"Q{i}_EvaluatorCount");
        }

        return string.Join(",", headers);
    }

    private string GetCsvRow(ExportDataRow data)
    {
        var values = new List<string>
        {
            EscapeCsv(data.StudentId),
            EscapeCsv(data.StudentName),
            EscapeCsv(data.StudentEmail),
            EscapeCsv(data.Hospital),
            EscapeCsv(data.Gender),
            EscapeCsv(data.Status),
            EscapeCsv(data.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
            data.PretestCompleted.ToString(),
            EscapeCsv(data.PretestSubmittedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
            data.PosttestCompleted.ToString(),
            EscapeCsv(data.PosttestSubmittedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
            data.MaterialsAccessedCount.ToString(),
            EscapeCsv(data.FirstMaterialAccessedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
            EscapeCsv(data.LastMaterialAccessedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
            data.TotalTimeSpentSeconds.ToString(),
            data.HasEvaluatorScores.ToString(),
            data.EvaluatorScoresFinalized.ToString(),
            data.HasCombinedScores.ToString(),
            data.AverageCombinedScore?.ToString("F2") ?? ""
        };

        // Add pre-test answers
        for (int i = 1; i <= 50; i++)
        {
            var key = $"Q{i}_PreTest";
            values.Add(EscapeCsv(data.PretestAnswers.GetValueOrDefault(key, "")));
        }

        // Add post-test answers
        for (int i = 1; i <= 50; i++)
        {
            var key = $"Q{i}_PostTest";
            values.Add(EscapeCsv(data.PosttestAnswers.GetValueOrDefault(key, "")));
        }

        // Add evaluator scores
        for (int i = 1; i <= 50; i++)
        {
            for (int e = 1; e <= 3; e++)
            {
                var key = $"Q{i}_Evaluator{e}";
                var score = data.EvaluatorScoreDetails.GetValueOrDefault(key, null);
                values.Add(score?.ToString("F2") ?? "");
            }
        }

        // Add combined scores
        for (int i = 1; i <= 50; i++)
        {
            var scoreKey = $"Q{i}_Combined";
            var countKey = $"Q{i}_EvaluatorCount";
            var score = data.CombinedScoreDetails.GetValueOrDefault(scoreKey, null);
            var count = data.CombinedScoreDetails.GetValueOrDefault(countKey, null);
            values.Add(score is decimal d ? d.ToString("F2") : "");
            values.Add(count is int c ? c.ToString() : "");
        }

        return string.Join(",", values);
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // If value contains comma, quote, or newline, wrap in quotes and escape internal quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private class ExportDataRow
    {
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string Hospital { get; set; } = "";
        public string Gender { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public bool PretestCompleted { get; set; }
        public DateTime? PretestSubmittedAt { get; set; }

        public bool PosttestCompleted { get; set; }
        public DateTime? PosttestSubmittedAt { get; set; }

        public int MaterialsAccessedCount { get; set; }
        public DateTime? FirstMaterialAccessedAt { get; set; }
        public DateTime? LastMaterialAccessedAt { get; set; }

        public int TotalTimeSpentSeconds { get; set; }

        public bool HasEvaluatorScores { get; set; }
        public bool EvaluatorScoresFinalized { get; set; }
        public bool HasCombinedScores { get; set; }
        public decimal? AverageCombinedScore { get; set; }

        public Dictionary<string, string> PretestAnswers { get; set; } = new();
        public Dictionary<string, string> PosttestAnswers { get; set; } = new();
        public Dictionary<string, decimal?> EvaluatorScoreDetails { get; set; } = new();
        public Dictionary<string, object?> CombinedScoreDetails { get; set; } = new();
    }
}
