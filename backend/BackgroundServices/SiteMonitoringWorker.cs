using Microsoft.EntityFrameworkCore;
using UptimeLab.Api.Data;
using UptimeLab.Api.Models;
using UptimeLab.Api.Services;

namespace UptimeLab.Api.BackgroundServices;

/// <summary>
/// Runs every 60 seconds and checks all monitored websites.
/// </summary>
public class SiteMonitoringWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SiteMonitoringWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    public SiteMonitoringWorker(IServiceScopeFactory scopeFactory, ILogger<SiteMonitoringWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Site monitoring worker started (interval: {Seconds}s)", Interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunChecksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during monitoring cycle");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunChecksAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var checker = scope.ServiceProvider.GetRequiredService<IWebsiteChecker>();
        var alerts = scope.ServiceProvider.GetRequiredService<IAlertService>();

        var sites = await db.MonitoredSites
            .Include(s => s.User)
            .Where(s => !s.IsPaused)
            .ToListAsync(ct);

        if (sites.Count == 0) return;

        _logger.LogInformation("Checking {Count} site(s)", sites.Count);

        foreach (var site in sites)
        {
            var previousStatus = site.LastStatus;
            var (status, httpCode, responseMs, error) = await checker.CheckAsync(site.Url, ct);

            var result = new CheckResult
            {
                Id = Guid.NewGuid(),
                MonitoredSiteId = site.Id,
                Status = status,
                HttpStatusCode = httpCode,
                ResponseTimeMs = responseMs,
                ErrorMessage = error,
                CheckedAt = DateTime.UtcNow
            };

            site.LastStatus = status;
            site.LastHttpStatusCode = httpCode;
            site.LastResponseTimeMs = responseMs;
            site.LastCheckedAt = result.CheckedAt;
            site.LastErrorMessage = error;

            db.CheckResults.Add(result);

            if (previousStatus != status && status != SiteStatus.Unknown)
            {
                await alerts.HandleStatusChangeAsync(
                    site, site.User, previousStatus, status, httpCode, responseMs, error, ct);
            }
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Monitoring cycle completed");
    }
}
