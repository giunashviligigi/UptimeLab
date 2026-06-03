using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UptimeLab.Api.DTOs;
using UptimeLab.Api.Services;

namespace UptimeLab.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IUserSettingsService _settings;

    public SettingsController(IUserSettingsService settings) => _settings = settings;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<UserSettingsResponse>> Get(CancellationToken ct)
    {
        var s = await _settings.GetAsync(GetUserId(), ct);
        if (s is null) return NotFound();
        return Ok(s);
    }

    [HttpPut]
    public async Task<ActionResult<UserSettingsResponse>> Update(
        [FromBody] UpdateUserSettingsRequest request,
        CancellationToken ct)
    {
        var s = await _settings.UpdateAsync(GetUserId(), request, ct);
        if (s is null) return NotFound();
        return Ok(s);
    }
}
