using Serilog.Core;
using Serilog.Events;

namespace PulsePoll.Api.Logging;

public class UserContextEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return;

        var subjectId = httpContext.User.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(subjectId))
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SubjectId", subjectId));
    }
}
