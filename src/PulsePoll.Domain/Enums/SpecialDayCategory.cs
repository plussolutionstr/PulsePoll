using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum SpecialDayCategory
{
    [Description("Milli")] National = 1,
    [Description("Dini")] Religious = 2,
    [Description("Özel")] Special = 3
}
