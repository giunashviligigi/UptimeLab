namespace UptimeLab.Api.Models;

/// <summary>
/// One check run stored for history charts and public status pages.
/// </summary>
public class CheckResult
{
    public Guid Id { get; set; }
    public Guid MonitoredSiteId { get; set; }
    public SiteStatus Status { get; set; }
    public int? HttpStatusCode { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public MonitoredSite MonitoredSite { get; set; } = null!;
}
