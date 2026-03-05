using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController(
    IAssignmentService assignmentService,
    IConfiguration configuration,
    ILogger<WebhookController> logger) : ControllerBase
{
    // Anket sağlayıcısı bu endpoint'i çağırır — JWT gerektirmez
    [HttpPost("survey-complete")]
    [EnableRateLimiting("webhook")]
    public async Task<IActionResult> SurveyComplete([FromBody] SurveyCompleteWebhookPayload payload)
    {
        var expectedSecret = configuration["Webhook:SurveyCompleteSecret"];
        if (string.IsNullOrWhiteSpace(expectedSecret))
        {
            logger.LogError("Webhook secret is not configured.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        if (!Request.Headers.TryGetValue("X-Webhook-Secret", out var providedSecret) ||
            !FixedTimeEquals(providedSecret.ToString(), expectedSecret))
            return Unauthorized();

        await assignmentService.MarkCompletedAsync(payload.ProjectId, payload.SubjectId, payload.RawPayload);
        return this.NoContentResponse();
    }

    private static bool FixedTimeEquals(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided.Trim());
        var expectedBytes = Encoding.UTF8.GetBytes(expected.Trim());
        return CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}

public record SurveyCompleteWebhookPayload(int ProjectId, int SubjectId, string RawPayload);
