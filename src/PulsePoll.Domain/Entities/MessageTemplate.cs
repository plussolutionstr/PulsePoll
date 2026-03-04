using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class MessageTemplate : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public MessageChannelType ChannelType { get; set; } = MessageChannelType.Push;
    public string? SmsText { get; set; }
    public string? PushTitle { get; set; }
    public string? PushBody { get; set; }
    public bool IsActive { get; set; } = true;
}
