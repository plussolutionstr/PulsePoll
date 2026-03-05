using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController(IStorageService storageService) : ControllerBase
{
    private static readonly HashSet<string> AllowedBuckets =
    [
        "media-library", "stories", "profile-photos", "customers"
    ];

    [AllowAnonymous]
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
}
