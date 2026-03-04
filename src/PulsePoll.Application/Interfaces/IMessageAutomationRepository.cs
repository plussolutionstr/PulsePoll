using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IMessageAutomationRepository
{
    Task<List<MessageTemplate>> GetTemplatesAsync();
    Task<MessageTemplate?> GetTemplateByIdAsync(int id);
    Task<bool> HasCampaignUsingTemplateAsync(int templateId);
    Task AddTemplateAsync(MessageTemplate template);
    Task UpdateTemplateAsync(MessageTemplate template);
    Task DeleteTemplateAsync(MessageTemplate template);

    Task<List<MessageCampaign>> GetCampaignsAsync();
    Task<List<MessageCampaign>> GetActiveCampaignsByTriggerAsync(MessageTriggerType triggerType);
    Task<MessageCampaign?> GetCampaignByIdAsync(int id);
    Task AddCampaignAsync(MessageCampaign campaign);
    Task UpdateCampaignAsync(MessageCampaign campaign);

    Task<bool> DispatchLogExistsAsync(int campaignId, int subjectId, DateOnly occurrenceDate, MessageChannelType channelType);
    Task AddDispatchLogAsync(MessageDispatchLog log);
}
