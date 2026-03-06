using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController(IStorageService storageService, IMediaUrlService mediaUrlService) : ControllerBase
{
    private static readonly HashSet<string> PublicBuckets = ["media-library", "stories"];
    private static readonly HashSet<string> AllowedBuckets = ["media-library", "stories", "profile-photos", "customers"];

    [AllowAnonymous]
    [HttpGet("public/{bucket}/{**objectKey}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPublic(string bucket, string objectKey)
    {
        if (!PublicBuckets.Contains(bucket))
            return NotFound();

        if (string.IsNullOrWhiteSpace(objectKey))
            return NotFound();

        try
        {
            var (stream, contentType) = await storageService.GetObjectStreamAsync(bucket, objectKey);
            Response.Headers["Cache-Control"] = "public, max-age=86400, immutable";
            return File(stream, contentType);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpGet("{bucket}/{**objectKey}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(string bucket, string objectKey)
    {
        if (!AllowedBuckets.Contains(bucket))
            return NotFound();

        if (string.IsNullOrWhiteSpace(objectKey))
            return NotFound();

        try
        {
            var (stream, contentType) = await storageService.GetObjectStreamAsync(bucket, objectKey);
            Response.Headers["Cache-Control"] = "public, max-age=86400, immutable";
            return File(stream, contentType);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpGet("profile-photo-url")]
    public async Task<IActionResult> GetProfilePhotoUrl([FromQuery] string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return NotFound();

        var url = await mediaUrlService.GetMediaUrlAsync("profile-photos", objectKey);
        return Ok(new { url });
    }
}
