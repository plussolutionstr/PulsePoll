using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum AssignmentStatus
{
    [Description("Başlamadı")]
    NotStarted = 0,

    [Description("Tamamlandı")]
    Completed = 1,

    [Description("Kısmi")]
    Partial = 2,

    [Description("Diskalifiye")]
    Disqualify = 3,

    [Description("Kota Dolu")]
    QuotaFull = 4,

    [Description("Elenmiş")]
    ScreenOut = 5,

    [Description("Bilinmiyor")]
    Unknown = 6
}
