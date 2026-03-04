namespace PulsePoll.Domain.Entities;

public class RewardUnitConfig : EntityBase
{
    public string UnitCode { get; set; } = "TRY";
    public string UnitLabel { get; set; } = "TL";
    public decimal TryMultiplier { get; set; } = 1m;
}
