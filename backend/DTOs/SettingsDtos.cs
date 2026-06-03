namespace UptimeLab.Api.DTOs;

public record UserSettingsResponse(string? WebhookUrl, bool WebhookAlertsEnabled);

public record UpdateUserSettingsRequest(string? WebhookUrl, bool WebhookAlertsEnabled);

public record SetPauseRequest(bool IsPaused);

public record SiteUptimeResponse(
    Guid SiteId,
    double? UptimePercent,
    int TotalChecks,
    int UpChecks,
    int Hours);
