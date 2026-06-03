using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UptimeLab.Api.DTOs;
using UptimeLab.Api.Services;

namespace UptimeLab.Api.Controllers;

[ApiController]
[Route("api/sites")]
[Authorize]
public class SitesController : ControllerBase
{
    private readonly ISiteService _sites;

    public SitesController(ISiteService sites) => _sites = sites;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SiteResponse>>> GetSites(CancellationToken ct)
    {
        var list = await _sites.GetSitesAsync(GetUserId(), ct);
        return Ok(list);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsResponse>> GetStats(CancellationToken ct)
    {
        var stats = await _sites.GetDashboardStatsAsync(GetUserId(), ct);
        return Ok(stats);
    }

    [HttpPost]
    public async Task<ActionResult<SiteResponse>> CreateSite([FromBody] CreateSiteRequest request, CancellationToken ct)
    {
        var site = await _sites.CreateSiteAsync(GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetHistory), new { id = site!.Id }, site);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSite(Guid id, CancellationToken ct)
    {
        var deleted = await _sites.DeleteSiteAsync(GetUserId(), id, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<SiteHistoryResponse>> GetHistory(Guid id, [FromQuery] int limit = 50, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        var history = await _sites.GetHistoryAsync(GetUserId(), id, limit, ct);
        if (history is null) return NotFound();
        return Ok(history);
    }
}
