namespace PulsePoll.Domain.Events;

public record SubjectRegistered(
    int SubjectId,
    string Email,
    string FirstName,
    string LastName,
    DateTime RegisteredAt);
