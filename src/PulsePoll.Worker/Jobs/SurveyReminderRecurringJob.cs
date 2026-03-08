using Hangfire;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Worker.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 60)]
public class SurveyReminderRecurringJob(
    IProjectRepository projectRepository,
    IDistributionService distributionService,
    ILogger<SurveyReminderRecurringJob> logger)
{
    public async Task ExecuteAsync()
    {
        var projects = await projectRepository.GetActiveScheduledDistributionProjectsAsync();

        if (projects.Count == 0)
        {
            logger.LogInformation("Hatırlatma: Aktif zamanlanmış proje yok.");
            return;
        }

        foreach (var project in projects)
        {
            var count = await distributionService.SendReminderNotificationsAsync(project.Id);
            logger.LogInformation("Hatırlatma gönderildi: Project={ProjectId} Count={Count}", project.Id, count);
        }
    }
}
