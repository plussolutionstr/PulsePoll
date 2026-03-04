using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetBySubjectIdAsync(int subjectId);
    Task MarkAllReadAsync(int subjectId);
    Task SendPushAsync(int subjectId, string title, string body, string? type = null, int? sentByAdminId = null);
    Task SendPushToManyAsync(IEnumerable<int> subjectIds, string title, string body, string? type = null, int? sentByAdminId = null);
}
