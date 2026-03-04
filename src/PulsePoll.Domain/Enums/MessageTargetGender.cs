using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum MessageTargetGender
{
    [Description("Kadın + Erkek")] All = 0,
    [Description("Kadın")] Female = 1,
    [Description("Erkek")] Male = 2
}
