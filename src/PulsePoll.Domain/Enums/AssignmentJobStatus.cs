using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum AssignmentJobStatus
{
    [Description("Beklemede")]  Pending    = 0,
    [Description("İşleniyor")]  Processing = 1,
    [Description("Tamamlandı")] Completed  = 2,
    [Description("Başarısız")]  Failed     = 3
}
