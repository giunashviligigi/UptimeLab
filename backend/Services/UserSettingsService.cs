using Microsoft.EntityFrameworkCore;
using UptimeLab.Api.Data;
using UptimeLab.Api.DTOs;

namespace UptimeLab.Api.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly AppDbContext _db;

    public UserSettingsService(AppDbContext db) => _db = db;

    public async Task<UserSettingsResponse?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user is null ? null : Map(user);
    }

    public async Task<UserSettingsResponse?> UpdateAsync(Guid userId, UpdateUserSettingsRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return null;

        user.WebhookUrl = string.IsNullOrWhiteSpace(request.WebhookUrl) ? null : request.WebhookUrl.Trim();
        user.WebhookAlertsEnabled = request.WebhookAlertsEnabled;
        await _db.SaveChangesAsync(ct);
        return Map(user);
    }

    private static UserSettingsResponse Map(Models.User user) => new(
        user.WebhookUrl,
        user.WebhookAlertsEnabled);
}
