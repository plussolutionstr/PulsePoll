using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public async Task AddAsync(Notification notification)
    {
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }

    public async Task AddManyAsync(IEnumerable<Notification> notifications)
    {
        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync();
    }

    public Task<List<Notification>> GetBySubjectIdAsync(int subjectId, int skip = 0, int take = 50)
        => db.Notifications
             .Where(n => n.SubjectId == subjectId && n.DeletedAt == null)
             .OrderByDescending(n => n.CreatedAt)
             .Skip(skip)
             .Take(take)
             .ToListAsync();

    public async Task MarkAllReadAsync(int subjectId)
    {
        await db.Notifications
            .Where(n => n.SubjectId == subjectId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task UpdateDeliveryStatusAsync(int notificationId, DeliveryStatus status, string? errorMessage)
    {
        await db.Notifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.DeliveryStatus, status)
                .SetProperty(n => n.ErrorMessage, errorMessage));
    }

    public async Task UpdateDeliveryStatusBulkAsync(IEnumerable<int> notificationIds, DeliveryStatus status)
    {
        var ids = notificationIds.Distinct().ToList();
        if (ids.Count == 0)
            return;

        await db.Notifications
            .Where(n => ids.Contains(n.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.DeliveryStatus, status)
                .SetProperty(n => n.ErrorMessage, (string?)null));
    }

    public async Task<(List<Notification> Items, int Total)> GetPagedAsync(
        int skip,
        int take,
        DeliveryStatus? statusFilter = null,
        string? typeFilter = null)
    {
        var query = db.Notifications
            .Include(n => n.Subject)
            .Where(n => n.DeletedAt == null)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(n => n.DeliveryStatus == statusFilter.Value);

        if (!string.IsNullOrWhiteSpace(typeFilter))
            query = query.Where(n => n.Type != null && n.Type.Contains(typeFilter));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, total);
    }
}
