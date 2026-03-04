using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum ProjectStatus
{
    [Description("Taslak")]
    Draft      = 0,
    
    [Description("Aktif")]
    Active     = 1,
    
    [Description("Duraklatıldı")]
    Paused     = 2,
    
    [Description("Tamamlandı")]
    Completed  = 3,
    
    [Description("İptal Edildi")]
    Cancelled  = 4
}
