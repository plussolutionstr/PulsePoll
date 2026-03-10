using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/telemetry")]
public class TelemetryController(IMessagePublisher publisher) : ControllerBase
{
    private int SubjectId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpPost("activity")]
    [EnableRateLimiting("telemetry")]
    public async Task<IActionResult> TrackActivity(
        [FromBody] TrackSubjectActivityDto dto,
        CancellationToken ct)
    {
        var message = new SubjectActivityTrackedMessage(
            SubjectId,
            (int)dto.Type,
            dto.Platform,
            dto.AppVersion,
            dto.DeviceId,
            TurkeyTime.Now);

        await publisher.PublishAsync(message, Queues.SubjectActivityTracked, ct);
        return this.AcceptedResponse();
    }
}
