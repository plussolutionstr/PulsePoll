namespace PulsePoll.Domain.Entities;

public class NotificationDistributionConfig : EntityBase
{
    public int HourlyLimit { get; set; } = 300;
}
