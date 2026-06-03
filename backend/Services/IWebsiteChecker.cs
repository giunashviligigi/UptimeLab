using UptimeLab.Api.Models;

namespace UptimeLab.Api.Services;

public interface IWebsiteChecker
{
    Task<(SiteStatus Status, int? HttpStatusCode, int ResponseTimeMs, string? Error)> CheckAsync(string url, CancellationToken ct = default);
}
