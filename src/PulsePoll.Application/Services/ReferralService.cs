using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Application.Services;

public class ReferralService(ISubjectRepository subjectRepository)
{
    public async Task<List<ReferredSubjectDto>> GetReferredSubjectsAsync(int referrerId)
    {
        var referrals = await subjectRepository.GetReferralsAsync(referrerId);
        return referrals.Select(r => new ReferredSubjectDto(
            r.ReferredSubject.Id,
            r.ReferredSubject.FullName,
            r.ReferredSubject.ReferralCode,
            r.ReferredSubject.Status,
            r.ReferredSubject.CreatedAt,
            ResolveRewardStatus(r),
            r.CommissionEarned,
            r.CommissionUnitLabel,
            r.CommissionAmountTry,
            r.CommissionGrantedAt
        )).ToList();
    }

    public async Task<ReferralSummaryDto> GetReferralSummaryAsync(int referrerId)
    {
        var count = await subjectRepository.GetReferralCountAsync(referrerId);
        var commission = await subjectRepository.GetReferralCommissionAsync(referrerId);
        var commissionTry = await subjectRepository.GetReferralCommissionTryAsync(referrerId);
        var referrals = await subjectRepository.GetReferralsAsync(referrerId);
        var approved = referrals.Count(r => r.ReferredSubject.Status == Domain.Enums.ApprovalStatus.Approved);

        return new ReferralSummaryDto(count, approved, commission, commissionTry);
    }

    private static Domain.Enums.ReferralRewardStatus ResolveRewardStatus(Domain.Entities.Referral referral)
    {
        if (referral.CommissionEarned.HasValue)
            return Domain.Enums.ReferralRewardStatus.RewardGranted;

        if (referral.ReferredSubject.Status == Domain.Enums.ApprovalStatus.Approved)
            return Domain.Enums.ReferralRewardStatus.WaitingTrigger;

        return Domain.Enums.ReferralRewardStatus.PendingApproval;
    }
}
