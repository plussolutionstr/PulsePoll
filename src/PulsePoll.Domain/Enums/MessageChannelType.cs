using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum MessageChannelType
{
    [Description("SMS")] Sms = 1,
    [Description("Push")] Push = 2,
    [Description("SMS + Push")] SmsAndPush = 3
}
