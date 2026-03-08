using Hangfire;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Worker.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 60)]
public class SurveyReminderRecurringJob(
    IDistributionService distributionService,
    ILogger<SurveyReminderRecurringJob> logger)
{
    public async Task ExecuteAsync()
    {
        var results = await distributionService.RunDueReminderNotificationsAsync();

        if (results.Count == 0)
        {
            logger.LogInformation("Hatırlatma: Bu saat için gönderilecek proje yok.");
            return;
        }

        foreach (var result in results)
        {
            logger.LogInformation(
                "Hatırlatma gönderildi: Project={ProjectId} '{ProjectName}' Count={Count}",
                result.ProjectId,
                result.ProjectName,
                result.ReminderCount);
        }
    }
}
