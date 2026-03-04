using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects")]
public class ProjectController(
    IProjectService projectService,
    IAssignmentService assignmentService) : ControllerBase
{
    private int SubjectId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAssigned()
    {
        var projects = await projectService.GetAssignedProjectsAsync(SubjectId);
        return this.OkResponse(projects);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await projectService.GetByIdAsync(id);
        return this.OkResponse(project);
    }

    [HttpPost("{id:int}/start")]
    [EnableRateLimiting("project-start")]
    public async Task<IActionResult> Start(int id)
    {
        var url = await assignmentService.StartAsync(id, SubjectId);
        return this.OkResponse(new { url });
    }
}
