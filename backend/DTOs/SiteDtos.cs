using System.ComponentModel.DataAnnotations;
using UptimeLab.Api.Models;

namespace UptimeLab.Api.DTOs;

public record CreateSiteRequest(
    [Required][Url] string Url,
    string? Name);

public record SiteResponse(
    Guid Id,
    string Url,
    string? Name,
    string Status,
    int? HttpStatusCode,
    int? ResponseTimeMs,
    DateTime? LastCheckedAt,
    DateTime CreatedAt,
    bool IsPaused,
    string? LastErrorMessage,
    double? UptimePercent24h);

public record CheckResultResponse(
    Guid Id,
    string Status,
    int? HttpStatusCode,
    int ResponseTimeMs,
    string? ErrorMessage,
    DateTime CheckedAt);

public record SiteHistoryResponse(
    Guid SiteId,
    string Url,
    string? Name,
    bool IsPaused,
    double? UptimePercent24h,
    double? UptimePercent7d,
    IReadOnlyList<CheckResultResponse> History);

public record PublicStatusSiteDto(
    string Url,
    string? Name,
    string Status,
    int? HttpStatusCode,
    int? ResponseTimeMs,
    DateTime? LastCheckedAt,
    bool IsPaused);

public record PublicStatusResponse(
    Guid UserId,
    string DisplayName,
    IReadOnlyList<PublicStatusSiteDto> Sites);

public record DashboardStatsResponse(
    int TotalSites,
    int OnlineSites,
    int OfflineSites,
    int PausedSites,
    double? AverageResponseTimeMs,
    double? OverallUptimePercent24h);

public static class DtoMappers
{
    public static SiteResponse ToSiteResponse(MonitoredSite site, double? uptime24h = null) => new(
        site.Id,
        site.Url,
        site.Name,
        site.IsPaused ? "PAUSED" : site.LastStatus.ToString().ToUpperInvariant(),
        site.LastHttpStatusCode,
        site.LastResponseTimeMs,
        site.LastCheckedAt,
        site.CreatedAt,
        site.IsPaused,
        site.LastErrorMessage,
        uptime24h);

    public static CheckResultResponse ToCheckResultResponse(CheckResult r) => new(
        r.Id,
        r.Status.ToString().ToUpperInvariant(),
        r.HttpStatusCode,
        r.ResponseTimeMs,
        r.ErrorMessage,
        r.CheckedAt);
}
