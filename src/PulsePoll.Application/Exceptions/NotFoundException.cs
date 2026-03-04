namespace PulsePoll.Application.Exceptions;

public class NotFoundException(string resourceName)
    : Exception($"{resourceName} bulunamadı.")
{
    public string ResourceName { get; } = resourceName;
}
