namespace PulsePoll.Worker.Services;

public interface IReferralRewardJobScheduler
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
