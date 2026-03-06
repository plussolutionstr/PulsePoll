using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Middleware;

public class SessionValidationMiddleware(RequestDelegate next)
{
    private static string SessionKey(int subjectId) => $"session:{subjectId}";

    public async Task InvokeAsync(HttpContext context, ICacheService cache)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sub = context.User.FindFirst("sub")?.Value;
            if (sub is not null && int.TryParse(sub, out var subjectId))
            {
                if (!await cache.ExistsAsync(SessionKey(subjectId)))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        error = new { code = "SESSION_EXPIRED", message = "Oturum sonlandırılmış. Lütfen tekrar giriş yapın." }
                    });
                    return;
                }
            }
        }

        await next(context);
    }
}
