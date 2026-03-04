using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IMediaAssetRepository
{
    Task<List<MediaAsset>> GetAllAsync();
    Task<MediaAsset?> GetByIdAsync(int id);
    Task AddAsync(MediaAsset asset);
    Task DeleteAsync(MediaAsset asset);
}
