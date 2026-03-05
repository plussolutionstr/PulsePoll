using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Enums;

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

    [HttpPost("{id:int}/result")]
    [EnableRateLimiting("project-start")]
    public async Task<IActionResult> SetResult(int id, [FromBody] SetProjectResultRequest request)
    {
        if (!Enum.TryParse<AssignmentStatus>(request.Status, true, out var status))
            throw new BusinessException("INVALID_ASSIGNMENT_STATUS", "Geçersiz anket sonucu.");

        await assignmentService.MarkResultAsync(id, SubjectId, status, request.RawPayload);
        return this.NoContentResponse();
    }
}

public record SetProjectResultRequest(string Status, string? RawPayload = null);
