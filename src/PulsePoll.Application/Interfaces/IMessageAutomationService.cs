using PulsePoll.Application.DTOs;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IMessageAutomationService
{
    Task<List<MessageTemplateDto>> GetTemplatesAsync();
    Task SaveTemplateAsync(SaveMessageTemplateDto dto, int adminId);
    Task DeleteTemplateAsync(int templateId, int adminId);

    Task<List<MessageCampaignDto>> GetCampaignsAsync();
    Task SaveCampaignAsync(SaveMessageCampaignDto dto, int adminId);

    Task<MessageDispatchRunResultDto> RunCampaignAsync(int campaignId, DateOnly occurrenceDate, int adminId);
    Task<List<MessageDispatchRunResultDto>> RunDueCampaignsAsync(
        DateOnly occurrenceDate,
        MessageTriggerType triggerType,
        string? triggerKey,
        int adminId);
}
