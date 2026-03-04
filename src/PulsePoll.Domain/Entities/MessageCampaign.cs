using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class MessageCampaign : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public MessageTriggerType TriggerType { get; set; } = MessageTriggerType.SpecialDay;
    public string? TriggerKey { get; set; }
    public MessageTargetGender TargetGender { get; set; } = MessageTargetGender.All;
    public int TemplateId { get; set; }
    public bool IsActive { get; set; } = true;

    public MessageTemplate Template { get; set; } = null!;
}
