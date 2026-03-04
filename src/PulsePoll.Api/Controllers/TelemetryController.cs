using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/telemetry")]
public class TelemetryController(ISubjectTelemetryService telemetryService) : ControllerBase
{
    private int SubjectId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpPost("activity")]
    [EnableRateLimiting("telemetry")]
    public async Task<IActionResult> TrackActivity([FromBody] TrackSubjectActivityDto dto)
    {
        await telemetryService.TrackActivityAsync(SubjectId, dto);
        return this.AcceptedResponse();
    }
}

