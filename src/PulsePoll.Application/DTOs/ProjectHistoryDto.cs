using PulsePoll.Application.Extensions;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record ProjectHistoryDto(
    int ProjectId,
    string ProjectName,
    string CustomerShortName,
    AssignmentStatus Status,
    DateTime AssignedAt,
    DateTime? CompletedAt,
    int DurationMinutes,
    decimal EarnedAmount,
    decimal RewardAmount,
    decimal ConsolationRewardAmount,
    string RewardUnitLabel);

public record ReferredSubjectDto(
    int SubjectId,
    string FullName,
    string ReferralCode,
    ApprovalStatus Status,
    DateTime RegisteredAt,
    ReferralRewardStatus RewardStatus,
    decimal? CommissionEarned,
    string? CommissionUnitLabel,
    decimal? CommissionAmountTry,
    DateTime? CommissionGrantedAt)
{
    public string RewardStatusLabel => RewardStatus.GetDescription();
}

public record ReferralSummaryDto(
    int TotalReferrals,
    int ApprovedReferrals,
    decimal TotalCommissionEarned,
    decimal TotalCommissionTry);
