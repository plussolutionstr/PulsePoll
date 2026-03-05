using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class ConnectionErrorViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;

    public ConnectionErrorViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty] private bool _isRetrying;

    [RelayCommand]
    private async Task RetryAsync()
    {
        if (IsRetrying)
            return;

        IsRetrying = true;
        try
        {
            await _apiClient.PingAsync();
            await Shell.Current.GoToAsync("//home");
        }
        catch
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlertAsync("Bağlantı Hatası", "Sunucuya hâlâ ulaşılamıyor. Lütfen tekrar deneyin.", "Tamam");
        }
        finally
        {
            IsRetrying = false;
        }
    }
}
