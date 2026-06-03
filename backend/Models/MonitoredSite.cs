namespace UptimeLab.Api.Models;

/// <summary>
/// A URL owned by a user that the background worker checks every 60 seconds.
/// </summary>
public class MonitoredSite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Latest snapshot for quick dashboard reads.</summary>
    public SiteStatus LastStatus { get; set; } = SiteStatus.Unknown;
    public int? LastHttpStatusCode { get; set; }
    public int? LastResponseTimeMs { get; set; }
    public DateTime? LastCheckedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<CheckResult> CheckResults { get; set; } = new List<CheckResult>();
}

public enum SiteStatus
{
    Unknown = 0,
    Up = 1,
    Down = 2
}
