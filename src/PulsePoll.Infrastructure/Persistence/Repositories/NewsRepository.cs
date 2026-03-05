using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class NewsRepository(AppDbContext db) : INewsRepository
{
    public Task<List<News>> GetAllAsync()
        => db.News
            .Include(n => n.MediaAsset)
            .Where(n => n.DeletedAt == null)
            .OrderBy(n => n.Order)
            .ThenByDescending(n => n.StartsAt)
            .ToListAsync();

    public Task<List<News>> GetActiveAsync()
    {
        var now = TurkeyTime.Now;
        return db.News
            .Include(n => n.MediaAsset)
            .Where(n => n.IsActive && n.StartsAt <= now && n.EndsAt >= now && n.DeletedAt == null)
            .OrderBy(n => n.Order)
            .ToListAsync();
    }

    public Task<News?> GetByIdAsync(int id)
        => db.News
            .Include(n => n.MediaAsset)
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);

    public async Task AddAsync(News news)
    {
        db.News.Add(news);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(News news)
    {
        db.News.Update(news);
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(IReadOnlyCollection<(int Id, int Order)> orders)
    {
        if (orders.Count == 0)
            return;

        var orderMap = orders.ToDictionary(x => x.Id, x => x.Order);
        var ids = orderMap.Keys.ToList();
        var newsItems = await db.News
            .Where(n => n.DeletedAt == null && ids.Contains(n.Id))
            .ToListAsync();

        foreach (var news in newsItems)
        {
            if (orderMap.TryGetValue(news.Id, out var order))
            {
                news.Order = order;
                news.SetUpdated(userId: 0);
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(News news)
    {
        db.News.Update(news);
        await db.SaveChangesAsync();
    }
}
