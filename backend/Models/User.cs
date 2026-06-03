namespace UptimeLab.Api.Models;

/// <summary>
/// Application user. Role supports future admin features.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>User or Admin — extensible for admin panel.</summary>
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>HTTPS webhook (Slack-compatible) for DOWN/UP alerts.</summary>
    public string? WebhookUrl { get; set; }
    public bool WebhookAlertsEnabled { get; set; }

    public ICollection<MonitoredSite> MonitoredSites { get; set; } = new List<MonitoredSite>();
}
