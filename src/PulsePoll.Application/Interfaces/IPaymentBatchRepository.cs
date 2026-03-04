using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IPaymentBatchRepository
{
    Task<PaymentBatch?> GetByIdAsync(int id);
    Task<PaymentBatch?> GetDetailAsync(int id);
    Task<(List<PaymentBatch> Items, int Total)> GetPagedAsync(PaymentBatchStatus? status, int skip, int take);
    Task<int> GetNextSequenceAsync(string datePrefix);
    Task AddAsync(PaymentBatch batch);
    Task UpdateAsync(PaymentBatch batch);
}
