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
        var storiesTask = FetchOrFallbackAsync(
            () => _apiClient.GetStoriesAsync(),
            () => _mockDataService.GetStories());
        var newsTask = FetchOrFallbackAsync(
            () => _apiClient.GetNewsAsync(),
            () => _mockDataService.GetNews());
        var surveysTask = FetchOrFallbackAsync(
            () => _apiClient.GetProjectsAsync(),
            () => _mockDataService.GetSurveys());

        await Task.WhenAll(storiesTask, newsTask, surveysTask);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var orderedStories = storiesTask.Result
                .OrderBy(s => s.IsSeen)
                .ToList();

            Stories = new ObservableCollection<StoryModel>(orderedStories);
            News = new ObservableCollection<NewsModel>(newsTask.Result);
            Surveys = new ObservableCollection<SurveyModel>(surveysTask.Result);
        });
    }

    private static async Task<List<T>> FetchOrFallbackAsync<T>(
        Func<Task<List<T>>> fetch, Func<List<T>> fallback)
    {
        try { return await fetch(); }
        catch { return fallback(); }
    }

    [RelayCommand]
    private async Task NavigateToSurveyDetail(SurveyModel survey)
    {
        await Shell.Current.GoToAsync($"surveydetail?id={survey.Id}");
    }

    [RelayCommand]
    private async Task OpenStory(StoryModel story)
    {
        if (!story.IsSeen)
            _ = MarkStorySeenAndReorderAsync(story);

        await Shell.Current.GoToAsync($"storyviewer?id={story.Id}&session={Guid.NewGuid():N}");
    }

    [RelayCommand]
    private async Task NavigateToNotifications()
    {
        await Shell.Current.GoToAsync("notifications");
    }

    private async Task MarkStorySeenAndReorderAsync(StoryModel story)
    {
        try
        {
            await _apiClient.MarkStorySeenAsync(story.Id);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Stories.Count == 0)
                    return;

                var index = Stories.IndexOf(story);
                if (index < 0)
                    return;

                var seenStory = story with { IsSeen = true };
                Stories.RemoveAt(index);
                Stories.Add(seenStory);
            });
        }
        catch
        {
            // Story seen senkronizasyonu kritik bir akış değil.
        }
    }
}
