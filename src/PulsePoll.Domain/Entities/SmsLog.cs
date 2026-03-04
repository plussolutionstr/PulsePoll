using System.ComponentModel.DataAnnotations;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class SmsLog : EntityBase
{
    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(1600)]
    public string Message { get; set; } = string.Empty;

    public int? SubjectId { get; set; }

    public SmsSource Source { get; set; }

    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Sent;

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    // Navigation
    public Subject? Subject { get; set; }
}
