using Microsoft.Extensions.Logging;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class AffiliateRewardService(
    ISubjectRepository subjectRepository,
    ISubjectAppActivityRepository activityRepository,
    IExternalAffiliateRepository affiliateRepository,
    IReferralRewardConfigService referralRewardConfigService,
    ILogger<AffiliateRewardService> logger) : IAffiliateRewardService
{
    public async Task TryGrantAsync(int subjectId, ReferralRewardTriggerType triggerType, int actorId)
    {
        var subject = await subjectRepository.GetByIdAsync(subjectId);
        if (subject?.ExternalAffiliateId is null)
            return;

        var affiliate = await affiliateRepository.GetByIdAsync(subject.ExternalAffiliateId.Value);
        if (affiliate is null || !affiliate.IsActive)
            return;

        await TryGrantCoreAsync(subject, affiliate, triggerType, actorId);
    }

    public async Task<int> ReconcilePendingAsync(int actorId)
    {
        // Toplu yükleme — N+1 yok
        var pendingSubjects = await affiliateRepository.GetPendingCommissionSubjectsWithAffiliateAsync();
        if (pendingSubjects.Count == 0)
            return 0;

        var config = await referralRewardConfigService.GetAsync();
        if (!config.IsActive)
            return 0;

        var grantedCount = 0;

        foreach (var subject in pendingSubjects)
        {
            var affiliate = subject.ExternalAffiliate!;
            if (await TryGrantCoreAsync(subject, affiliate, config.TriggerType, actorId, config))
                grantedCount++;
        }

        logger.LogInformation(
            "Affiliate reward reconciliation completed: PendingCount={PendingCount} GrantedCount={GrantedCount}",
            pendingSubjects.Count, grantedCount);

        return grantedCount;
    }

    private async Task<bool> TryGrantCoreAsync(
        Subject subject,
        ExternalAffiliate affiliate,
        ReferralRewardTriggerType triggerType,
        int actorId,
        DTOs.ReferralRewardConfigDto? configOverride = null)
    {
        var config = configOverride ?? await referralRewardConfigService.GetAsync();
        if (!config.IsActive)
            return false;

        var effectiveTrigger = config.TriggerType;
        var effectiveAmount = affiliate.CommissionAmount ?? config.RewardAmount;

        if (effectiveTrigger != triggerType || effectiveAmount < 0)
            return false;

        if (triggerType == ReferralRewardTriggerType.ActiveDaysReached)
        {
            var activeDays = await GetActiveDayCountAsync(subject.Id, subject.CreatedAt);
            if (activeDays < config.ActiveDaysThreshold)
                return false;
        }

        // Idempotency kontrolü
        var referenceId = $"affiliate-commission:{affiliate.Id}:{subject.Id}";
        var existingTx = await affiliateRepository.GetTransactionByReferenceAsync(affiliate.Id, referenceId);
        if (existingTx is not null)
            return false;

        // Bakiye + hareket tek transaction içinde
        affiliate.Balance += effectiveAmount;
        affiliate.TotalEarned += effectiveAmount;
        affiliate.SetUpdated(actorId);

        var tx = new AffiliateTransaction
        {
            ExternalAffiliateId = affiliate.Id,
            Type = AffiliateTransactionType.Commission,
            Amount = effectiveAmount,
            SubjectId = subject.Id,
            ReferenceId = referenceId,
            Description = $"Komisyon (Denek #{subject.Id})"
        };
        tx.SetCreated(actorId);

        var granted = await affiliateRepository.GrantCommissionAsync(affiliate, tx);
        if (!granted)
        {
            logger.LogWarning(
                "Affiliate komisyonu zaten verilmiş (idempotent): AffiliateId={AffiliateId} SubjectId={SubjectId}",
                affiliate.Id, subject.Id);
            return false;
        }

        logger.LogInformation(
            "Affiliate komisyonu verildi: AffiliateId={AffiliateId} SubjectId={SubjectId} Trigger={TriggerType} Amount={Amount}",
            affiliate.Id, subject.Id, triggerType, effectiveAmount);

        return true;
    }

    private async Task<int> GetActiveDayCountAsync(int subjectId, DateTime referenceDate)
    {
        var statsMap = await activityRepository.GetStatsBySubjectIdsAsync(
            [subjectId], referenceDate);

        return statsMap.GetValueOrDefault(subjectId)?.ActiveDays30 ?? 0;
    }
}
