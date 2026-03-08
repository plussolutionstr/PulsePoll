using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class SurveysViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;

    public SurveysViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private ObservableCollection<SurveyModel> _surveys = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _isLoading = true;
    [ObservableProperty] private bool _hasConnectionError;

    public bool IsEmpty => !IsLoading && Surveys.Count == 0;

    private bool _isLoaded;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (_isLoaded)
            return;

        IsLoading = true;
        try
        {
            var surveys = await _apiClient.GetProjectsAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Surveys = new ObservableCollection<SurveyModel>(surveys);
                _isLoaded = true;
            });
        }
        catch
        {
            HasConnectionError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RetryConnectionAsync()
    {
        HasConnectionError = false;
        _isLoaded = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task OpenSurveyAsync(SurveyModel survey)
    {
        if (survey is null)
            return;

        await Shell.Current.GoToAsync($"surveydetail?id={survey.Id}");
    }
}
