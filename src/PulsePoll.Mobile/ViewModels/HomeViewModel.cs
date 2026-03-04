using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly MockDataService _mockDataService;

    public HomeViewModel(IPulsePollApiClient apiClient, MockDataService mockDataService)
    {
        _apiClient = apiClient;
        _mockDataService = mockDataService;
    }

    [ObservableProperty] private ObservableCollection<StoryModel> _stories = [];
    [ObservableProperty] private ObservableCollection<NewsModel> _news = [];
    [ObservableProperty] private ObservableCollection<SurveyModel> _surveys = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isRefreshing;

    private bool _isLoaded;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (_isLoaded) return;

        IsLoading = true;
        try
        {
            await FetchDataAsync();
            _isLoaded = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            await FetchDataAsync();
            _isLoaded = true;
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task FetchDataAsync()
    {
        try
        {
            var storiesTask = _apiClient.GetStoriesAsync();
            var newsTask = _apiClient.GetNewsAsync();
            var projectsTask = _apiClient.GetProjectsAsync();

            await Task.WhenAll(storiesTask, newsTask, projectsTask);

            Stories = new ObservableCollection<StoryModel>(storiesTask.Result);
            News = new ObservableCollection<NewsModel>(newsTask.Result);
            Surveys = new ObservableCollection<SurveyModel>(projectsTask.Result);
        }
        catch
        {
            Stories = new ObservableCollection<StoryModel>(_mockDataService.GetStories());
            News = new ObservableCollection<NewsModel>(_mockDataService.GetNews());
            Surveys = new ObservableCollection<SurveyModel>(_mockDataService.GetSurveys());
        }
    }

    [RelayCommand]
    private async Task NavigateToSurveyDetail(SurveyModel survey)
    {
        await Shell.Current.GoToAsync($"surveydetail?id={survey.Id}");
    }

    [RelayCommand]
    private async Task NavigateToNotifications()
    {
        await Shell.Current.GoToAsync("notifications");
    }
}
