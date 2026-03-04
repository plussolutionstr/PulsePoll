using Microsoft.Extensions.Logging;

namespace PulsePoll.Infrastructure.Notifications;

public class SmtpMailService(ILogger<SmtpMailService> logger)
{
    public Task SendAsync(string to, string subject, string htmlBody)
    {
        logger.LogInformation("[MOCK MAIL] To: {To} | Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
