using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    ISubjectRepository subjectRepository,
    IMessagePublisher publisher,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<IEnumerable<NotificationDto>> GetBySubjectIdAsync(int subjectId)
    {
        var notifications = await notificationRepository.GetBySubjectIdAsync(subjectId);
        return notifications.Select(n => new NotificationDto(n.Id, n.Title, n.Body, n.Type, n.IsRead, n.CreatedAt));
    }

    public Task MarkAllReadAsync(int subjectId)
        => notificationRepository.MarkAllReadAsync(subjectId);

    public Task MarkOneReadAsync(int notificationId, int subjectId)
        => notificationRepository.MarkOneReadAsync(notificationId, subjectId);

    public Task DeleteAsync(int notificationId, int subjectId)
        => notificationRepository.SoftDeleteAsync(notificationId, subjectId);

    public async Task SendPushAsync(int subjectId, string title, string body, string? type = null, int? sentByAdminId = null)
    {
        var notification = new Notification
        {
            SubjectId = subjectId,
            Title     = title,
            Body      = body,
            Type      = type
        };
        notification.SetCreated(sentByAdminId ?? 0);
        await notificationRepository.AddAsync(notification);

        var subject = await subjectRepository.GetByIdAsync(subjectId);
        if (subject?.FcmToken is not null)
        {
            await publisher.PublishAsync(
                new NotificationSendMessage(notification.Id, subjectId, subject.FcmToken, title, body, type),
                Queues.NotificationSend);
        }
        else
        {
            await notificationRepository.UpdateDeliveryStatusAsync(notification.Id, DeliveryStatus.Skipped, null);
        }

        logger.LogInformation("Bildirim gönderildi: SubjectId={SubjectId} Title={Title}", subjectId, title);
    }

    public async Task SendPushToManyAsync(IEnumerable<int> subjectIds, string title, string body, string? type = null, int? sentByAdminId = null)
    {
        var idList = subjectIds.Distinct().ToList();
        if (idList.Count == 0) return;

        var notifications = idList.Select(sid =>
        {
            var n = new Notification { SubjectId = sid, Title = title, Body = body, Type = type };
            n.SetCreated(sentByAdminId ?? 0);
            return n;
        }).ToList();

        await notificationRepository.AddManyAsync(notifications);

        var fcmTokens = await subjectRepository.GetFcmTokensByIdsAsync(idList);
        var notifBySubject = notifications.ToDictionary(n => n.SubjectId);

        var skippedNotificationIds = idList
            .Except(fcmTokens.Keys)
            .Select(subjectId => notifBySubject[subjectId].Id)
            .ToList();

        if (skippedNotificationIds.Count > 0)
            await notificationRepository.UpdateDeliveryStatusBulkAsync(skippedNotificationIds, DeliveryStatus.Skipped);

        foreach (var (subjectId, fcmToken) in fcmTokens)
        {
            if (notifBySubject.TryGetValue(subjectId, out var notification))
            {
                await publisher.PublishAsync(
                    new NotificationSendMessage(notification.Id, subjectId, fcmToken, title, body, type),
                    Queues.NotificationSend);
            }
        }

        logger.LogInformation("Toplu bildirim gönderildi: {Count} denek, Title={Title}", idList.Count, title);
    }
}
