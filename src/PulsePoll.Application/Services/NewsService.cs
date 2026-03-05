using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class NewsService(
    INewsRepository repository,
    IMediaAssetRepository mediaAssetRepository,
    IMediaUrlService mediaUrlService) : INewsService
{
    private const string MediaLibraryBucketName = "media-library";

    public async Task<List<NewsDto>> GetAllAsync()
    {
        var newsList = await repository.GetAllAsync();
        var dtos = new List<NewsDto>(newsList.Count);

        foreach (var news in newsList)
        {
            var imageUrl = await ResolveImageUrlAsync(news);
            dtos.Add(ToDto(news, imageUrl));
        }

        return dtos;
    }

    public async Task<List<NewsDto>> GetActiveAsync()
    {
        var newsList = await repository.GetActiveAsync();
        var dtos = new List<NewsDto>(newsList.Count);

        foreach (var news in newsList)
        {
            var imageUrl = await ResolveImageUrlAsync(news);
            dtos.Add(ToDto(news, imageUrl));
        }

        return dtos;
    }

    public async Task<NewsDto> CreateAsync(CreateNewsDto dto)
    {
        if (!dto.MediaAssetId.HasValue)
            throw new BusinessException("NEWS_IMAGE_REQUIRED", "Haber görseli zorunludur.");

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(dto.MediaAssetId.Value)
            ?? throw new NotFoundException("Medya dosyası");

        var news = new News
        {
            Title = dto.Title,
            Summary = dto.Summary,
            ImageUrl = mediaAsset.ObjectKey,
            MediaAssetId = mediaAsset.Id,
            LinkUrl = dto.LinkUrl,
            StartsAt = NormalizeToTurkeyLocal(dto.StartsAt),
            EndsAt = NormalizeToTurkeyLocal(dto.EndsAt),
            Order = dto.Order,
            IsActive = dto.IsActive
        };
        news.SetCreated(userId: 0);

        await repository.AddAsync(news);

        var imageUrl = await ResolveImageUrlAsync(news);
        return ToDto(news, imageUrl);
    }

    public async Task UpdateAsync(int id, CreateNewsDto dto)
    {
        var news = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Haber");

        if (dto.MediaAssetId.HasValue && dto.MediaAssetId.Value != news.MediaAssetId)
        {
            var mediaAsset = await mediaAssetRepository.GetByIdAsync(dto.MediaAssetId.Value)
                ?? throw new NotFoundException("Medya dosyası");
            news.ImageUrl = mediaAsset.ObjectKey;
            news.MediaAssetId = mediaAsset.Id;
        }

        news.Title = dto.Title;
        news.Summary = dto.Summary;
        news.LinkUrl = dto.LinkUrl;
        news.StartsAt = NormalizeToTurkeyLocal(dto.StartsAt);
        news.EndsAt = NormalizeToTurkeyLocal(dto.EndsAt);
        news.Order = dto.Order;
        news.IsActive = dto.IsActive;
        news.SetUpdated(userId: 0);

        await repository.UpdateAsync(news);
    }

    public async Task ReorderAsync(IReadOnlyCollection<OrderUpdateDto> orders)
    {
        if (orders.Count == 0)
            return;

        var normalized = orders
            .Where(x => x.Id > 0 && x.Order > 0)
            .Select(x => (x.Id, x.Order))
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
            return;

        await repository.ReorderAsync(normalized);
    }

    public async Task DeleteAsync(int id)
    {
        var news = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Haber");

        news.SetDeleted(userId: 0);
        await repository.DeleteAsync(news);
    }

    private static NewsDto ToDto(News news, string imageUrl) =>
        new(news.Id, news.Title, news.Summary, imageUrl, news.MediaAssetId, news.LinkUrl, news.StartsAt, news.EndsAt, news.Order, news.IsActive);

    private Task<string> ResolveImageUrlAsync(News news)
    {
        var objectKey = news.MediaAsset?.ObjectKey ?? news.ImageUrl;
        return mediaUrlService.GetMediaUrlAsync(MediaLibraryBucketName, objectKey);
    }

    private static DateTime NormalizeToTurkeyLocal(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => TurkeyTime.FromUtc(value),
        DateTimeKind.Local => DateTime.SpecifyKind(value, DateTimeKind.Unspecified),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Unspecified)
    };
}
