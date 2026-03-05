namespace PulsePoll.Domain.Entities;

public class StoryView : EntityBase
{
    public int SubjectId { get; set; }
    public int StoryId { get; set; }
    public DateTime SeenAt { get; set; } = DateTime.UtcNow;

    public Subject Subject { get; set; } = null!;
    public Story Story { get; set; } = null!;
}
