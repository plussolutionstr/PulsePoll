using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPushNotificationService _pushService;

    public LoginViewModel(IPulsePollApiClient apiClient, IServiceProvider serviceProvider, IPushNotificationService pushService)
    {
        _apiClient = apiClient;
        _serviceProvider = serviceProvider;
        _pushService = pushService;
    }

    [ObservableProperty] private string _phoneNumber = string.Empty;
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

        if (string.IsNullOrWhiteSpace(PhoneNumber) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Telefon numarası ve parola gerekli.";
            return;
        }

        IsBusy = true;
        try
        {
            var success = await _apiClient.LoginAsync(PhoneNumber.Trim(), Password);
            if (success)
            {
                _ = _pushService.RegisterAsync();
                Application.Current!.Windows[0].Page = _serviceProvider.GetRequiredService<AppShell>();
            }
            else
            {
                ErrorMessage = "Telefon numarası veya parola hatalı.";
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

    [RelayCommand]
    private async Task GoToForgotPasswordAsync()
    {
        var page = _serviceProvider.GetRequiredService<Views.ForgotPasswordPage>();
        await Application.Current!.Windows[0].Page!.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        var page = _serviceProvider.GetRequiredService<Views.RegisterPage>();
        await Application.Current!.Windows[0].Page!.Navigation.PushAsync(page);
    }
}
