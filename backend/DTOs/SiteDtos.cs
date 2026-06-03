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
    DateTime CreatedAt);

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
    IReadOnlyList<CheckResultResponse> History);

public record PublicStatusSiteDto(
    string Url,
    string? Name,
    string Status,
    int? HttpStatusCode,
    int? ResponseTimeMs,
    DateTime? LastCheckedAt);

public record PublicStatusResponse(
    Guid UserId,
    string DisplayName,
    IReadOnlyList<PublicStatusSiteDto> Sites);

public record DashboardStatsResponse(
    int TotalSites,
    int OnlineSites,
    int OfflineSites,
    double? AverageResponseTimeMs);

public static class DtoMappers
{
    public static SiteResponse ToSiteResponse(MonitoredSite site) => new(
        site.Id,
        site.Url,
        site.Name,
        site.LastStatus.ToString().ToUpperInvariant(),
        site.LastHttpStatusCode,
        site.LastResponseTimeMs,
        site.LastCheckedAt,
        site.CreatedAt);

    public static CheckResultResponse ToCheckResultResponse(CheckResult r) => new(
        r.Id,
        r.Status.ToString().ToUpperInvariant(),
        r.HttpStatusCode,
        r.ResponseTimeMs,
        r.ErrorMessage,
        r.CheckedAt);
}
