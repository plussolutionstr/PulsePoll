using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IReferralRewardService
{
    Task TryGrantAsync(int referredSubjectId, ReferralRewardTriggerType triggerType, int actorId);
}
