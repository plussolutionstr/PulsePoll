using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.DTOs;
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

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        await subjectService.UpdateProfileAsync(SubjectId, dto);
        var updated = await subjectService.GetByIdAsync(SubjectId);
        return this.OkResponse(updated);
    }

    [HttpPost("photo")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        if (file.Length == 0)
            return this.BadRequestResponse("INVALID_FILE", "Dosya boş.");

        if (file.Length > 5 * 1024 * 1024)
            return this.BadRequestResponse("FILE_TOO_LARGE", "Dosya 5 MB'dan büyük olamaz.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return this.BadRequestResponse("INVALID_FILE_TYPE", "Sadece JPEG, PNG veya WebP formatları kabul edilir.");

        await using var stream = file.OpenReadStream();
        var url = await subjectService.UploadProfilePhotoAsync(SubjectId, stream, file.ContentType, file.FileName);
        return this.OkResponse(new { url });
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
