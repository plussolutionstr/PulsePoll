using System.ComponentModel.DataAnnotations;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class Notification : EntityBase
{
    public int SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    public Subject Subject { get; set; } = null!;
}
