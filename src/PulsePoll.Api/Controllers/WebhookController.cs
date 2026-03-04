using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController(IAssignmentService assignmentService) : ControllerBase
{
    // Anket sağlayıcısı bu endpoint'i çağırır — JWT gerektirmez
    [HttpPost("survey-complete")]
    [EnableRateLimiting("webhook")]
    public async Task<IActionResult> SurveyComplete([FromBody] SurveyCompleteWebhookPayload payload)
    {
        await assignmentService.MarkCompletedAsync(payload.ProjectId, payload.SubjectId, payload.RawPayload);
        return this.NoContentResponse();
    }
}

public record SurveyCompleteWebhookPayload(int ProjectId, int SubjectId, string RawPayload);
