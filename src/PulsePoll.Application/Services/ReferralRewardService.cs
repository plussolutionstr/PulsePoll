using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class ReferralRewardService(
    ISubjectRepository subjectRepository,
    ISubjectAppActivityRepository activityRepository,
    IProjectRepository projectRepository,
    IWalletRepository walletRepository,
    IReferralRewardConfigService referralRewardConfigService,
    IRewardUnitConfigService rewardUnitConfigService,
    ILogger<ReferralRewardService> logger) : IReferralRewardService
{
    public async Task TryGrantAsync(int referredSubjectId, ReferralRewardTriggerType triggerType, int actorId)
    {
        _ = await TryGrantCoreAsync(referredSubjectId, triggerType, actorId);
    }

    public async Task<int> ReconcilePendingAsync(int actorId)
    {
        var config = await referralRewardConfigService.GetAsync();
        if (!config.IsActive || config.RewardAmount <= 0)
            return 0;

        var pendingReferrals = await subjectRepository.GetPendingRewardReferralsAsync();
        if (pendingReferrals.Count == 0)
            return 0;

        var eligibleSubjectIds = await GetEligibleSubjectIdsAsync(pendingReferrals, config);
        var grantedCount = 0;

        foreach (var subjectId in eligibleSubjectIds)
        {
            if (await TryGrantCoreAsync(subjectId, config.TriggerType, actorId, config))
                grantedCount++;
        }

        logger.LogInformation(
            "Referral reward reconciliation completed: Trigger={TriggerType} PendingReferralCount={PendingCount} GrantedCount={GrantedCount}",
            config.TriggerType,
            pendingReferrals.Count,
            grantedCount);

        return grantedCount;
    }

    private async Task<bool> TryGrantCoreAsync(
        int referredSubjectId,
        ReferralRewardTriggerType triggerType,
        int actorId,
        ReferralRewardConfigDto? configOverride = null)
    {
        var referral = await subjectRepository.GetReferralByReferredSubjectIdAsync(referredSubjectId);
        if (referral is null || referral.CommissionEarned.HasValue)
            return false;

        var config = configOverride ?? await referralRewardConfigService.GetAsync();
        if (!config.IsActive || config.RewardAmount <= 0 || config.TriggerType != triggerType)
            return false;

        if (triggerType == ReferralRewardTriggerType.ActiveDaysReached)
        {
            var activeDays = await GetActiveDayCountAsync(referral);
            if (activeDays < config.ActiveDaysThreshold)
                return false;
        }

        var referrerWallet = await walletRepository.GetBySubjectIdAsync(referral.ReferrerId);
        if (referrerWallet is null)
        {
            logger.LogWarning(
                "Referral ödülü atlandı: Referrer wallet bulunamadı. ReferralId={ReferralId} ReferrerId={ReferrerId}",
                referral.Id, referral.ReferrerId);
            return false;
        }

        var rewardUnit = await rewardUnitConfigService.GetAsync();
        var rewardAmount = config.RewardAmount;
        var rewardAmountTry = decimal.Round(rewardAmount * rewardUnit.TryMultiplier, 2, MidpointRounding.AwayFromZero);
        var referenceId = $"referral-reward:{referral.Id}";
        var existingTx = await walletRepository.GetTransactionByReferenceAsync(referrerWallet.Id, referenceId);

        if (existingTx is null)
        {
            referrerWallet.Balance += rewardAmount;
            referrerWallet.TotalEarned += rewardAmount;
            referrerWallet.SetUpdated(actorId);
            await walletRepository.UpdateAsync(referrerWallet);

            var tx = new WalletTransaction
            {
                WalletId = referrerWallet.Id,
                Amount = rewardAmount,
                Type = WalletTransactionType.Credit,
                ReferenceId = referenceId,
                Description = $"Referans ödülü (Denek #{referredSubjectId})"
            };
            tx.SetCreated(actorId);
            await walletRepository.AddTransactionAsync(tx);
        }

        referral.CommissionEarned = rewardAmount;
        referral.CommissionAmountTry = rewardAmountTry;
        referral.CommissionUnitCode = rewardUnit.UnitCode;
        referral.CommissionUnitLabel = rewardUnit.UnitLabel;
        referral.CommissionUnitTryMultiplier = rewardUnit.TryMultiplier;
        referral.CommissionGrantedAt = TurkeyTime.Now;
        referral.SetUpdated(actorId);
        await subjectRepository.UpdateReferralAsync(referral);

        logger.LogInformation(
            "Referral ödülü verildi: ReferralId={ReferralId} ReferrerId={ReferrerId} ReferredSubjectId={ReferredSubjectId} Trigger={TriggerType} Amount={Amount} Unit={Unit}",
            referral.Id, referral.ReferrerId, referredSubjectId, triggerType, rewardAmount, rewardUnit.UnitLabel);

        return true;
    }

    private async Task<List<int>> GetEligibleSubjectIdsAsync(
        List<Referral> pendingReferrals,
        ReferralRewardConfigDto config)
    {
        return config.TriggerType switch
        {
            ReferralRewardTriggerType.RegistrationCompleted
                => pendingReferrals.Select(x => x.ReferredSubjectId).ToList(),
            ReferralRewardTriggerType.AccountApproved
                => pendingReferrals
                    .Where(x => x.ReferredSubject.Status == ApprovalStatus.Approved)
                    .Select(x => x.ReferredSubjectId)
                    .ToList(),
            ReferralRewardTriggerType.FirstSurveyCompleted
                => await GetCompletedSurveySubjectIdsAsync(pendingReferrals),
            ReferralRewardTriggerType.FirstRewardApproved
                => await GetApprovedRewardSubjectIdsAsync(pendingReferrals),
            ReferralRewardTriggerType.ActiveDaysReached
                => await GetActiveDayEligibleSubjectIdsAsync(pendingReferrals, config.ActiveDaysThreshold),
            _ => []
        };
    }

    private async Task<List<int>> GetCompletedSurveySubjectIdsAsync(List<Referral> pendingReferrals)
    {
        var subjectIds = pendingReferrals.Select(x => x.ReferredSubjectId).ToList();
        var completedIds = await projectRepository.GetSubjectIdsWithCompletedSurveyAsync(subjectIds);
        var completedSet = completedIds.ToHashSet();

        return pendingReferrals
            .Select(x => x.ReferredSubjectId)
            .Where(completedSet.Contains)
            .ToList();
    }

    private async Task<List<int>> GetApprovedRewardSubjectIdsAsync(List<Referral> pendingReferrals)
    {
        var subjectIds = pendingReferrals.Select(x => x.ReferredSubjectId).ToList();
        var approvedRewardIds = await projectRepository.GetSubjectIdsWithApprovedRewardAsync(subjectIds);
        var approvedSet = approvedRewardIds.ToHashSet();

        return pendingReferrals
            .Select(x => x.ReferredSubjectId)
            .Where(approvedSet.Contains)
            .ToList();
    }

    private async Task<List<int>> GetActiveDayEligibleSubjectIdsAsync(List<Referral> pendingReferrals, int threshold)
    {
        var eligible = new List<int>();
        foreach (var referral in pendingReferrals)
        {
            var activeDays = await GetActiveDayCountAsync(referral);
            if (activeDays >= threshold)
                eligible.Add(referral.ReferredSubjectId);
        }

        return eligible;
    }

    private async Task<int> GetActiveDayCountAsync(Referral referral)
    {
        var statsMap = await activityRepository.GetStatsBySubjectIdsAsync(
            [referral.ReferredSubjectId],
            referral.ReferredAt);

        return statsMap.GetValueOrDefault(referral.ReferredSubjectId)?.ActiveDays30 ?? 0;
    }
}
