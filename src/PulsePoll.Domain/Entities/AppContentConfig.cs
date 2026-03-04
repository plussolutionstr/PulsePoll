namespace PulsePoll.Domain.Entities;

public class AppContentConfig : EntityBase
{
    public string KvkkText { get; set; } = string.Empty;
    public string ContactTitle { get; set; } = "Bize Ulaşın";
    public string ContactBody { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactWhatsapp { get; set; }
}
