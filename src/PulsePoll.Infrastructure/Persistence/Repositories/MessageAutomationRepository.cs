using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class MessageAutomationRepository(AppDbContext db) : IMessageAutomationRepository
{
    public Task<List<MessageTemplate>> GetTemplatesAsync()
        => db.MessageTemplates
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .ToListAsync();

    public Task<MessageTemplate?> GetTemplateByIdAsync(int id)
        => db.MessageTemplates.FirstOrDefaultAsync(x => x.Id == id);

    public Task<bool> HasCampaignUsingTemplateAsync(int templateId)
        => db.MessageCampaigns.AnyAsync(x => x.TemplateId == templateId);

    public async Task AddTemplateAsync(MessageTemplate template)
    {
        db.MessageTemplates.Add(template);
        await db.SaveChangesAsync();
    }

    public async Task UpdateTemplateAsync(MessageTemplate template)
    {
        db.MessageTemplates.Update(template);
        await db.SaveChangesAsync();
    }

    public async Task DeleteTemplateAsync(MessageTemplate template)
    {
        db.MessageTemplates.Remove(template);
        await db.SaveChangesAsync();
    }

    public Task<List<MessageCampaign>> GetCampaignsAsync()
        => db.MessageCampaigns
            .AsNoTracking()
            .Include(x => x.Template)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

    public Task<List<MessageCampaign>> GetActiveCampaignsByTriggerAsync(MessageTriggerType triggerType)
        => db.MessageCampaigns
            .AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.IsActive && x.TriggerType == triggerType)
            .ToListAsync();

    public Task<MessageCampaign?> GetCampaignByIdAsync(int id)
        => db.MessageCampaigns
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddCampaignAsync(MessageCampaign campaign)
    {
        db.MessageCampaigns.Add(campaign);
        await db.SaveChangesAsync();
    }

    public async Task UpdateCampaignAsync(MessageCampaign campaign)
    {
        db.MessageCampaigns.Update(campaign);
        await db.SaveChangesAsync();
    }

    public Task<bool> DispatchLogExistsAsync(int campaignId, int subjectId, DateOnly occurrenceDate, MessageChannelType channelType)
        => db.MessageDispatchLogs.AnyAsync(x =>
            x.CampaignId == campaignId &&
            x.SubjectId == subjectId &&
            x.OccurrenceDate == occurrenceDate &&
            x.ChannelType == channelType);

    public async Task<HashSet<(int SubjectId, MessageChannelType ChannelType)>> GetExistingDispatchLogsAsync(
        int campaignId, IEnumerable<int> subjectIds, DateOnly occurrenceDate)
    {
        var ids = subjectIds.ToList();
        var existing = await db.MessageDispatchLogs
            .Where(x => x.CampaignId == campaignId &&
                        ids.Contains(x.SubjectId) &&
                        x.OccurrenceDate == occurrenceDate)
            .Select(x => new { x.SubjectId, x.ChannelType })
            .ToListAsync();

        return existing.Select(x => (x.SubjectId, x.ChannelType)).ToHashSet();
    }

    public async Task AddDispatchLogAsync(MessageDispatchLog log)
    {
        db.MessageDispatchLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task AddDispatchLogsAsync(List<MessageDispatchLog> logs)
    {
        if (logs.Count == 0) return;
        db.MessageDispatchLogs.AddRange(logs);
        await db.SaveChangesAsync();
    }
}
