using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IAffiliateRewardService
{
    Task TryGrantAsync(int subjectId, ReferralRewardTriggerType triggerType, int actorId);
    Task<int> ReconcilePendingAsync(int actorId);
}
