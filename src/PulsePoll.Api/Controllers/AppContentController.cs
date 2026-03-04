using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/app-content")]
public class AppContentController(IAppContentConfigService appContentConfigService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dto = await appContentConfigService.GetAsync();
        return this.OkResponse(dto);
    }
}
