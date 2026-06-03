using System.Net.Http.Json;
using UptimeLab.Api.Models;

namespace UptimeLab.Api.Services;

/// <summary>
/// Sends webhook notifications when a site transitions UP/DOWN (Slack-compatible JSON).
/// </summary>
public class AlertService : IAlertService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AlertService> _logger;

    public AlertService(IHttpClientFactory httpClientFactory, ILogger<AlertService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task HandleStatusChangeAsync(
        MonitoredSite site,
        User user,
        SiteStatus previousStatus,
        SiteStatus newStatus,
        int? httpCode,
        int responseMs,
        string? error,
        CancellationToken ct = default)
    {
        if (!user.WebhookAlertsEnabled || string.IsNullOrWhiteSpace(user.WebhookUrl))
            return;

        if (newStatus == SiteStatus.Unknown)
            return;

        var shouldNotifyDown = newStatus == SiteStatus.Down && site.LastNotifiedStatus != SiteStatus.Down;
        var shouldNotifyUp = newStatus == SiteStatus.Up &&
                             (site.LastNotifiedStatus == SiteStatus.Down || previousStatus == SiteStatus.Down);

        if (!shouldNotifyDown && !shouldNotifyUp)
            return;

        var eventType = shouldNotifyDown ? "down" : "up";
        var payload = new
        {
            text = $"UptimeLab: {(site.Name ?? site.Url)} is {newStatus.ToString().ToUpperInvariant()}",
            site = new { id = site.Id, url = site.Url, name = site.Name },
            @event = eventType,
            status = newStatus.ToString().ToUpperInvariant(),
            previousStatus = previousStatus.ToString().ToUpperInvariant(),
            httpStatusCode = httpCode,
            responseTimeMs = responseMs,
            error,
            checkedAt = DateTime.UtcNow
        };

        try
        {
            var client = _httpClientFactory.CreateClient("webhook");
            var response = await client.PostAsJsonAsync(user.WebhookUrl.Trim(), payload, ct);
            if (response.IsSuccessStatusCode)
            {
                site.LastNotifiedStatus = newStatus;
                _logger.LogInformation("Webhook {Event} sent for site {SiteId}", eventType, site.Id);
            }
            else
            {
                _logger.LogWarning("Webhook failed {StatusCode} for site {SiteId}", response.StatusCode, site.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook error for site {SiteId}", site.Id);
        }
    }
}
