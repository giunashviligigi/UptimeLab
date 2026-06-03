using UptimeLab.Api.Models;

namespace UptimeLab.Api.Services;

public interface IAlertService
{
    Task HandleStatusChangeAsync(
        MonitoredSite site,
        User user,
        SiteStatus previousStatus,
        SiteStatus newStatus,
        int? httpCode,
        int responseMs,
        string? error,
        CancellationToken ct = default);
}
