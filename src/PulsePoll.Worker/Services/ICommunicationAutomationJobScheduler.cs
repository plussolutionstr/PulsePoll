namespace PulsePoll.Worker.Services;

public interface ICommunicationAutomationJobScheduler
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
