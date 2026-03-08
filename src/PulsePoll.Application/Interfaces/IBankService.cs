using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IBankService
{
    Task<List<Bank>> GetAllAsync();
    Task ToggleActiveAsync(int bankId);
    Task CreateOrUpdateAsync(int id, string name, string? code, string? bankCode, bool isActive, int? thumbnailMediaAssetId, int? logoMediaAssetId);
}
