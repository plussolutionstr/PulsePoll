using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[ApiController]
[Route("api/news")]
public class NewsController(INewsService newsService) : ControllerBase
{
    /// <summary>Mobil ana ekran — aktif haber kartlarını döner.</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var news = await newsService.GetActiveAsync();
        return this.OkResponse(news);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNewsRequest dto)
    {
        var createDto = new CreateNewsDto(
            dto.Title,
            dto.Summary,
            dto.LinkUrl,
            dto.StartsAt,
            dto.EndsAt,
            dto.Order,
            dto.IsActive,
            dto.MediaAssetId,
            null,
            null);

        var created = await newsService.CreateAsync(createDto);
        return this.OkResponse(created);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateNewsRequest dto)
    {
        var updateDto = new CreateNewsDto(
            dto.Title,
            dto.Summary,
            dto.LinkUrl,
            dto.StartsAt,
            dto.EndsAt,
            dto.Order,
            dto.IsActive,
            dto.MediaAssetId,
            null,
            null);

        await newsService.UpdateAsync(id, updateDto);
        return this.NoContentResponse();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await newsService.DeleteAsync(id);
        return this.NoContentResponse();
    }
}

public class CreateNewsRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public int? MediaAssetId { get; set; }
}
