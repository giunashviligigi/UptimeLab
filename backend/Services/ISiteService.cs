using UptimeLab.Api.DTOs;

namespace UptimeLab.Api.Services;

public interface ISiteService
{
    Task<IReadOnlyList<SiteResponse>> GetSitesAsync(Guid userId, CancellationToken ct = default);
    Task<SiteResponse?> CreateSiteAsync(Guid userId, CreateSiteRequest request, CancellationToken ct = default);
    Task<bool> DeleteSiteAsync(Guid userId, Guid siteId, CancellationToken ct = default);
    Task<SiteResponse?> SetPauseAsync(Guid userId, Guid siteId, bool isPaused, CancellationToken ct = default);
    Task<SiteHistoryResponse?> GetHistoryAsync(Guid userId, Guid siteId, int limit, CancellationToken ct = default);
    Task<SiteUptimeResponse?> GetUptimeAsync(Guid userId, Guid siteId, int hours, CancellationToken ct = default);
    Task<PublicStatusResponse?> GetPublicStatusAsync(Guid userId, CancellationToken ct = default);
    Task<DashboardStatsResponse> GetDashboardStatsAsync(Guid userId, CancellationToken ct = default);
}
