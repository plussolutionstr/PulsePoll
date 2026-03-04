using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly IServiceProvider _serviceProvider;

    public LoginViewModel(IPulsePollApiClient apiClient, IServiceProvider serviceProvider)
    {
        _apiClient = apiClient;
        _serviceProvider = serviceProvider;
    }

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty] private bool _isBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "E-posta ve şifre gerekli.";
            return;
        }

        IsBusy = true;
        try
        {
            var success = await _apiClient.LoginAsync(Email.Trim(), Password);
            if (success)
            {
                Application.Current!.Windows[0].Page = _serviceProvider.GetRequiredService<AppShell>();
            }
            else
            {
                ErrorMessage = "E-posta veya şifre hatalı.";
            }
        }
        catch
        {
            ErrorMessage = "Sunucuya bağlanılamadı.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
