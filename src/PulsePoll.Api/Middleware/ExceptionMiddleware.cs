using FluentValidation;
using PulsePoll.Api.Models;
using PulsePoll.Application.Exceptions;

namespace PulsePoll.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var requestId = context.Items.TryGetValue("CorrelationId", out var corrId)
            ? corrId?.ToString() ?? context.TraceIdentifier
            : context.TraceIdentifier;

        if (ex is BusinessException or ValidationException or NotFoundException or ForbiddenException or UnauthorizedAccessException)
            logger.LogWarning(ex, "İstek iş kuralı/doğrulama hatası ile sonuçlandı. RequestId: {RequestId}", requestId);
        else
            logger.LogError(ex, "İşlenmemiş hata. RequestId: {RequestId}", requestId);

        var (statusCode, response) = ex switch
        {
            ValidationException vex => (
                StatusCodes.Status400BadRequest,
                ApiResponseFactory.ValidationError(
                    vex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    requestId)),

            NotFoundException nex => (
                StatusCodes.Status404NotFound,
                ApiResponseFactory.NotFound(nex.ResourceName, requestId)),

            BusinessException bex => (
                StatusCodes.Status400BadRequest,
                ApiResponseFactory.Error(bex.Code, bex.Message, requestId)),

            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                ApiResponseFactory.Forbidden(requestId)),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                ApiResponseFactory.Unauthorized(requestId)),

            _ => (
                StatusCodes.Status500InternalServerError,
                ApiResponseFactory.Error(
                    ErrorCodes.InternalError,
                    env.IsDevelopment() ? ex.Message : "Beklenmeyen bir hata oluştu.",
                    requestId))
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
