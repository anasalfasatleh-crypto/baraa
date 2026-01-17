using Microsoft.EntityFrameworkCore;
using ResearchPlatform.Api.Data;
using ResearchPlatform.Api.Models;
using ResearchPlatform.Api.Models.Enums;
using BCrypt.Net;

namespace ResearchPlatform.Api.Services;

public class UserManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        ApplicationDbContext context,
        ILogger<UserManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<User>> GetAllUsersAsync(Role? role = null, UserStatus? status = null)
    {
        var query = _context.Users.AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        return await query
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User> CreateUserAsync(
        string email,
        string password,
        string name,
        Role role,
        string? hospital = null,
        Gender? gender = null)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = name,
            Role = role,
            Status = UserStatus.Active,
            Hospital = hospital,
            Gender = gender
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created user: {user.Id} - {user.Email} ({user.Role})");

        return user;
    }

    public async Task<User> UpdateUserAsync(
        Guid userId,
        string? name = null,
        string? hospital = null,
        Gender? gender = null,
        Role? role = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (name != null) user.Name = name;
        if (hospital != null) user.Hospital = hospital;
        if (gender.HasValue) user.Gender = gender.Value;
        if (role.HasValue) user.Role = role.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Updated user: {user.Id} - {user.Email}");

        return user;
    }

    public async Task ActivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Activated user: {user.Id} - {user.Email}");
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Deactivated user: {user.Id} - {user.Email}");
    }

    public async Task<string> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Reset password for user: {user.Id} - {user.Email}");

        return newPassword;
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Deleted user: {userId}");
    }

    public async Task<int> GetUserCountByRoleAsync(Role role)
    {
        return await _context.Users
            .Where(u => u.Role == role && u.Status == UserStatus.Active)
            .CountAsync();
    }
}
