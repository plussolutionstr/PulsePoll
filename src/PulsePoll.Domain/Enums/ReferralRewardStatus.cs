using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum ReferralRewardStatus
{
    [Description("Onay Bekleniyor")] PendingApproval = 1,
    [Description("Tetikleyici Bekleniyor")] WaitingTrigger = 2,
    [Description("Ödül Verildi")] RewardGranted = 3
}
