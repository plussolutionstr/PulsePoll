using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Api.Controllers;

[Route("survey-result")]
[EnableRateLimiting("survey-result")]
public class SurveyResultController(
    ISubjectRepository subjectRepository,
    IAssignmentService assignmentService,
    ILogger<SurveyResultController> logger) : ControllerBase
{
    [HttpGet("completed")]
    public Task<IActionResult> Completed([FromQuery] Guid sguid, [FromQuery] int pid)
        => HandleAsync(sguid, pid, AssignmentStatus.Completed);

    [HttpGet("disqualify")]
    public Task<IActionResult> Disqualify([FromQuery] Guid sguid, [FromQuery] int pid)
        => HandleAsync(sguid, pid, AssignmentStatus.Disqualify);

    [HttpGet("quotafull")]
    public Task<IActionResult> QuotaFull([FromQuery] Guid sguid, [FromQuery] int pid)
        => HandleAsync(sguid, pid, AssignmentStatus.QuotaFull);

    [HttpGet("screenout")]
    public Task<IActionResult> ScreenOut([FromQuery] Guid sguid, [FromQuery] int pid)
        => HandleAsync(sguid, pid, AssignmentStatus.ScreenOut);

    [HttpGet("partial")]
    public Task<IActionResult> Partial([FromQuery] Guid sguid, [FromQuery] int pid)
        => HandleAsync(sguid, pid, AssignmentStatus.Partial);

    [HttpGet("set")]
    public Task<IActionResult> Set([FromQuery] Guid sguid, [FromQuery] int pid, [FromQuery] string status)
    {
        if (!Enum.TryParse<AssignmentStatus>(status, ignoreCase: true, out var parsed)
            || parsed == AssignmentStatus.NotStarted)
        {
            return Task.FromResult<IActionResult>(ErrorPage("Geçersiz durum parametresi."));
        }

        return HandleAsync(sguid, pid, parsed);
    }

    private async Task<IActionResult> HandleAsync(Guid sguid, int pid, AssignmentStatus status)
    {
        if (sguid == Guid.Empty || pid <= 0)
            return ErrorPage("Eksik veya geçersiz parametreler.");

        try
        {
            var subject = await subjectRepository.GetByPublicIdAsync(sguid);
            if (subject is null)
            {
                logger.LogWarning("Survey result: subject not found for sguid {Sguid}", sguid);
                return ErrorPage("Kullanıcı bulunamadı.");
            }

            await assignmentService.MarkResultAsync(pid, subject.Id, status);

            return SuccessPage(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Survey result error: sguid={Sguid}, pid={Pid}, status={Status}", sguid, pid, status);
            return ErrorPage("İşlem sırasında bir hata oluştu.");
        }
    }

    private static ContentResult SuccessPage(AssignmentStatus status)
        => new()
        {
            Content = $"""
                <!DOCTYPE html>
                <html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
                <title>PulsePoll</title></head>
                <body style="font-family:sans-serif;text-align:center;padding:40px">
                <h2>Sonucunuz kaydedildi</h2>
                <p>Durum: {status}</p>
                <p>Bu sayfayı kapatabilirsiniz.</p>
                </body></html>
                """,
            ContentType = "text/html; charset=utf-8",
            StatusCode = 200
        };

    private static ContentResult ErrorPage(string message)
        => new()
        {
            Content = $"""
                <!DOCTYPE html>
                <html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
                <title>PulsePoll - Hata</title></head>
                <body style="font-family:sans-serif;text-align:center;padding:40px">
                <h2>Hata</h2>
                <p>{message}</p>
                </body></html>
                """,
            ContentType = "text/html; charset=utf-8",
            StatusCode = 400
        };
}
