using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _api;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterViewModel(
        IPulsePollApiClient api,
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory)
    {
        _api = api;
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
    }

    // Step tracking: 0=Phone, 1=OTP, 2=PersonalInfo, 3=KVKK
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPhoneStep))]
    [NotifyPropertyChangedFor(nameof(IsOtpStep))]
    [NotifyPropertyChangedFor(nameof(IsInfoStep))]
    [NotifyPropertyChangedFor(nameof(IsKvkkStep))]
    [NotifyPropertyChangedFor(nameof(StepTitle))]
    [NotifyPropertyChangedFor(nameof(StepSubtitle))]
    private int _currentStep;

    public bool IsPhoneStep => CurrentStep == 0;
    public bool IsOtpStep => CurrentStep == 1;
    public bool IsInfoStep => CurrentStep == 2;
    public bool IsKvkkStep => CurrentStep == 3;

    public string StepTitle => CurrentStep switch
    {
        0 => "Hesap Oluştur",
        1 => "Doğrulama Kodu",
        2 => "Kişisel Bilgiler",
        3 => "KVKK Onayı",
        _ => ""
    };

    public string StepSubtitle => CurrentStep switch
    {
        0 => "Hemen ücretsiz kayıt ol, anketlere katılmaya başla.",
        1 => "Telefonunuza gönderilen 6 haneli kodu girin.",
        2 => "Bilgilerinizi eksiksiz doldurun.",
        3 => "",
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
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrengthColor1))]
    [NotifyPropertyChangedFor(nameof(StrengthColor2))]
    [NotifyPropertyChangedFor(nameof(StrengthColor3))]
    [NotifyPropertyChangedFor(nameof(StrengthColor4))]
    [NotifyPropertyChangedFor(nameof(StrengthLabel))]
    [NotifyPropertyChangedFor(nameof(StrengthLabelColor))]
    private string _password = string.Empty;
    [ObservableProperty] private string _passwordConfirm = string.Empty;
    [ObservableProperty] private int _selectedGender;
    [ObservableProperty] private DateTime _birthDate = new(2000, 1, 1);

    public DateTime MinBirthDate => DateTime.Today.AddYears(-100);
    public DateTime MaxBirthDate => DateTime.Today.AddYears(-18);
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

    // District loading cancellation
    private CancellationTokenSource? _districtCts;

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
    [ObservableProperty] private string _kvkkText = string.Empty;

    // Common
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    partial void OnSelectedCityChanged(LookupItemDto? value)
    {
        _ = LoadDistrictsAsync(value);
    }

    private async Task LoadDistrictsAsync(LookupItemDto? city)
    {
        _districtCts?.Cancel();
        _districtCts?.Dispose();
        _districtCts = new CancellationTokenSource();
        var token = _districtCts.Token;

        SelectedDistrict = null;
        Districts.Clear();

        if (city is null) return;

        try
        {
            var list = await _api.GetRegisterDistrictsAsync(city.Id, token);
            if (token.IsCancellationRequested) return;
            foreach (var item in list)
                Districts.Add(item);
        }
        catch (OperationCanceledException) { }
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
        var phone = NormalizePhone(PhoneNumber);
        if (phone.Length != 10 || !phone.StartsWith('5'))
        {
            ErrorMessage = "Geçerli bir telefon numarası girin. (05XX XXX XX XX)";
            return;
        }

        IsBusy = true;
        try
        {
            await _api.SendOtpAsync(PhoneNumber.Trim());
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
        if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Trim().Length != 6 || !OtpCode.Trim().All(char.IsDigit))
        {
            ErrorMessage = "6 haneli doğrulama kodunu girin.";
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
    private async Task GoToKvkkAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Ad ve soyad gerekli.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email.Trim()))
        {
            ErrorMessage = "Geçerli bir e-posta adresi girin.";
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

        IsBusy = true;
        try
        {
            var content = await _api.GetAppContentAsync();
            KvkkText = content?.KvkkText ?? "KVKK metni yüklenemedi.";
            CurrentStep = 3;
        }
        catch
        {
            KvkkText = "KVKK metni yüklenemedi.";
            CurrentStep = 3;
        }
        finally
        {
            IsBusy = false;
        }
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
                using var http = _httpClientFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(5);
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

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Application.Current!.Windows[0].Page!.Navigation.PopAsync();
    }

    // Password strength
    private int PasswordStrengthLevel
    {
        get
        {
            if (string.IsNullOrEmpty(Password)) return 0;
            int score = 0;
            if (Password.Length >= 8) score++;
            if (Password.Any(char.IsUpper) && Password.Any(char.IsLower)) score++;
            if (Password.Any(char.IsDigit)) score++;
            if (Password.Any(c => !char.IsLetterOrDigit(c))) score++;
            return score;
        }
    }

    private static Color GetBorderColor() =>
        Application.Current!.Resources.TryGetValue("CardBorder", out var c) ? (Color)c : Colors.LightGray;
    private static Color GetDangerColor() =>
        Application.Current!.Resources.TryGetValue("Danger", out var c) ? (Color)c : Colors.Red;
    private static Color GetAmberColor() =>
        Application.Current!.Resources.TryGetValue("Amber", out var c) ? (Color)c : Colors.Orange;
    private static Color GetSuccessColor() =>
        Application.Current!.Resources.TryGetValue("Success", out var c) ? (Color)c : Colors.Green;

    private Color GetStrengthSegColor(int segment)
    {
        var level = PasswordStrengthLevel;
        if (segment > level) return GetBorderColor();
        return level switch
        {
            1 => GetDangerColor(),
            2 => GetAmberColor(),
            _ => GetSuccessColor()
        };
    }

    public Color StrengthColor1 => GetStrengthSegColor(1);
    public Color StrengthColor2 => GetStrengthSegColor(2);
    public Color StrengthColor3 => GetStrengthSegColor(3);
    public Color StrengthColor4 => GetStrengthSegColor(4);

    public string StrengthLabel => PasswordStrengthLevel switch
    {
        0 => "",
        1 => "Zayıf",
        2 => "Orta",
        3 => "İyi",
        4 => "Güçlü",
        _ => ""
    };

    public Color StrengthLabelColor => PasswordStrengthLevel switch
    {
        1 => GetDangerColor(),
        2 => GetAmberColor(),
        _ => GetSuccessColor()
    };

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

    private static bool IsValidEmail(string email)
        => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private static string NormalizePhone(string raw)
    {
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("90") && digits.Length == 12)
            digits = digits[2..];
        if (digits.StartsWith('0') && digits.Length == 11)
            digits = digits[1..];
        return digits;
    }
}
