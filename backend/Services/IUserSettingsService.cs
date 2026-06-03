using UptimeLab.Api.DTOs;

namespace UptimeLab.Api.Services;

public interface IUserSettingsService
{
    Task<UserSettingsResponse?> GetAsync(Guid userId, CancellationToken ct = default);
    Task<UserSettingsResponse?> UpdateAsync(Guid userId, UpdateUserSettingsRequest request, CancellationToken ct = default);
}
