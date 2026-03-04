using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class PaymentSetting : EntityBase
{
    [Required, MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }
}
