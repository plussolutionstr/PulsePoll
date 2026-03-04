using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum Gender
{
    [Description("Erkek")] Male = 1,
    [Description("Kadın")] Female = 2,
    [Description("Diğer")] Other = 3
}
