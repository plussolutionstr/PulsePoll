using Hangfire;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Worker.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 60)]
public class SurveyDistributionRecurringJob(
    IDistributionService distributionService,
    ILogger<SurveyDistributionRecurringJob> logger)
{
    public async Task ExecuteAsync()
    {
        var results = await distributionService.RunAllHourlyDistributionsAsync();

        if (results.Count == 0)
        {
            logger.LogInformation("Saatlik dağıtım: Aktif zamanlanmış proje yok.");
            return;
        }

        foreach (var r in results)
        {
            logger.LogInformation(
                "Dağıtım turu: Project={ProjectId} '{ProjectName}' Distributed={Count} Remaining={Remaining} DailyQuota={DQ} LastDay={LD}",
                r.ProjectId, r.ProjectName, r.DistributedCount, r.RemainingScheduled, r.DailyQuota, r.IsLastDayFlush);
        }
    }
}
