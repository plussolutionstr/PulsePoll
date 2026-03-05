using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class Bank
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Code { get; set; }

    public bool IsActive { get; set; } = true;

    public int? ThumbnailMediaAssetId { get; set; }
    public int? LogoMediaAssetId { get; set; }

    public int SortOrder { get; set; }

    public MediaAsset? ThumbnailMediaAsset { get; set; }
    public MediaAsset? LogoMediaAsset { get; set; }
}
