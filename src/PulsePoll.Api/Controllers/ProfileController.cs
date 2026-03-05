using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Services;

namespace PulsePoll.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController(
    ISubjectService subjectService,
    SubjectProjectHistoryService historyService) : ControllerBase
{
    private int SubjectId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var subject = await subjectService.GetByIdAsync(SubjectId);
        return this.OkResponse(subject);
    }

    [HttpPut("fcm-token")]
    public async Task<IActionResult> UpdateFcmToken([FromBody] string fcmToken)
    {
        await subjectService.UpdateFcmTokenAsync(SubjectId, fcmToken);
        return this.NoContentResponse();
    }

    [HttpGet("projects/history")]
    public async Task<IActionResult> GetProjectHistory()
    {
        var history = await historyService.GetSubjectProjectHistoryAsync(SubjectId);
        return this.OkResponse(history);
    }
}
