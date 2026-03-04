using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task AddManyAsync(IEnumerable<Notification> notifications);
    Task<List<Notification>> GetBySubjectIdAsync(int subjectId, int skip = 0, int take = 50);
    Task MarkAllReadAsync(int subjectId);
    Task UpdateDeliveryStatusAsync(int notificationId, DeliveryStatus status, string? errorMessage);
    Task UpdateDeliveryStatusBulkAsync(IEnumerable<int> notificationIds, DeliveryStatus status);
    Task<(List<Notification> Items, int Total)> GetPagedAsync(
        int skip,
        int take,
        DeliveryStatus? statusFilter = null,
        string? typeFilter = null);
}
