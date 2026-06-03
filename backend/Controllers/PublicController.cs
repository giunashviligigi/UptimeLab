using Microsoft.AspNetCore.Mvc;
using UptimeLab.Api.DTOs;
using UptimeLab.Api.Services;

namespace UptimeLab.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly ISiteService _sites;

    public PublicController(ISiteService sites) => _sites = sites;

    /// <summary>Public status page — no auth required.</summary>
    [HttpGet("status/{userId:guid}")]
    public async Task<ActionResult<PublicStatusResponse>> GetStatus(Guid userId, CancellationToken ct)
    {
        var status = await _sites.GetPublicStatusAsync(userId, ct);
        if (status is null) return NotFound(new { message = "User not found." });
        return Ok(status);
    }
}
