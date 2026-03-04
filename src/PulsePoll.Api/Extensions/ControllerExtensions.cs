using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Models;
using PulsePoll.Application.Models;

namespace PulsePoll.Api.Extensions;

public static class ControllerExtensions
{
    public static string GetRequestId(this ControllerBase controller)
    {
        var ctx = controller.HttpContext;
        return ctx.Items.TryGetValue("CorrelationId", out var corrId)
            ? corrId?.ToString() ?? ctx.TraceIdentifier
            : ctx.TraceIdentifier;
    }

    public static IActionResult OkResponse<T>(this ControllerBase controller, T data)
        => controller.Ok(ApiResponseFactory.Success(data, controller.GetRequestId()));

    public static IActionResult CreatedResponse<T>(this ControllerBase controller, string actionName, object routeValues, T data)
        => controller.CreatedAtAction(actionName, routeValues, ApiResponseFactory.Success(data, controller.GetRequestId()));

    public static IActionResult NoContentResponse(this ControllerBase controller)
        => controller.Ok(ApiResponseFactory.Success(controller.GetRequestId()));

    public static IActionResult NotFoundResponse(this ControllerBase controller, string resource)
        => controller.NotFound(ApiResponseFactory.NotFound(resource, controller.GetRequestId()));

    public static IActionResult BadRequestResponse(this ControllerBase controller, string code, string message)
        => controller.BadRequest(ApiResponseFactory.Error(code, message, controller.GetRequestId()));

    public static IActionResult ValidationErrorResponse(this ControllerBase controller, Dictionary<string, string[]> errors)
        => controller.BadRequest(ApiResponseFactory.ValidationError(errors, controller.GetRequestId()));

    public static IActionResult AcceptedResponse(this ControllerBase controller)
        => controller.Accepted(ApiResponseFactory.Success(controller.GetRequestId()));

    public static IActionResult OkPagedResponse<T>(this ControllerBase controller, PagedResult<T> result)
        => controller.Ok(ApiResponseFactory.Paged(
            result.Items,
            new PaginationMeta
            {
                Page       = result.Page,
                PageSize   = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            },
            controller.GetRequestId()));
}
