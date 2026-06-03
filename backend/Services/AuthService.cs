using Microsoft.EntityFrameworkCore;
using UptimeLab.Api.Data;
using UptimeLab.Api.DTOs;
using UptimeLab.Api.Models;

namespace UptimeLab.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext db, IJwtTokenService jwt, ILogger<AuthService> logger)
    {
        _db = db;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
        {
            _logger.LogWarning("Registration failed: email already exists");
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("User registered {UserId}", user.Id);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for {Email}", normalizedEmail);
            return null;
        }

        _logger.LogInformation("User logged in {UserId}", user.Id);
        return BuildAuthResponse(user);
    }

    private AuthResponse BuildAuthResponse(User user) =>
        new(_jwt.GenerateToken(user), user.Id, user.Email, user.DisplayName, user.Role);
}
