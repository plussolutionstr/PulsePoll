using PulsePoll.Application.Interfaces;

namespace PulsePoll.Worker.Jobs;

public class NotificationBatchRecurringJob(
    IDistributionService distributionService,
    ILogger<NotificationBatchRecurringJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Bildirim batch dağıtımı başlatıldı.");
        var totalSent = await distributionService.RunNotificationBatchAsync();
        logger.LogInformation("Bildirim batch dağıtımı tamamlandı. Toplam={Total}", totalSent);
    }
}
