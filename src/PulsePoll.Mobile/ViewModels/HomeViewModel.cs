using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly NotificationCenterService _notificationCenter;

    public HomeViewModel(
        IPulsePollApiClient apiClient,
        NotificationCenterService notificationCenter)
    {
        _apiClient = apiClient;
        _notificationCenter = notificationCenter;
        _notificationCenter.PropertyChanged += OnNotificationCenterPropertyChanged;
        SyncNotificationBadge();
    }

    [ObservableProperty] private ObservableCollection<StoryModel> _stories = [];
    [ObservableProperty] private ObservableCollection<NewsModel> _news = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoSurveys))]
    private ObservableCollection<SurveyModel> _surveys = [];
    [ObservableProperty] private bool _isLoading;

    public bool HasNoSurveys => Surveys.Count == 0 && _isLoaded;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private bool _needsRefreshOnReturn;
    [ObservableProperty] private bool _hasUnreadNotifications;
    [ObservableProperty] private int _unreadNotificationCount;

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
            OnPropertyChanged(nameof(HasNoSurveys));
        }
        catch
        {
            await Shell.Current.GoToAsync("connectionerror");
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
            OnPropertyChanged(nameof(HasNoSurveys));
            NeedsRefreshOnReturn = false;
        }
        catch
        {
            await Shell.Current.GoToAsync("connectionerror");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadNotificationBadgeAsync()
    {
        try
        {
            await _notificationCenter.LoadAsync();
        }
        catch
        {
            // Bildirim badge yüklenemese de anasayfa çalışmaya devam etmeli.
        }

        SyncNotificationBadge();
    }

    private async Task FetchDataAsync()
    {
        var storiesTask = _apiClient.GetStoriesAsync();
        var newsTask = _apiClient.GetNewsAsync();
        var surveysTask = _apiClient.GetProjectsAsync();

        await Task.WhenAll(storiesTask, newsTask, surveysTask);
        var stories = await storiesTask;
        var news = await newsTask;
        var surveys = await surveysTask;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var orderedStories = stories
                .OrderBy(s => s.IsSeen)
                .ToList();

            Stories = new ObservableCollection<StoryModel>(orderedStories);
            News = new ObservableCollection<NewsModel>(news);
            Surveys = new ObservableCollection<SurveyModel>(surveys.Take(3));
            OnPropertyChanged(nameof(HasNoSurveys));
        });
    }

    [RelayCommand]
    private async Task NavigateToSurveyDetail(SurveyModel survey)
    {
        await Shell.Current.GoToAsync($"surveydetail?id={survey.Id}");
    }

    [RelayCommand]
    private async Task NavigateToSurveys()
    {
        await Shell.Current.GoToAsync("//surveys");
    }

    [RelayCommand]
    private async Task OpenNews(NewsModel news)
    {
        await Shell.Current.GoToAsync($"newsdetail?id={news.Id}");
    }

    [RelayCommand]
    private async Task OpenStory(StoryModel story)
    {
        NeedsRefreshOnReturn = true;

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

    private void OnNotificationCenterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NotificationCenterService.UnreadCount) or nameof(NotificationCenterService.Items))
            SyncNotificationBadge();
    }

    private void SyncNotificationBadge()
    {
        UnreadNotificationCount = _notificationCenter.UnreadCount;
        HasUnreadNotifications = _notificationCenter.HasUnread;
    }
}
