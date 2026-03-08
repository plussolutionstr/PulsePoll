using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IDistributionService
{
    Task<DistributionRunResultDto> RunHourlyDistributionAsync(int projectId);
    Task<List<DistributionRunResultDto>> RunAllHourlyDistributionsAsync();
    Task<List<DistributionReminderResultDto>> RunDueReminderNotificationsAsync();
    Task<int> SendReminderNotificationsAsync(int projectId);
    Task<DistributionProgressDto> GetDistributionProgressAsync(int projectId);
    Task<List<DistributionLogDto>> GetDistributionLogsAsync(int projectId);

    // Bildirim batch dağıtımı (non-scheduled projeler)
    Task<int> RunNotificationBatchAsync();
}
