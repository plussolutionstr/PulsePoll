using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class MessageDispatchLog : EntityBase
{
    public int CampaignId { get; set; }
    public int SubjectId { get; set; }
    public DateOnly OccurrenceDate { get; set; }
    public MessageChannelType ChannelType { get; set; }
    public MessageDispatchStatus Status { get; set; } = MessageDispatchStatus.Queued;
    public string? ErrorMessage { get; set; }

    public MessageCampaign Campaign { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
