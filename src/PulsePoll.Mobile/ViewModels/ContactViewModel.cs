using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class ContactViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _api;

    public ContactViewModel(IPulsePollApiClient api)
    {
        _api = api;
    }

    [ObservableProperty] private string _contactTitle = "";
    [ObservableProperty] private string _contactBody = "";
    [ObservableProperty] private string _contactEmail = "";
    [ObservableProperty] private string _contactPhone = "";
    [ObservableProperty] private string _contactWhatsapp = "";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = "";

    public bool HasEmail => !string.IsNullOrEmpty(ContactEmail);
    public bool HasPhone => !string.IsNullOrEmpty(ContactPhone);
    public bool HasWhatsapp => !string.IsNullOrEmpty(ContactWhatsapp);

    [RelayCommand]
    private async Task LoadContentAsync()
    {
        IsLoading = true;
        try
        {
            var content = await _api.GetAppContentAsync();
            if (content is null) return;

            ContactTitle = content.ContactTitle;
            ContactBody = content.ContactBody;
            ContactEmail = content.ContactEmail ?? "";
            ContactPhone = content.ContactPhone ?? "";
            ContactWhatsapp = content.ContactWhatsapp ?? "";

            OnPropertyChanged(nameof(HasEmail));
            OnPropertyChanged(nameof(HasPhone));
            OnPropertyChanged(nameof(HasWhatsapp));
        }
        catch
        {
            ContactTitle = "İletişim";
            ContactBody = "Şu an iletişim bilgileri yüklenemiyor.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenEmailAsync()
    {
        if (!HasEmail) return;
        try
        {
            if (Email.Default.IsComposeSupported)
            {
                await Email.Default.ComposeAsync(new EmailMessage
                {
                    To = [ContactEmail],
                    Subject = "PulsePoll İletişim"
                });
            }
            else
            {
                await Clipboard.Default.SetTextAsync(ContactEmail);
                await ShowStatusAsync("E-posta adresi kopyalandı!");
            }
        }
        catch
        {
            await Clipboard.Default.SetTextAsync(ContactEmail);
            await ShowStatusAsync("E-posta adresi kopyalandı!");
        }
    }

    [RelayCommand]
    private async Task OpenPhoneAsync()
    {
        if (!HasPhone) return;
        try
        {
            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open(ContactPhone);
            }
            else
            {
                await Clipboard.Default.SetTextAsync(ContactPhone);
                await ShowStatusAsync("Telefon numarası kopyalandı!");
            }
        }
        catch
        {
            await Clipboard.Default.SetTextAsync(ContactPhone);
            await ShowStatusAsync("Telefon numarası kopyalandı!");
        }
    }

    [RelayCommand]
    private async Task OpenWhatsappAsync()
    {
        if (!HasWhatsapp) return;
        try
        {
            var number = ContactWhatsapp.Replace("+", "").Replace(" ", "");
            var opened = await Launcher.Default.TryOpenAsync($"https://wa.me/{number}");
            if (!opened)
            {
                await Clipboard.Default.SetTextAsync(ContactWhatsapp);
                await ShowStatusAsync("WhatsApp numarası kopyalandı!");
            }
        }
        catch
        {
            await Clipboard.Default.SetTextAsync(ContactWhatsapp);
            await ShowStatusAsync("WhatsApp numarası kopyalandı!");
        }
    }

    private async Task ShowStatusAsync(string message)
    {
        StatusMessage = message;
        await Task.Delay(2500);
        StatusMessage = "";
    }
}
