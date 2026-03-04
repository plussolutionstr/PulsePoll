using DevExpress.Blazor;

namespace PulsePoll.Admin.Services;

public class AppToastService(IToastNotificationService toastService) : IAppToastService
{
    public void Success(string message, string? title = null) =>
        toastService.ShowToast(new ToastOptions
        {
            Title        = title ?? "Başarılı",
            Text         = message,
            RenderStyle  = ToastRenderStyle.Success,
            IconCssClass = "bi bi-check-circle-fill"
        });

    public void Error(string message, string? title = null) =>
        toastService.ShowToast(new ToastOptions
        {
            Title        = title ?? "Hata",
            Text         = message,
            RenderStyle  = ToastRenderStyle.Danger,
            IconCssClass = "bi bi-x-circle-fill",
            DisplayTime  = TimeSpan.FromSeconds(8)
        });

    public void Warning(string message, string? title = null) =>
        toastService.ShowToast(new ToastOptions
        {
            Title        = title ?? "Uyarı",
            Text         = message,
            RenderStyle  = ToastRenderStyle.Warning,
            IconCssClass = "bi bi-exclamation-triangle-fill"
        });

    public void Info(string message, string? title = null) =>
        toastService.ShowToast(new ToastOptions
        {
            Title        = title ?? "Bilgi",
            Text         = message,
            RenderStyle  = ToastRenderStyle.Info,
            IconCssClass = "bi bi-info-circle-fill"
        });
}
