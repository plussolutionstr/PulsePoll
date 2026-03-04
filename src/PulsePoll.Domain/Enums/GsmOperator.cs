using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum GsmOperator
{
    [Description("Turkcell")] Turkcell = 1,
    [Description("Vodafone")] Vodafone = 2,
    [Description("Türk Telekom")] TurkTelekom = 3,
    [Description("Diğer")] Other = 4
}
