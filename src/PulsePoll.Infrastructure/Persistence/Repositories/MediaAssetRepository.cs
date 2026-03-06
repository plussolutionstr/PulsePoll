using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class MediaAssetRepository(AppDbContext db) : IMediaAssetRepository
{
    public Task<List<MediaAsset>> GetAllAsync()
        => db.MediaAssets
             .Include(a => a.Projects.Where(p => p.DeletedAt == null))
             .Include(a => a.Stories.Where(s => s.DeletedAt == null))
             .Include(a => a.StoryImages.Where(s => s.DeletedAt == null))
             .Include(a => a.News.Where(n => n.DeletedAt == null))
             .Include(a => a.BankThumbnails)
             .Include(a => a.BankLogos)
             .Where(a => a.DeletedAt == null)
             .OrderByDescending(a => a.CreatedAt)
             .ToListAsync();

    public Task<MediaAsset?> GetByIdAsync(int id)
        => db.MediaAssets
             .Include(a => a.Projects.Where(p => p.DeletedAt == null))
             .Include(a => a.Stories.Where(s => s.DeletedAt == null))
             .Include(a => a.StoryImages.Where(s => s.DeletedAt == null))
             .Include(a => a.News.Where(n => n.DeletedAt == null))
             .Include(a => a.BankThumbnails)
             .Include(a => a.BankLogos)
             .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);

    public async Task AddAsync(MediaAsset asset)
    {
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(MediaAsset asset)
    {
        db.MediaAssets.Update(asset);
        await db.SaveChangesAsync();
    }
}
