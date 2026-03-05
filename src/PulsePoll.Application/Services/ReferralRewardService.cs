using Microsoft.Extensions.Logging;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class ReferralRewardService(
    ISubjectRepository subjectRepository,
    ISubjectAppActivityRepository activityRepository,
    IWalletRepository walletRepository,
    IReferralRewardConfigService referralRewardConfigService,
    IRewardUnitConfigService rewardUnitConfigService,
    ILogger<ReferralRewardService> logger) : IReferralRewardService
{
    public async Task TryGrantAsync(int referredSubjectId, ReferralRewardTriggerType triggerType, int actorId)
    {
        var referral = await subjectRepository.GetReferralByReferredSubjectIdAsync(referredSubjectId);
        if (referral is null || referral.CommissionEarned.HasValue)
            return;

        var config = await referralRewardConfigService.GetAsync();
        if (!config.IsActive || config.RewardAmount <= 0 || config.TriggerType != triggerType)
            return;

        if (triggerType == ReferralRewardTriggerType.ActiveDaysReached)
        {
            var activeDays = await GetActiveDayCountAsync(referral);
            if (activeDays < config.ActiveDaysThreshold)
                return;
        }

        var referrerWallet = await walletRepository.GetBySubjectIdAsync(referral.ReferrerId);
        if (referrerWallet is null)
        {
            logger.LogWarning(
                "Referral ödülü atlandı: Referrer wallet bulunamadı. ReferralId={ReferralId} ReferrerId={ReferrerId}",
                referral.Id, referral.ReferrerId);
            return;
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
    }

    private async Task<int> GetActiveDayCountAsync(Referral referral)
    {
        var statsMap = await activityRepository.GetStatsBySubjectIdsAsync(
            [referral.ReferredSubjectId],
            referral.ReferredAt);

        return statsMap.GetValueOrDefault(referral.ReferredSubjectId)?.ActiveDays30 ?? 0;
    }
}
