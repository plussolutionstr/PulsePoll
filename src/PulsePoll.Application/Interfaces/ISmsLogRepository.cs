using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface ISmsLogRepository
{
    Task AddAsync(SmsLog log);
    Task<(List<SmsLog> Items, int Total)> GetPagedAsync(
        int skip,
        int take,
        string? phoneFilter = null,
        SmsSource? sourceFilter = null,
        DeliveryStatus? statusFilter = null);
}
