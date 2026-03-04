using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum SmsSource
{
    [Description("OTP")] Otp = 0,
    [Description("Admin")] AdminBulk = 1,
    [Description("Sistem")] System = 2
}
