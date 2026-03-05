using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _api;

    public ForgotPasswordViewModel(IPulsePollApiClient api)
    {
        _api = api;
    }

    // Step: 0=Phone, 1=OTP+NewPassword
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPhoneStep))]
    [NotifyPropertyChangedFor(nameof(IsResetStep))]
    private int _currentStep;

    public bool IsPhoneStep => CurrentStep == 0;
    public bool IsResetStep => CurrentStep == 1;

    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _otpCode = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _newPasswordConfirm = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [RelayCommand]
    private async Task SendOtpAsync()
    {
        ErrorMessage = string.Empty;
        var phone = PhoneNumber.Trim();
        if (phone.Length < 10)
        {
            ErrorMessage = "Geçerli bir telefon numarası girin.";
            return;
        }

        IsBusy = true;
        try
        {
            await _api.SendPasswordResetOtpAsync(phone);
            CurrentStep = 1;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length < 4)
        {
            ErrorMessage = "Doğrulama kodunu girin.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
        {
            ErrorMessage = "Parola en az 8 karakter olmalı.";
            return;
        }
        if (NewPassword != NewPasswordConfirm)
        {
            ErrorMessage = "Parolalar eşleşmiyor.";
            return;
        }

        IsBusy = true;
        try
        {
            await _api.ResetPasswordAsync(PhoneNumber.Trim(), OtpCode.Trim(), NewPassword);

            await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
                "Başarılı",
                "Parolanız değiştirildi. Giriş yapabilirsiniz.",
                "Tamam");

            await Application.Current!.Windows[0].Page!.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        ErrorMessage = string.Empty;
        if (CurrentStep > 0)
            CurrentStep--;
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Application.Current!.Windows[0].Page!.Navigation.PopAsync();
    }
}
