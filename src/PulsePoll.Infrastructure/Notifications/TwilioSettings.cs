namespace PulsePoll.Infrastructure.Notifications;

public class TwilioSettings
{
    public const string SectionName = "Twilio";

    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string VerifyServiceSid { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}
