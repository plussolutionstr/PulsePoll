using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(Title), "title")]
[QueryProperty(nameof(Url), "url")]
public partial class SurveyWebViewViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "Anket";
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private bool _isLoading = true;

    public string ResolvedUrl => Uri.UnescapeDataString(Url ?? string.Empty);

    partial void OnUrlChanged(string value)
    {
        OnPropertyChanged(nameof(ResolvedUrl));
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.GoToAsync("..");
    }
}
