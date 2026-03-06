using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Helpers;
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
    [ObservableProperty] private ImageSource? _profilePhotoSource;
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
    private CancellationTokenSource? _districtCts;
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

            Cities.Clear();
            foreach (var c in cities) Cities.Add(c);
            Professions.Clear();
            foreach (var p in professions) Professions.Add(p);
            EducationLevels.Clear();
            foreach (var e in educationLevels) EducationLevels.Add(e);

            SelectedCity = cities.FirstOrDefault(c => c.Name == _currentProfile.CityName);
            SelectedProfession = professions.FirstOrDefault(p => p.Name == _currentProfile.ProfessionName);
            SelectedEducationLevel = educationLevels.FirstOrDefault(e => e.Name == _currentProfile.EducationLevelName);

            if (SelectedCity is not null)
            {
                var districts = await _api.GetDistrictsAsync(SelectedCity.Id);
                Districts.Clear();
                foreach (var d in districts) Districts.Add(d);
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
        HasProfilePhoto = !string.IsNullOrEmpty(p.ProfilePhotoUrl);
        if (HasProfilePhoto)
            _ = LoadProfilePhotoAsync(p.ProfilePhotoUrl!);
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
        if (_isInitialLoad) return;
        _ = LoadDistrictsAsync(value);
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
            var list = await _api.GetDistrictsAsync(city.Id, token);
            if (token.IsCancellationRequested) return;
            foreach (var item in list)
                Districts.Add(item);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            StatusMessage = $"İlçeler yüklenemedi: {ex.Message}";
        }
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

            FileResult? photo = null;

            if (action == "Galeriden Seç")
            {
                photo = (await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
                {
                    Title = "Profil fotoğrafı seçin"
                })).FirstOrDefault();
            }
            else if (action == "Kamera ile Çek")
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.Camera>();

                if (status != PermissionStatus.Granted)
                {
                    StatusMessage = "Kamera izni gerekiyor.";
                    return;
                }

                photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Profil fotoğrafı çekin"
                });
            }

            if (photo is null) return;

            IsLoading = true;
            StatusMessage = "Fotoğraf yükleniyor...";

            // photo.FullPath on iOS camera capture may return only a filename;
            // copy to a temp file with a known full path to ensure ImageResizer can load it
            var tempPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
            await using (var sourceStream = await photo.OpenReadAsync())
            await using (var fs = File.Create(tempPath))
                await sourceStream.CopyToAsync(fs);

            var resizedStream = await ImageResizer.ResizeAsync(tempPath, 512, 512, 75);
            var url = await _api.UploadProfilePhotoAsync(resizedStream, "profile.jpg", "image/jpeg", default);

            if (!string.IsNullOrEmpty(url))
            {
                HasProfilePhoto = true;
                await LoadProfilePhotoAsync(url);
                StatusMessage = "Fotoğraf yüklendi.";
            }
        }
        catch (PermissionException pex)
        {
            StatusMessage = $"İzin gerekli: {pex.Message}";
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

    private async Task LoadProfilePhotoAsync(string url)
    {
        try
        {
            var bytes = await _api.GetImageBytesAsync(url);
            if (bytes is { Length: > 0 })
                ProfilePhotoSource = ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        catch
        {
            // ignore — initials will show instead
        }
    }

    private static string GetInitials(string firstName, string lastName)
    {
        var f = string.IsNullOrEmpty(firstName) ? "" : firstName[..1].ToUpper();
        var l = string.IsNullOrEmpty(lastName) ? "" : lastName[..1].ToUpper();
        return $"{f}{l}";
    }
}
