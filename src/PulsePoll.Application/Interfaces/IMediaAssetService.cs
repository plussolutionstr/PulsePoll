using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IMediaAssetService
{
    Task<List<MediaAssetDto>> GetAllAsync();
    Task<MediaAssetDto> UploadAsync(Stream stream, string fileName, string contentType, long size, int adminId);
    Task DeleteAsync(int id, int adminId);
}
