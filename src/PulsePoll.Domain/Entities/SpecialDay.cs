using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class SpecialDay : EntityBase
{
    public string EventCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public SpecialDayCategory Category { get; set; } = SpecialDayCategory.Special;
    public string Source { get; set; } = "system";
    public bool IsActive { get; set; } = true;
}
