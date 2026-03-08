namespace PulsePoll.Domain.Entities;

public class RegistrationConfig : EntityBase
{
    public bool AutoApproveNewSubjects { get; set; } = true;
}
