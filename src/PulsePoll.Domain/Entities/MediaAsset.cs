using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class MediaAsset : EntityBase
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ObjectKey { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public ICollection<Project> Projects { get; set; } = [];
    public ICollection<Story> Stories { get; set; } = [];
    public ICollection<Story> StoryImages { get; set; } = [];
    public ICollection<News> News { get; set; } = [];
    public ICollection<Bank> BankThumbnails { get; set; } = [];
    public ICollection<Bank> BankLogos { get; set; } = [];
}
