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

        return sites.Select(DtoMappers.ToSiteResponse).ToList();
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

        return DtoMappers.ToSiteResponse(site);
    }

    public async Task<bool> DeleteSiteAsync(Guid userId, Guid siteId, CancellationToken ct = default)
    {
        var site = await _db.MonitoredSites.FirstOrDefaultAsync(s => s.Id == siteId && s.UserId == userId, ct);
        if (site is null) return false;

        _db.MonitoredSites.Remove(site);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Site {SiteId} deleted", siteId);
        return true;
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

        return new SiteHistoryResponse(
            site.Id,
            site.Url,
            site.Name,
            history.Select(DtoMappers.ToCheckResultResponse).ToList());
    }

    public async Task<PublicStatusResponse?> GetPublicStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.MonitoredSites)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return null;

        var sites = user.MonitoredSites
            .OrderBy(s => s.Name ?? s.Url)
            .Select(s => new PublicStatusSiteDto(
                s.Url,
                s.Name,
                s.LastStatus.ToString().ToUpperInvariant(),
                s.LastHttpStatusCode,
                s.LastResponseTimeMs,
                s.LastCheckedAt))
            .ToList();

        return new PublicStatusResponse(user.Id, user.DisplayName, sites);
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var sites = await _db.MonitoredSites.Where(s => s.UserId == userId).ToListAsync(ct);
        var online = sites.Count(s => s.LastStatus == SiteStatus.Up);
        var offline = sites.Count(s => s.LastStatus == SiteStatus.Down);
        var responseTimes = sites
            .Where(s => s.LastResponseTimeMs.HasValue)
            .Select(s => s.LastResponseTimeMs!.Value)
            .ToList();

        double? avg = responseTimes.Count > 0 ? responseTimes.Average() : null;

        return new DashboardStatsResponse(sites.Count, online, offline, avg);
    }
}
