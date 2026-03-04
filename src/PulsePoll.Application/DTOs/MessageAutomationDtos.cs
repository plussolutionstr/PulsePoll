using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record MessageTemplateDto(
    int Id,
    string Name,
    MessageChannelType ChannelType,
    string? SmsText,
    string? PushTitle,
    string? PushBody,
    bool IsActive,
    DateTime CreatedAt);

public record SaveMessageTemplateDto(
    int? Id,
    string Name,
    MessageChannelType ChannelType,
    string? SmsText,
    string? PushTitle,
    string? PushBody,
    bool IsActive);

public record MessageCampaignDto(
    int Id,
    string Name,
    MessageTriggerType TriggerType,
    string? TriggerKey,
    MessageTargetGender TargetGender,
    int TemplateId,
    string TemplateName,
    MessageChannelType ChannelType,
    bool IsActive,
    DateTime CreatedAt);

public record SaveMessageCampaignDto(
    int? Id,
    string Name,
    MessageTriggerType TriggerType,
    string? TriggerKey,
    MessageTargetGender TargetGender,
    int TemplateId,
    bool IsActive);

public record MessageDispatchRunResultDto(
    int CampaignId,
    DateOnly OccurrenceDate,
    int TargetCount,
    int QueuedCount,
    int SkippedCount);
