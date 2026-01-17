using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ResearchPlatform.Api.Models.Enums;

namespace ResearchPlatform.Api.Services;

public class CsvImportService
{
    private readonly UserManagementService _userManagementService;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(
        UserManagementService userManagementService,
        ILogger<CsvImportService> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<CsvImportResult> ImportUsersFromCsvAsync(Stream csvStream)
    {
        var result = new CsvImportResult();

        using var reader = new StreamReader(csvStream);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var csv = new CsvReader(reader, config);

        try
        {
            var records = csv.GetRecords<CsvUserRecord>().ToList();

            foreach (var record in records)
            {
                try
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(record.Email) ||
                        string.IsNullOrWhiteSpace(record.Name) ||
                        string.IsNullOrWhiteSpace(record.Role))
                    {
                        result.Errors.Add($"Row {result.TotalRows + 1}: Missing required fields (Email, Name, Role)");
                        result.TotalRows++;
                        continue;
                    }

                    // Parse role
                    if (!Enum.TryParse<Role>(record.Role, true, out var role))
                    {
                        result.Errors.Add($"Row {result.TotalRows + 1}: Invalid role '{record.Role}'");
                        result.TotalRows++;
                        continue;
                    }

                    // Parse gender if provided
                    Gender? gender = null;
                    if (!string.IsNullOrWhiteSpace(record.Gender))
                    {
                        if (Enum.TryParse<Gender>(record.Gender, true, out var parsedGender))
                        {
                            gender = parsedGender;
                        }
                    }

                    // Generate password if not provided
                    var password = string.IsNullOrWhiteSpace(record.Password)
                        ? GenerateRandomPassword()
                        : record.Password;

                    // Create user
                    await _userManagementService.CreateUserAsync(
                        record.Email,
                        password,
                        record.Name,
                        role,
                        record.Hospital,
                        gender);

                    result.SuccessCount++;
                    result.GeneratedPasswords.Add(new UserPasswordInfo
                    {
                        Email = record.Email,
                        Name = record.Name,
                        Password = password
                    });

                    _logger.LogInformation($"Imported user: {record.Email}");
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {result.TotalRows + 1}: {ex.Message}");
                    _logger.LogWarning($"Failed to import user at row {result.TotalRows + 1}: {ex.Message}");
                }

                result.TotalRows++;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"CSV parsing error: {ex.Message}");
            _logger.LogError(ex, "CSV parsing failed");
        }

        _logger.LogInformation($"CSV import completed: {result.SuccessCount} succeeded, {result.Errors.Count} failed");

        return result;
    }

    private static string GenerateRandomPassword(int length = 12)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var random = new Random();
        var password = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            password.Append(chars[random.Next(chars.Length)]);
        }

        return password.ToString();
    }
}

public class CsvUserRecord
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? Hospital { get; set; }
    public string? Gender { get; set; }
}

public class CsvImportResult
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<UserPasswordInfo> GeneratedPasswords { get; set; } = new();
}

public class UserPasswordInfo
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
