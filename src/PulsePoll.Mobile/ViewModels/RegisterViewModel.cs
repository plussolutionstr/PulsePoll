using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _api;
    private readonly IServiceProvider _serviceProvider;

    public RegisterViewModel(IPulsePollApiClient api, IServiceProvider serviceProvider)
    {
        _api = api;
        _serviceProvider = serviceProvider;
    }

    // Step tracking: 0=Phone, 1=OTP, 2=PersonalInfo, 3=KVKK
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPhoneStep))]
    [NotifyPropertyChangedFor(nameof(IsOtpStep))]
    [NotifyPropertyChangedFor(nameof(IsInfoStep))]
    [NotifyPropertyChangedFor(nameof(IsKvkkStep))]
    [NotifyPropertyChangedFor(nameof(StepTitle))]
    private int _currentStep;

    public bool IsPhoneStep => CurrentStep == 0;
    public bool IsOtpStep => CurrentStep == 1;
    public bool IsInfoStep => CurrentStep == 2;
    public bool IsKvkkStep => CurrentStep == 3;

    public string StepTitle => CurrentStep switch
    {
        0 => "Telefon Numarası",
        1 => "Doğrulama Kodu",
        2 => "Kişisel Bilgiler",
        3 => "KVKK Onayı",
        _ => ""
    };

    // Step 0 — Phone
    [ObservableProperty] private string _phoneNumber = string.Empty;

    // Step 1 — OTP
    [ObservableProperty] private string _otpCode = string.Empty;
    private string _registrationToken = string.Empty;

    // Step 2 — Personal info
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _passwordConfirm = string.Empty;
    [ObservableProperty] private int _selectedGender;
    [ObservableProperty] private DateTime _birthDate = new(2000, 1, 1);
    [ObservableProperty] private int _selectedMaritalStatus;
    [ObservableProperty] private int _selectedGsmOperator;
    [ObservableProperty] private LookupItemDto? _selectedCity;
    [ObservableProperty] private LookupItemDto? _selectedDistrict;
    [ObservableProperty] private LookupItemDto? _selectedProfession;
    [ObservableProperty] private LookupItemDto? _selectedEducationLevel;
    [ObservableProperty] private bool _isRetired;

    // Hane reisi
    [ObservableProperty] private bool _isHeadOfFamily;
    [ObservableProperty] private bool _isHeadOfFamilyRetired;
    [ObservableProperty] private LookupItemDto? _selectedHofProfession;
    [ObservableProperty] private LookupItemDto? _selectedHofEducationLevel;

    // Referans kodu
    [ObservableProperty] private string _referenceCode = string.Empty;

    // Lookup collections
    [ObservableProperty] private ObservableCollection<LookupItemDto> _cities = [];
    [ObservableProperty] private ObservableCollection<LookupItemDto> _districts = [];
    [ObservableProperty] private ObservableCollection<LookupItemDto> _professions = [];
    [ObservableProperty] private ObservableCollection<LookupItemDto> _educationLevels = [];

    public List<string> GenderOptions { get; } = ["Erkek", "Kadın", "Diğer"];
    public List<string> MaritalStatusOptions { get; } = ["Bekar", "Evli", "Boşanmış", "Dul"];
    public List<string> GsmOperatorOptions { get; } = ["Turkcell", "Vodafone", "Türk Telekom", "Diğer"];

    // Step 3 — KVKK
    [ObservableProperty] private bool _kvkkAccepted;

    // Common
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    async partial void OnSelectedCityChanged(LookupItemDto? value)
    {
        if (value is null) return;
        try
        {
            var list = await _api.GetRegisterDistrictsAsync(value.Id);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Districts = new ObservableCollection<LookupItemDto>(list);
                SelectedDistrict = null;
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    partial void OnIsHeadOfFamilyChanged(bool value)
    {
        if (value)
        {
            SelectedHofProfession = null;
            SelectedHofEducationLevel = null;
            IsHeadOfFamilyRetired = false;
        }
    }

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
            await _api.SendOtpAsync(phone);
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
    private async Task VerifyOtpAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length < 4)
        {
            ErrorMessage = "Doğrulama kodunu girin.";
            return;
        }

        IsBusy = true;
        try
        {
            _registrationToken = await _api.VerifyOtpAsync(PhoneNumber.Trim(), OtpCode.Trim());
            CurrentStep = 2;
            await LoadLookupsAsync();
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

    private async Task LoadLookupsAsync()
    {
        var cities = await _api.GetRegisterCitiesAsync();
        var professions = await _api.GetRegisterProfessionsAsync();
        var educationLevels = await _api.GetRegisterEducationLevelsAsync();
        Cities = new ObservableCollection<LookupItemDto>(cities);
        Professions = new ObservableCollection<LookupItemDto>(professions);
        EducationLevels = new ObservableCollection<LookupItemDto>(educationLevels);
    }

    [RelayCommand]
    private void GoToKvkk()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Ad ve soyad gerekli.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "E-posta gerekli.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Password) || !IsPasswordStrong(Password))
        {
            ErrorMessage = "Parola en az 8 karakter, büyük/küçük harf, rakam ve özel karakter içermeli.";
            return;
        }
        if (Password != PasswordConfirm)
        {
            ErrorMessage = "Parolalar eşleşmiyor.";
            return;
        }
        if (SelectedCity is null || SelectedDistrict is null ||
            SelectedProfession is null || SelectedEducationLevel is null)
        {
            ErrorMessage = "Tüm alanları doldurun.";
            return;
        }
        if (!IsHeadOfFamily && (SelectedHofProfession is null || SelectedHofEducationLevel is null))
        {
            ErrorMessage = "Hane reisi bilgilerini doldurun.";
            return;
        }

        CurrentStep = 3;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = string.Empty;

        if (!KvkkAccepted)
        {
            ErrorMessage = "KVKK metnini onaylamanız gerekiyor.";
            return;
        }

        IsBusy = true;
        try
        {
            var ip = "unknown";
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                ip = (await http.GetStringAsync("https://api.ipify.org")).Trim();
            }
            catch { /* IP alınamazsa devam et */ }

            var deviceInfo = $"{DeviceInfo.Manufacturer} {DeviceInfo.Model} - {DeviceInfo.Platform} {DeviceInfo.VersionString} | IP: {ip}";

            var dto = new
            {
                registrationToken = _registrationToken,
                firstName = FirstName.Trim(),
                lastName = LastName.Trim(),
                email = Email.Trim(),
                password = Password,
                gender = SelectedGender + 1,
                birthDate = DateOnly.FromDateTime(BirthDate).ToString("yyyy-MM-dd"),
                maritalStatus = SelectedMaritalStatus + 1,
                gsmOperator = SelectedGsmOperator + 1,
                cityId = SelectedCity!.Id,
                districtId = SelectedDistrict!.Id,
                isRetired = IsRetired,
                professionId = SelectedProfession!.Id,
                educationLevelId = SelectedEducationLevel!.Id,
                isHeadOfFamily = IsHeadOfFamily,
                isHeadOfFamilyRetired = IsHeadOfFamily ? false : IsHeadOfFamilyRetired,
                headOfFamilyProfessionId = IsHeadOfFamily ? (int?)null : SelectedHofProfession?.Id,
                headOfFamilyEducationLevelId = IsHeadOfFamily ? (int?)null : SelectedHofEducationLevel?.Id,
                referenceCode = string.IsNullOrWhiteSpace(ReferenceCode) ? null : ReferenceCode.Trim(),
                kvkkApproval = true,
                kvkkDetail = deviceInfo
            };

            await _api.RegisterAsync(dto);

            await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
                "Başarılı",
                "Kaydınız alındı. Hesabınız onaylandıktan sonra giriş yapabilirsiniz.",
                "Tamam");

            await Application.Current!.Windows[0].Page!.Navigation.PopToRootAsync();
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

    private static bool IsPasswordStrong(string password)
    {
        if (password.Length < 8) return false;
        bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
        foreach (var c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSpecial = true;
        }
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
