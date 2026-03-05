using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class SurveysViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly MockDataService _dataService;

    public SurveysViewModel(IPulsePollApiClient apiClient, MockDataService dataService)
    {
        _apiClient = apiClient;
        _dataService = dataService;
    }

    [ObservableProperty] private ObservableCollection<SurveyModel> _surveys = [];
    [ObservableProperty] private bool _isLoading;

    private bool _isLoaded;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (_isLoaded)
            return;

        IsLoading = true;
        try
        {
            var surveys = await FetchOrFallbackAsync(
                () => _apiClient.GetProjectsAsync(),
                () => _dataService.GetSurveys());

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Surveys = new ObservableCollection<SurveyModel>(surveys);
                _isLoaded = true;
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenSurveyAsync(SurveyModel survey)
    {
        if (survey is null)
            return;

        await Shell.Current.GoToAsync($"surveydetail?id={survey.Id}");
    }

    private static async Task<List<T>> FetchOrFallbackAsync<T>(
        Func<Task<List<T>>> fetch, Func<List<T>> fallback)
    {
        try { return await fetch(); }
        catch { return fallback(); }
    }

}
