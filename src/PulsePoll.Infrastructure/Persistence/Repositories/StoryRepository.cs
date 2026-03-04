using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class StoryRepository(AppDbContext db) : IStoryRepository
{
    public Task<List<Story>> GetAllAsync()
        => db.Stories
            .Include(s => s.MediaAsset)
            .Where(s => s.DeletedAt == null)
            .OrderBy(s => s.Order)
            .ThenByDescending(s => s.StartsAt)
            .ToListAsync();

    public Task<List<Story>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;
        return db.Stories
            .Include(s => s.MediaAsset)
            .Where(s => s.IsActive && s.StartsAt <= now && s.EndsAt >= now && s.DeletedAt == null)
            .OrderBy(s => s.Order)
            .ToListAsync();
    }

    public Task<Story?> GetByIdAsync(int id)
        => db.Stories
            .Include(s => s.MediaAsset)
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);

    public async Task AddAsync(Story story)
    {
        db.Stories.Add(story);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Story story)
    {
        db.Stories.Update(story);
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(IReadOnlyCollection<(int Id, int Order)> orders)
    {
        if (orders.Count == 0)
            return;

        var orderMap = orders.ToDictionary(x => x.Id, x => x.Order);
        var ids = orderMap.Keys.ToList();
        var stories = await db.Stories
            .Where(s => s.DeletedAt == null && ids.Contains(s.Id))
            .ToListAsync();

        foreach (var story in stories)
        {
            if (orderMap.TryGetValue(story.Id, out var order))
            {
                story.Order = order;
                story.SetUpdated(userId: 0);
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Story story)
    {
        db.Stories.Update(story);
        await db.SaveChangesAsync();
    }
}
