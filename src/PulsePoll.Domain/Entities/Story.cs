namespace PulsePoll.Domain.Entities;

public class Story : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int? MediaAssetId { get; set; }
    public string? LinkUrl { get; set; }
    public string? BrandName { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public MediaAsset? MediaAsset { get; set; }
}
