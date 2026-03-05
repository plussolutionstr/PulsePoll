using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/lookups")]
public class LookupController(ILookupService lookupService) : ControllerBase
{
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await lookupService.GetCitiesAsync();
        return this.OkResponse(cities.Select(c => new { c.Id, c.Name }));
    }

    [HttpGet("cities/{cityId:int}/districts")]
    public async Task<IActionResult> GetDistricts(int cityId)
    {
        var districts = await lookupService.GetDistrictsByCityIdAsync(cityId);
        return this.OkResponse(districts.Select(d => new { d.Id, d.Name }));
    }

    [HttpGet("professions")]
    public async Task<IActionResult> GetProfessions()
    {
        var professions = await lookupService.GetProfessionsAsync();
        return this.OkResponse(professions.Select(p => new { p.Id, p.Name }));
    }

    [HttpGet("education-levels")]
    public async Task<IActionResult> GetEducationLevels()
    {
        var levels = await lookupService.GetEducationLevelsAsync();
        return this.OkResponse(levels.Select(e => new { e.Id, e.Name }));
    }
}
