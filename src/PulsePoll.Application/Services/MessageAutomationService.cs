using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class MessageAutomationService(
    IMessageAutomationRepository repository,
    ISubjectRepository subjectRepository,
    INotificationService notificationService,
    IMessagePublisher publisher) : IMessageAutomationService
{
    public async Task<List<MessageTemplateDto>> GetTemplatesAsync()
    {
        var rows = await repository.GetTemplatesAsync();
        return rows
            .OrderByDescending(x => x.Id)
            .Select(MapTemplate)
            .ToList();
    }

    public async Task SaveTemplateAsync(SaveMessageTemplateDto dto, int adminId)
    {
        ValidateTemplate(dto);

        if (dto.Id is null or <= 0)
        {
            var row = new MessageTemplate
            {
                Name = dto.Name.Trim(),
                ChannelType = dto.ChannelType,
                SmsText = NormalizeNullable(dto.SmsText),
                PushTitle = NormalizeNullable(dto.PushTitle),
                PushBody = NormalizeNullable(dto.PushBody),
                IsActive = dto.IsActive
            };
            row.SetCreated(adminId);
            await repository.AddTemplateAsync(row);
            return;
        }

        var existing = await repository.GetTemplateByIdAsync(dto.Id.Value)
            ?? throw new NotFoundException("Mesaj şablonu");

        existing.Name = dto.Name.Trim();
        existing.ChannelType = dto.ChannelType;
        existing.SmsText = NormalizeNullable(dto.SmsText);
        existing.PushTitle = NormalizeNullable(dto.PushTitle);
        existing.PushBody = NormalizeNullable(dto.PushBody);
        existing.IsActive = dto.IsActive;
        existing.SetUpdated(adminId);
        await repository.UpdateTemplateAsync(existing);
    }

    public async Task DeleteTemplateAsync(int templateId, int adminId)
    {
        var existing = await repository.GetTemplateByIdAsync(templateId)
            ?? throw new NotFoundException("Mesaj şablonu");

        if (await repository.HasCampaignUsingTemplateAsync(templateId))
            throw new BusinessException("TEMPLATE_IN_USE", "Şablon kampanyada kullanıldığı için silinemez.");

        existing.SetDeleted(adminId);
        await repository.DeleteTemplateAsync(existing);
    }

    public async Task<List<MessageCampaignDto>> GetCampaignsAsync()
    {
        var rows = await repository.GetCampaignsAsync();
        return rows
            .OrderByDescending(x => x.Id)
            .Select(MapCampaign)
            .ToList();
    }

    public async Task SaveCampaignAsync(SaveMessageCampaignDto dto, int adminId)
    {
        ValidateCampaign(dto);

        var template = await repository.GetTemplateByIdAsync(dto.TemplateId)
            ?? throw new NotFoundException("Mesaj şablonu");

        var normalizedKey = NormalizeTriggerKey(dto.TriggerType, dto.TriggerKey);

        if (dto.Id is null or <= 0)
        {
            var row = new MessageCampaign
            {
                Name = dto.Name.Trim(),
                TriggerType = dto.TriggerType,
                TriggerKey = normalizedKey,
                TargetGender = dto.TargetGender,
                TemplateId = template.Id,
                IsActive = dto.IsActive
            };
            row.SetCreated(adminId);
            await repository.AddCampaignAsync(row);
            return;
        }

        var existing = await repository.GetCampaignByIdAsync(dto.Id.Value)
            ?? throw new NotFoundException("Mesaj kampanyası");

        existing.Name = dto.Name.Trim();
        existing.TriggerType = dto.TriggerType;
        existing.TriggerKey = normalizedKey;
        existing.TargetGender = dto.TargetGender;
        existing.TemplateId = template.Id;
        existing.IsActive = dto.IsActive;
        existing.SetUpdated(adminId);
        await repository.UpdateCampaignAsync(existing);
    }

    public async Task<MessageDispatchRunResultDto> RunCampaignAsync(int campaignId, DateOnly occurrenceDate, int adminId)
    {
        var campaign = await repository.GetCampaignByIdAsync(campaignId)
            ?? throw new NotFoundException("Mesaj kampanyası");

        if (!campaign.IsActive)
            throw new BusinessException("CAMPAIGN_NOT_ACTIVE", "Pasif kampanya çalıştırılamaz.");

        if (!campaign.Template.IsActive)
            throw new BusinessException("TEMPLATE_NOT_ACTIVE", "Pasif şablon ile kampanya çalıştırılamaz.");

        var subjects = await subjectRepository.GetAllAsync();
        var targets = subjects
            .Where(s => s.Status == ApprovalStatus.Approved)
            .Where(s => MatchGender(s.Gender, campaign.TargetGender))
            .Where(s => campaign.TriggerType != MessageTriggerType.Birthday ||
                        (s.BirthDate.Month == occurrenceDate.Month && s.BirthDate.Day == occurrenceDate.Day))
            .ToList();

        var queued = 0;
        var skipped = 0;

        foreach (var subject in targets)
        {
            var smsText = Render(campaign.Template.SmsText, subject, occurrenceDate);
            var pushTitle = Render(campaign.Template.PushTitle, subject, occurrenceDate);
            var pushBody = Render(campaign.Template.PushBody, subject, occurrenceDate);

            if (ShouldSendSms(campaign.Template.ChannelType))
            {
                var result = await TryQueueSmsAsync(campaign, subject, occurrenceDate, smsText, adminId);
                if (result) queued++; else skipped++;
            }

            if (ShouldSendPush(campaign.Template.ChannelType))
            {
                var result = await TryQueuePushAsync(campaign, subject, occurrenceDate, pushTitle, pushBody, adminId);
                if (result) queued++; else skipped++;
            }
        }

        return new MessageDispatchRunResultDto(campaignId, occurrenceDate, targets.Count, queued, skipped);
    }

    public async Task<List<MessageDispatchRunResultDto>> RunDueCampaignsAsync(
        DateOnly occurrenceDate,
        MessageTriggerType triggerType,
        string? triggerKey,
        int adminId)
    {
        var campaigns = await repository.GetActiveCampaignsByTriggerAsync(triggerType);
        var filtered = triggerType == MessageTriggerType.SpecialDay
            ? campaigns.Where(c => string.Equals(c.TriggerKey, NormalizeNullable(triggerKey), StringComparison.OrdinalIgnoreCase)).ToList()
            : campaigns;

        var results = new List<MessageDispatchRunResultDto>(filtered.Count);
        foreach (var campaign in filtered)
        {
            var result = await RunCampaignAsync(campaign.Id, occurrenceDate, adminId);
            results.Add(result);
        }

        return results;
    }

    private async Task<bool> TryQueueSmsAsync(
        MessageCampaign campaign,
        Subject subject,
        DateOnly occurrenceDate,
        string? smsText,
        int adminId)
    {
        if (string.IsNullOrWhiteSpace(smsText))
            return false;

        var exists = await repository.DispatchLogExistsAsync(
            campaign.Id,
            subject.Id,
            occurrenceDate,
            MessageChannelType.Sms);

        if (exists)
            return false;

        try
        {
            await publisher.PublishAsync(
                new SmsSendMessage(subject.PhoneNumber, smsText, subject.Id, adminId),
                Queues.SmsSend);

            var log = new MessageDispatchLog
            {
                CampaignId = campaign.Id,
                SubjectId = subject.Id,
                OccurrenceDate = occurrenceDate,
                ChannelType = MessageChannelType.Sms,
                Status = MessageDispatchStatus.Queued
            };
            log.SetCreated(adminId);
            await repository.AddDispatchLogAsync(log);
            return true;
        }
        catch (Exception ex)
        {
            var log = new MessageDispatchLog
            {
                CampaignId = campaign.Id,
                SubjectId = subject.Id,
                OccurrenceDate = occurrenceDate,
                ChannelType = MessageChannelType.Sms,
                Status = MessageDispatchStatus.Failed,
                ErrorMessage = ex.Message[..Math.Min(ex.Message.Length, 500)]
            };
            log.SetCreated(adminId);
            await repository.AddDispatchLogAsync(log);
            return false;
        }
    }

    private async Task<bool> TryQueuePushAsync(
        MessageCampaign campaign,
        Subject subject,
        DateOnly occurrenceDate,
        string? pushTitle,
        string? pushBody,
        int adminId)
    {
        if (string.IsNullOrWhiteSpace(pushTitle) || string.IsNullOrWhiteSpace(pushBody))
            return false;

        var exists = await repository.DispatchLogExistsAsync(
            campaign.Id,
            subject.Id,
            occurrenceDate,
            MessageChannelType.Push);

        if (exists)
            return false;

        try
        {
            await notificationService.SendPushAsync(subject.Id, pushTitle, pushBody, "automation", adminId);

            var log = new MessageDispatchLog
            {
                CampaignId = campaign.Id,
                SubjectId = subject.Id,
                OccurrenceDate = occurrenceDate,
                ChannelType = MessageChannelType.Push,
                Status = MessageDispatchStatus.Queued
            };
            log.SetCreated(adminId);
            await repository.AddDispatchLogAsync(log);
            return true;
        }
        catch (Exception ex)
        {
            var log = new MessageDispatchLog
            {
                CampaignId = campaign.Id,
                SubjectId = subject.Id,
                OccurrenceDate = occurrenceDate,
                ChannelType = MessageChannelType.Push,
                Status = MessageDispatchStatus.Failed,
                ErrorMessage = ex.Message[..Math.Min(ex.Message.Length, 500)]
            };
            log.SetCreated(adminId);
            await repository.AddDispatchLogAsync(log);
            return false;
        }
    }

    private static MessageTemplateDto MapTemplate(MessageTemplate x)
        => new(
            x.Id,
            x.Name,
            x.ChannelType,
            x.SmsText,
            x.PushTitle,
            x.PushBody,
            x.IsActive,
            x.CreatedAt);

    private static MessageCampaignDto MapCampaign(MessageCampaign x)
        => new(
            x.Id,
            x.Name,
            x.TriggerType,
            x.TriggerKey,
            x.TargetGender,
            x.TemplateId,
            x.Template.Name,
            x.Template.ChannelType,
            x.IsActive,
            x.CreatedAt);

    private static void ValidateTemplate(SaveMessageTemplateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessException("TEMPLATE_NAME_REQUIRED", "Şablon adı zorunludur.");

        if (dto.ChannelType is MessageChannelType.Sms or MessageChannelType.SmsAndPush)
        {
            if (string.IsNullOrWhiteSpace(dto.SmsText))
                throw new BusinessException("SMS_TEXT_REQUIRED", "SMS metni zorunludur.");
        }

        if (dto.ChannelType is MessageChannelType.Push or MessageChannelType.SmsAndPush)
        {
            if (string.IsNullOrWhiteSpace(dto.PushTitle))
                throw new BusinessException("PUSH_TITLE_REQUIRED", "Push başlığı zorunludur.");
            if (string.IsNullOrWhiteSpace(dto.PushBody))
                throw new BusinessException("PUSH_BODY_REQUIRED", "Push içeriği zorunludur.");
        }
    }

    private static void ValidateCampaign(SaveMessageCampaignDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessException("CAMPAIGN_NAME_REQUIRED", "Kampanya adı zorunludur.");

        if (dto.TriggerType == MessageTriggerType.SpecialDay && string.IsNullOrWhiteSpace(dto.TriggerKey))
            throw new BusinessException("TRIGGER_KEY_REQUIRED", "Özel gün kampanyasında tetikleyici kod zorunludur.");
    }

    private static bool MatchGender(Gender gender, MessageTargetGender targetGender)
        => targetGender switch
        {
            MessageTargetGender.Female => gender == Gender.Female,
            MessageTargetGender.Male => gender == Gender.Male,
            _ => gender is Gender.Female or Gender.Male or Gender.Other
        };

    private static bool ShouldSendSms(MessageChannelType channelType)
        => channelType is MessageChannelType.Sms or MessageChannelType.SmsAndPush;

    private static bool ShouldSendPush(MessageChannelType channelType)
        => channelType is MessageChannelType.Push or MessageChannelType.SmsAndPush;

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeTriggerKey(MessageTriggerType triggerType, string? triggerKey)
        => triggerType == MessageTriggerType.SpecialDay
            ? NormalizeNullable(triggerKey)?.ToUpperInvariant()
            : null;

    private static string? Render(string? template, Subject subject, DateOnly occurrenceDate)
    {
        if (string.IsNullOrWhiteSpace(template))
            return template;

        return template
            .Replace("{FirstName}", subject.FirstName, StringComparison.Ordinal)
            .Replace("{LastName}", subject.LastName, StringComparison.Ordinal)
            .Replace("{FullName}", subject.FullName, StringComparison.Ordinal)
            .Replace("{Date}", occurrenceDate.ToString("dd.MM.yyyy"), StringComparison.Ordinal);
    }
}
