using PulsePoll.Application.DTOs;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IPaymentBatchService
{
    Task<(List<WithdrawalRequestAdminDto> Items, int Total)> GetPendingWithdrawalsAsync(int skip, int take);
    Task<(List<WithdrawalRequestAdminDto> Items, int Total)> GetApprovedWithdrawalsAsync(int skip, int take);
    Task ApproveWithdrawalAsync(int requestId, int adminId);
    Task RejectWithdrawalAsync(int requestId, string reason, int adminId);

    Task<(List<PaymentBatchDto> Items, int Total)> GetBatchesAsync(PaymentBatchStatus? status, int skip, int take);
    Task<PaymentBatchDetailDto> GetBatchDetailAsync(int batchId);
    Task<PaymentBatchDto> CreateBatchAsync(CreatePaymentBatchDto dto, int adminId);
    Task MarkBatchSentAsync(int batchId, int adminId);
    Task UpdateItemStatusAsync(int batchId, int itemId, UpdatePaymentItemStatusDto dto, int adminId);
    Task FinalizeBatchAsync(int batchId, int adminId);
    Task<PaymentExportFileDto> GenerateBankTransferFileAsync(int batchId);

    Task<List<PaymentSettingDto>> GetSettingsAsync();
    Task UpsertSettingAsync(string key, string value, int adminId, string? description = null);
}
