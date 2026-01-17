using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;

namespace ResearchPlatform.Api.Services;

public class ParticipantCodeService
{
    private readonly ApplicationDbContext _context;

    public ParticipantCodeService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates the next sequential participant code.
    /// Pattern: A1-A99, B1-B99, ..., Z99, AA1-AA99, AB1-AB99, ...
    /// Uses database locking to ensure uniqueness under concurrent access.
    /// </summary>
    public async Task<string> GenerateNextCodeAsync()
    {
        // Use a transaction with row-level locking for thread safety
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get the sequence with locking (FOR UPDATE equivalent in EF Core)
            var sequence = await _context.ParticipantCodeSequences
                .FromSqlRaw("SELECT * FROM participant_code_sequences WHERE id = 1 FOR UPDATE")
                .FirstOrDefaultAsync();

            if (sequence == null)
            {
                // Initialize sequence if not exists
                sequence = new ParticipantCodeSequence
                {
                    Id = 1,
                    CurrentPrefix = "A",
                    CurrentNumber = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ParticipantCodeSequences.Add(sequence);
                await _context.SaveChangesAsync();
            }

            // Increment the number
            sequence.CurrentNumber++;

            // Check if we need to advance the prefix (after 99)
            if (sequence.CurrentNumber > 99)
            {
                sequence.CurrentNumber = 1;
                sequence.CurrentPrefix = IncrementPrefix(sequence.CurrentPrefix);
            }

            sequence.UpdatedAt = DateTime.UtcNow;

            // Generate the code
            var code = $"{sequence.CurrentPrefix}{sequence.CurrentNumber}";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return code;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Increments the prefix following the pattern:
    /// A→B, B→C, ..., Z→AA, AA→AB, ..., AZ→BA, ..., ZZ→AAA
    /// </summary>
    private string IncrementPrefix(string prefix)
    {
        var chars = prefix.ToCharArray();
        var index = chars.Length - 1;

        while (index >= 0)
        {
            if (chars[index] < 'Z')
            {
                chars[index]++;
                return new string(chars);
            }
            else
            {
                chars[index] = 'A';
                index--;
            }
        }

        // All characters were 'Z', need to add a new character
        // e.g., "ZZ" → "AAA"
        return new string('A', chars.Length + 1);
    }

    /// <summary>
    /// Gets the current sequence state (for debugging/admin purposes)
    /// </summary>
    public async Task<(string CurrentPrefix, int CurrentNumber)> GetCurrentStateAsync()
    {
        var sequence = await _context.ParticipantCodeSequences
            .FirstOrDefaultAsync(s => s.Id == 1);

        if (sequence == null)
        {
            return ("A", 0);
        }

        return (sequence.CurrentPrefix, sequence.CurrentNumber);
    }
}
