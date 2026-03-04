using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[ApiController]
[Route("api/stories")]
public class StoryController(IStoryService storyService) : ControllerBase
{
    /// <summary>Mobil ana ekran — aktif hikayeleri döner.</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var stories = await storyService.GetActiveStoriesAsync();
        return this.OkResponse(stories);
    }

    // TODO: Admin auth eklenince [Authorize(Roles = "Admin")] eklenmeli
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateStoryFormDto dto)
    {
        var createDto = new CreateStoryDto(
            dto.Title,
            dto.Description,
            dto.LinkUrl,
            dto.StartsAt,
            dto.EndsAt,
            dto.Order,
            dto.IsActive,
            dto.MediaAssetId,
            dto.StoryMediaAssetId,
            dto.Image?.OpenReadStream(),
            dto.Image?.FileName);

        var created = await storyService.CreateAsync(createDto);
        return this.OkResponse(created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] CreateStoryFormDto dto)
    {
        var updateDto = new CreateStoryDto(
            dto.Title,
            dto.Description,
            dto.LinkUrl,
            dto.StartsAt,
            dto.EndsAt,
            dto.Order,
            dto.IsActive,
            dto.MediaAssetId,
            dto.StoryMediaAssetId,
            dto.Image?.OpenReadStream(),
            dto.Image?.FileName);

        await storyService.UpdateAsync(id, updateDto);
        return this.NoContentResponse();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await storyService.DeleteAsync(id);
        return this.NoContentResponse();
    }
}

/// <summary>Multipart/form-data DTO — IFormFile JSON serialize edilemez.</summary>
public class CreateStoryFormDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LinkUrl { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public int? MediaAssetId { get; set; }
    public int? StoryMediaAssetId { get; set; }
    public IFormFile? Image { get; set; }
}
