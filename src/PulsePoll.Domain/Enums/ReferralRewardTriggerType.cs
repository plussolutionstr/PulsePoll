using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum ReferralRewardTriggerType
{
    [Description("Kayıt Tamamlandı")] RegistrationCompleted = 1,
    [Description("Hesap Onaylandı")] AccountApproved = 2,
    [Description("İlk Anketi Tamamladı")] FirstSurveyCompleted = 3,
    [Description("İlk Ödülü Onaylandı")] FirstRewardApproved = 4,
    [Description("X Gün Aktif Kaldı")] ActiveDaysReached = 5
}
