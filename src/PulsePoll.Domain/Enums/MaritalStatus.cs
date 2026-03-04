using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum MaritalStatus
{
    [Description("Bekar")] Single = 1,
    [Description("Evli")] Married = 2,
    [Description("Boşanmış")] Divorced = 3,
    [Description("Dul")] Widowed = 4
}
