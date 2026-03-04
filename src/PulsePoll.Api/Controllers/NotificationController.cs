using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    private int SubjectId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var notifications = await notificationService.GetBySubjectIdAsync(SubjectId);
        return this.OkResponse(notifications);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await notificationService.MarkAllReadAsync(SubjectId);
        return this.NoContentResponse();
    }
}
