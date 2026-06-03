using System.Diagnostics;
using UptimeLab.Api.Models;

namespace UptimeLab.Api.Services;

/// <summary>
/// Performs HTTP GET checks with a timeout. Treats 2xx/3xx as UP.
/// </summary>
public class WebsiteChecker : IWebsiteChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebsiteChecker> _logger;

    public WebsiteChecker(IHttpClientFactory httpClientFactory, ILogger<WebsiteChecker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<(SiteStatus Status, int? HttpStatusCode, int ResponseTimeMs, string? Error)> CheckAsync(
        string url, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("uptime-checker");
        var sw = Stopwatch.StartNew();

        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            sw.Stop();
            var code = (int)response.StatusCode;
            var isUp = code >= 200 && code < 400;
            return (isUp ? SiteStatus.Up : SiteStatus.Down, code, (int)sw.ElapsedMilliseconds, null);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogDebug(ex, "Check failed for {Url}", url);
            return (SiteStatus.Down, null, (int)sw.ElapsedMilliseconds, ex.Message);
        }
    }
}
