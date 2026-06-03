using Microsoft.EntityFrameworkCore;
using UptimeLab.Api.Data;
using UptimeLab.Api.DTOs;
using UptimeLab.Api.Models;

namespace UptimeLab.Api.Services;

public class SiteService : ISiteService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SiteService> _logger;

    public SiteService(AppDbContext db, ILogger<SiteService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SiteResponse>> GetSitesAsync(Guid userId, CancellationToken ct = default)
    {
        var sites = await _db.MonitoredSites
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        var uptimeMap = await GetUptimeMapAsync(sites.Select(s => s.Id).ToList(), 24, ct);

        return sites
            .Select(s => DtoMappers.ToSiteResponse(s, uptimeMap.GetValueOrDefault(s.Id)))
            .ToList();
    }

    public async Task<SiteResponse?> CreateSiteAsync(Guid userId, CreateSiteRequest request, CancellationToken ct = default)
    {
        var site = new MonitoredSite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Url = request.Url.Trim(),
            Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(),
            LastStatus = SiteStatus.Unknown
        };

        _db.MonitoredSites.Add(site);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Site {SiteId} created for user {UserId}", site.Id, userId);

        return DtoMappers.ToSiteResponse(site, null);
    }

    public async Task<bool> DeleteSiteAsync(Guid userId, Guid siteId, CancellationToken ct = default)
    {
        var site = await _db.MonitoredSites.FirstOrDefaultAsync(s => s.Id == siteId && s.UserId == userId, ct);
        if (site is null) return false;

        _db.MonitoredSites.Remove(site);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<SiteResponse?> SetPauseAsync(Guid userId, Guid siteId, bool isPaused, CancellationToken ct = default)
    {
        var site = await _db.MonitoredSites.FirstOrDefaultAsync(s => s.Id == siteId && s.UserId == userId, ct);
        if (site is null) return null;

        site.IsPaused = isPaused;
        await _db.SaveChangesAsync(ct);
        var uptime = await ComputeUptimeAsync(siteId, 24, ct);
        return DtoMappers.ToSiteResponse(site, uptime);
    }

    public async Task<SiteHistoryResponse?> GetHistoryAsync(Guid userId, Guid siteId, int limit, CancellationToken ct = default)
    {
        var site = await _db.MonitoredSites.FirstOrDefaultAsync(s => s.Id == siteId && s.UserId == userId, ct);
        if (site is null) return null;

        var history = await _db.CheckResults
            .Where(c => c.MonitoredSiteId == siteId)
            .OrderByDescending(c => c.CheckedAt)
            .Take(limit)
            .ToListAsync(ct);

        var uptime24 = await ComputeUptimeAsync(siteId, 24, ct);
        var uptime7d = await ComputeUptimeAsync(siteId, 24 * 7, ct);

        return new SiteHistoryResponse(
            site.Id,
            site.Url,
            site.Name,
            site.IsPaused,
            uptime24,
            uptime7d,
            history.Select(DtoMappers.ToCheckResultResponse).ToList());
    }

    public async Task<SiteUptimeResponse?> GetUptimeAsync(Guid userId, Guid siteId, int hours, CancellationToken ct = default)
    {
        var exists = await _db.MonitoredSites.AnyAsync(s => s.Id == siteId && s.UserId == userId, ct);
        if (!exists) return null;

        hours = Math.Clamp(hours, 1, 24 * 30);
        var since = DateTime.UtcNow.AddHours(-hours);
        var checks = await _db.CheckResults
            .Where(c => c.MonitoredSiteId == siteId && c.CheckedAt >= since)
            .ToListAsync(ct);

        if (checks.Count == 0)
            return new SiteUptimeResponse(siteId, null, 0, 0, hours);

        var up = checks.Count(c => c.Status == SiteStatus.Up);
        var percent = Math.Round(100.0 * up / checks.Count, 1);
        return new SiteUptimeResponse(siteId, percent, checks.Count, up, hours);
    }

    public async Task<PublicStatusResponse?> GetPublicStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.MonitoredSites)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return null;

        var sites = user.MonitoredSites
            .Where(s => !s.IsPaused)
            .OrderBy(s => s.Name ?? s.Url)
            .Select(s => new PublicStatusSiteDto(
                s.Url,
                s.Name,
                s.LastStatus.ToString().ToUpperInvariant(),
                s.LastHttpStatusCode,
                s.LastResponseTimeMs,
                s.LastCheckedAt,
                s.IsPaused))
            .ToList();

        return new PublicStatusResponse(user.Id, user.DisplayName, sites);
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var sites = await _db.MonitoredSites.Where(s => s.UserId == userId).ToListAsync(ct);
        var active = sites.Where(s => !s.IsPaused).ToList();
        var online = active.Count(s => s.LastStatus == SiteStatus.Up);
        var offline = active.Count(s => s.LastStatus == SiteStatus.Down);
        var paused = sites.Count(s => s.IsPaused);

        var responseTimes = active
            .Where(s => s.LastResponseTimeMs.HasValue)
            .Select(s => s.LastResponseTimeMs!.Value)
            .ToList();
        double? avg = responseTimes.Count > 0 ? responseTimes.Average() : null;

        double? overallUptime = null;
        if (sites.Count > 0)
        {
            var map = await GetUptimeMapAsync(sites.Select(s => s.Id).ToList(), 24, ct);
            var values = map.Values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (values.Count > 0)
                overallUptime = Math.Round(values.Average(), 1);
        }

        return new DashboardStatsResponse(sites.Count, online, offline, paused, avg, overallUptime);
    }

    private async Task<Dictionary<Guid, double?>> GetUptimeMapAsync(List<Guid> siteIds, int hours, CancellationToken ct)
    {
        if (siteIds.Count == 0) return new Dictionary<Guid, double?>();

        var since = DateTime.UtcNow.AddHours(-hours);
        var grouped = await _db.CheckResults
            .Where(c => siteIds.Contains(c.MonitoredSiteId) && c.CheckedAt >= since)
            .GroupBy(c => c.MonitoredSiteId)
            .Select(g => new
            {
                SiteId = g.Key,
                Total = g.Count(),
                Up = g.Count(c => c.Status == SiteStatus.Up)
            })
            .ToListAsync(ct);

        return grouped.ToDictionary(
            x => x.SiteId,
            x => x.Total == 0 ? (double?)null : Math.Round(100.0 * x.Up / x.Total, 1));
    }

    private async Task<double?> ComputeUptimeAsync(Guid siteId, int hours, CancellationToken ct)
    {
        var map = await GetUptimeMapAsync(new List<Guid> { siteId }, hours, ct);
        return map.GetValueOrDefault(siteId);
    }
}
