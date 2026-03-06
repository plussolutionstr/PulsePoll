using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _api;
    private readonly IServiceProvider _serviceProvider;

    public ProfileViewModel(IPulsePollApiClient api, IServiceProvider serviceProvider)
    {
        _api = api;
        _serviceProvider = serviceProvider;
        LoadProfileCommand.ExecuteAsync(null);
    }

    // Header
    [ObservableProperty] private string _fullName = "";
    [ObservableProperty] private string _initials = "";
    [ObservableProperty] private string? _profilePhotoUrl;
    [ObservableProperty] private bool _hasProfilePhoto;
    [ObservableProperty] private string _referralCode = "";
    [ObservableProperty] private int _star;

    // Stats
    [ObservableProperty] private int _completedCount;
    [ObservableProperty] private int _disqualifiedCount;
    [ObservableProperty] private int _successRate;

    // Editable fields
    [ObservableProperty] private string _firstName = "";
    [ObservableProperty] private string _lastName = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _phoneNumber = "";

    [ObservableProperty] private int _selectedGender;
    [ObservableProperty] private string _birthDateDisplay = "";
    [ObservableProperty] private DateOnly _birthDate;
    [ObservableProperty] private int _selectedMaritalStatus;
    [ObservableProperty] private int _selectedGsmOperator;

    [ObservableProperty] private LookupItemDto? _selectedCity;
    [ObservableProperty] private LookupItemDto? _selectedDistrict;
    [ObservableProperty] private LookupItemDto? _selectedProfession;
    [ObservableProperty] private LookupItemDto? _selectedEducationLevel;
    [ObservableProperty] private bool _isRetired;

    [ObservableProperty] private bool _isHeadOfFamily;
    [ObservableProperty] private bool _isHeadOfFamilyRetired;
    [ObservableProperty] private LookupItemDto? _selectedHofProfession;
    [ObservableProperty] private LookupItemDto? _selectedHofEducationLevel;

    // Lookup lists
    [ObservableProperty] private ObservableCollection<LookupItemDto> _cities = [];
    [ObservableProperty] private ObservableCollection<LookupItemDto> _districts = [];
    [ObservableProperty] private ObservableCollection<LookupItemDto> _professions = [];
    [ObservableProperty] private ObservableCollection<LookupItemDto> _educationLevels = [];

    // UI State
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _statusMessage = "";
    private CancellationTokenSource? _statusCts;

    public List<string> GenderOptions { get; } = ["Erkek", "Kadın", "Diğer"];
    public List<string> MaritalStatusOptions { get; } = ["Bekar", "Evli", "Boşanmış", "Dul"];
    public List<string> GsmOperatorOptions { get; } = ["Turkcell", "Vodafone", "Türk Telekom", "Diğer"];

    private ProfileApiDto? _currentProfile;
    private bool _isInitialLoad;

    partial void OnStatusMessageChanged(string value)
    {
        _statusCts?.Cancel();
        if (string.IsNullOrEmpty(value)) return;
        _statusCts = new CancellationTokenSource();
        _ = ClearStatusAfterDelayAsync(_statusCts.Token);
    }

    private async Task ClearStatusAfterDelayAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(3000, ct);
            StatusMessage = "";
        }
        catch (TaskCanceledException) { }
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        IsLoading = true;
        try
        {
            _currentProfile = await _api.GetProfileAsync();
            if (_currentProfile is null) return;

            MapProfileToFields(_currentProfile);

            var citiesTask = _api.GetCitiesAsync();
            var professionsTask = _api.GetProfessionsAsync();
            var educationTask = _api.GetEducationLevelsAsync();

            var cities = await citiesTask;
            var professions = await professionsTask;
            var educationLevels = await educationTask;

            _isInitialLoad = true;

            Cities = new ObservableCollection<LookupItemDto>(cities);
            Professions = new ObservableCollection<LookupItemDto>(professions);
            EducationLevels = new ObservableCollection<LookupItemDto>(educationLevels);

            SelectedCity = cities.FirstOrDefault(c => c.Name == _currentProfile.CityName);
            SelectedProfession = professions.FirstOrDefault(p => p.Name == _currentProfile.ProfessionName);
            SelectedEducationLevel = educationLevels.FirstOrDefault(e => e.Name == _currentProfile.EducationLevelName);

            if (SelectedCity is not null)
            {
                var districts = await _api.GetDistrictsAsync(SelectedCity.Id);
                Districts = new ObservableCollection<LookupItemDto>(districts);
                SelectedDistrict = districts.FirstOrDefault(d => d.Name == _currentProfile.DistrictName);
            }

            _isInitialLoad = false;

            if (!_currentProfile.IsHeadOfFamily)
            {
                SelectedHofProfession = professions.FirstOrDefault(p => p.Name == _currentProfile.HeadOfFamilyProfessionName);
                SelectedHofEducationLevel = educationLevels.FirstOrDefault(e => e.Name == _currentProfile.HeadOfFamilyEducationLevelName);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Profil yüklenemedi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void MapProfileToFields(ProfileApiDto p)
    {
        FullName = p.FullName;
        Initials = GetInitials(p.FirstName, p.LastName);
        ProfilePhotoUrl = p.ProfilePhotoUrl;
        HasProfilePhoto = !string.IsNullOrEmpty(p.ProfilePhotoUrl);
        ReferralCode = p.ReferralCode;
        Star = p.Star ?? 0;

        FirstName = p.FirstName;
        LastName = p.LastName;
        Email = p.Email;
        PhoneNumber = p.PhoneNumber;
        SelectedGender = p.Gender - 1;
        BirthDate = DateOnly.Parse(p.BirthDate);
        BirthDateDisplay = BirthDate.ToString("dd.MM.yyyy");
        SelectedMaritalStatus = p.MaritalStatus - 1;
        SelectedGsmOperator = p.GsmOperator - 1;
        IsRetired = p.IsRetired;
        IsHeadOfFamily = p.IsHeadOfFamily;
        IsHeadOfFamilyRetired = p.IsHeadOfFamilyRetired;

        CompletedCount = p.CompletedCount;
        DisqualifiedCount = p.DisqualifiedCount;
        SuccessRate = p.SuccessRate;
    }

    partial void OnSelectedCityChanged(LookupItemDto? value)
    {
        if (value is null || _isInitialLoad) return;
        _ = LoadDistrictsAsync(value.Id);
    }

    partial void OnIsHeadOfFamilyChanged(bool value)
    {
        if (_isInitialLoad) return;
        if (value)
        {
            SelectedHofProfession = null;
            SelectedHofEducationLevel = null;
            IsHeadOfFamilyRetired = false;
        }
    }

    private async Task LoadDistrictsAsync(int cityId)
    {
        var districts = await _api.GetDistrictsAsync(cityId);
        Districts = new ObservableCollection<LookupItemDto>(districts);
        SelectedDistrict = null;
    }

    [RelayCommand]
    private void ToggleEdit()
    {
        IsEditing = !IsEditing;
        StatusMessage = "";
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        StatusMessage = "";
        if (_currentProfile is not null)
            MapProfileToFields(_currentProfile);
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (SelectedCity is null || SelectedDistrict is null ||
            SelectedProfession is null || SelectedEducationLevel is null)
        {
            StatusMessage = "Lütfen tüm alanları doldurun.";
            return;
        }

        IsSaving = true;
        StatusMessage = "";
        try
        {
            var dto = new
            {
                firstName = FirstName,
                lastName = LastName,
                email = Email,
                gender = SelectedGender + 1,
                birthDate = BirthDate.ToString("yyyy-MM-dd"),
                maritalStatus = SelectedMaritalStatus + 1,
                gsmOperator = SelectedGsmOperator + 1,
                cityId = SelectedCity.Id,
                districtId = SelectedDistrict.Id,
                professionId = SelectedProfession.Id,
                educationLevelId = SelectedEducationLevel.Id,
                isRetired = IsRetired,
                isHeadOfFamily = IsHeadOfFamily,
                isHeadOfFamilyRetired = IsHeadOfFamilyRetired,
                headOfFamilyProfessionId = IsHeadOfFamily ? (int?)null : SelectedHofProfession?.Id,
                headOfFamilyEducationLevelId = IsHeadOfFamily ? (int?)null : SelectedHofEducationLevel?.Id
            };

            var updated = await _api.UpdateProfileAsync(dto);
            if (updated is not null)
            {
                _currentProfile = updated;
                MapProfileToFields(updated);
                StatusMessage = "Profil güncellendi.";
            }

            IsEditing = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CopyReferralCodeAsync()
    {
        await Clipboard.Default.SetTextAsync(ReferralCode);
        StatusMessage = "Referans kodu kopyalandı!";
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            string action = await Shell.Current.DisplayActionSheetAsync(
                "Profil Fotoğrafı", "İptal", null, "Galeriden Seç", "Kamera ile Çek");

            FileResult? photo = action switch
            {
                "Galeriden Seç" => (await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
                {
                    Title = "Profil fotoğrafı seçin"
                })).FirstOrDefault(),
                "Kamera ile Çek" => await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Profil fotoğrafı çekin"
                }),
                _ => null
            };

            if (photo is null) return;

            IsLoading = true;
            StatusMessage = "Fotoğraf yükleniyor...";

            await using var originalStream = await photo.OpenReadAsync();
            var resizedStream = await ResizeImageAsync(originalStream, 512, 512);
            var url = await _api.UploadProfilePhotoAsync(resizedStream, photo.FileName, "image/jpeg", default);

            if (!string.IsNullOrEmpty(url))
            {
                ProfilePhotoUrl = url;
                HasProfilePhoto = true;
                StatusMessage = "Fotoğraf yüklendi.";
            }
        }
        catch (PermissionException)
        {
            StatusMessage = "Kamera/galeri izni gerekiyor.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fotoğraf yüklenemedi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Çıkış Yap", "Hesabınızdan çıkış yapmak istediğinize emin misiniz?", "Çıkış Yap", "İptal");

        if (!confirm) return;

        try
        {
            await _api.LogoutAsync();
        }
        catch
        {
            // ignore — tokens are already cleared
        }

        var welcomePage = _serviceProvider.GetRequiredService<Views.WelcomePage>();
        Application.Current!.Windows[0].Page = new NavigationPage(welcomePage)
        {
            BarBackgroundColor = Color.FromArgb("#F7F5FF"),
            BarTextColor = Color.FromArgb("#1A1535")
        };
    }

    private static async Task<Stream> ResizeImageAsync(Stream sourceStream, int maxWidth, int maxHeight)
    {
        var image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(sourceStream);

        if (image.Width <= maxWidth && image.Height <= maxHeight)
        {
            sourceStream.Position = 0;
            return sourceStream;
        }

        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);
        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        var resized = image.Resize(newWidth, newHeight, ResizeMode.Stretch);

        var ms = new MemoryStream();
        await Task.Run(() => resized.Save(ms, Microsoft.Maui.Graphics.ImageFormat.Jpeg, 0.75f));
        ms.Position = 0;
        return ms;
    }

    private static string GetInitials(string firstName, string lastName)
    {
        var f = string.IsNullOrEmpty(firstName) ? "" : firstName[..1].ToUpper();
        var l = string.IsNullOrEmpty(lastName) ? "" : lastName[..1].ToUpper();
        return $"{f}{l}";
    }
}
