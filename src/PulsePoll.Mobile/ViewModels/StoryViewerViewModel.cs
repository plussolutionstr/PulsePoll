using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(StoryId), "id")]
[QueryProperty(nameof(SessionKey), "session")]
public partial class StoryViewerViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly HashSet<int> _seenSyncStoryIds = [];

    private CancellationTokenSource? _progressCts;

    private const int StoryDurationMs = 5000;
    private const int ProgressTickMs = 50;

    public StoryViewerViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty] private int _storyId;
    [ObservableProperty] private string _sessionKey = string.Empty;
    [ObservableProperty] private ObservableCollection<StoryModel> _stories = [];
    [ObservableProperty] private ObservableCollection<StoryProgressSegment> _progressSegments = [];
    [ObservableProperty] private StoryModel? _currentStory;
    [ObservableProperty] private int _currentIndex;
    [ObservableProperty] private double _currentProgress;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _hasConnectionError;

    public bool HasLink => !string.IsNullOrWhiteSpace(CurrentStory?.LinkUrl);
    public string ProgressText => Stories.Count == 0 ? string.Empty : $"{CurrentIndex + 1}/{Stories.Count}";

    partial void OnStoryIdChanged(int value)
    {
        // SessionKey üzerinden kontrollü yükleme yapılıyor.
    }

    partial void OnSessionKeyChanged(string value)
    {
        if (StoryId <= 0)
            return;

        _ = LoadStoriesAsync(StoryId);
    }

    partial void OnCurrentStoryChanged(StoryModel? value)
    {
        OnPropertyChanged(nameof(HasLink));

        if (value is not null)
            _ = MarkStorySeenSafeAsync(value.Id);
    }

    partial void OnCurrentIndexChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressText));
    }

    partial void OnCurrentProgressChanged(double value)
    {
        UpdateProgressSegments();
    }

    public void ResumeProgress()
    {
        if (IsLoading || CurrentStory is null || _progressCts is not null)
            return;

        RestartProgressLoop();
    }

    public void PauseProgress()
    {
        _progressCts?.Cancel();
        _progressCts = null;
    }

    [RelayCommand]
    private async Task RetryConnectionAsync()
    {
        HasConnectionError = false;
        await LoadStoriesAsync(StoryId);
    }

    private async Task LoadStoriesAsync(int selectedStoryId)
    {
        PauseProgress();
        IsLoading = true;

        try
        {
            var stories = await _apiClient.GetStoriesAsync();

            Stories = new ObservableCollection<StoryModel>(stories);
            ProgressSegments = new ObservableCollection<StoryProgressSegment>(
                stories.Select(s => new StoryProgressSegment(s.Id)));

            if (stories.Count == 0)
            {
                await CloseAsync();
                return;
            }

            var initialIndex = stories.FindIndex(s => s.Id == selectedStoryId);
            if (initialIndex < 0)
                initialIndex = 0;

            SetCurrentStory(initialIndex, restartProgress: true);
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
    private Task Next()
    {
        return MoveNextAsync();
    }

    [RelayCommand]
    private Task Previous()
    {
        if (Stories.Count == 0)
            return Task.CompletedTask;

        if (CurrentIndex <= 0)
        {
            SetCurrentStory(0, restartProgress: true);
            return Task.CompletedTask;
        }

        SetCurrentStory(CurrentIndex - 1, restartProgress: true);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenLink()
    {
        if (CurrentStory is null || string.IsNullOrWhiteSpace(CurrentStory.LinkUrl))
            return;

        PauseProgress();

        if (!Uri.TryCreate(CurrentStory.LinkUrl, UriKind.Absolute, out var uri))
        {
            ResumeProgress();
            return;
        }

        try
        {
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            try
            {
                await Launcher.Default.OpenAsync(uri);
            }
            catch
            {
                await Shell.Current.DisplayAlertAsync("Hata", "Bu bağlantı açılamıyor.", "Tamam");
            }
        }
        finally
        {
            ResumeProgress();
        }
    }

    [RelayCommand]
    private Task Close()
    {
        return CloseAsync();
    }

    private async Task MoveNextAsync()
    {
        if (Stories.Count == 0)
            return;

        if (CurrentIndex >= Stories.Count - 1)
        {
            await CloseAsync();
            return;
        }

        SetCurrentStory(CurrentIndex + 1, restartProgress: true);
    }

    private void SetCurrentStory(int index, bool restartProgress)
    {
        if (Stories.Count == 0)
            return;

        var normalized = Math.Clamp(index, 0, Stories.Count - 1);
        CurrentIndex = normalized;
        CurrentStory = Stories[normalized];
        OnPropertyChanged(nameof(ProgressText));
        CurrentProgress = 0;
        UpdateProgressSegments();

        if (restartProgress)
            RestartProgressLoop();
    }

    private void RestartProgressLoop()
    {
        PauseProgress();
        _progressCts = new CancellationTokenSource();
        _ = RunProgressLoopAsync(_progressCts.Token);
    }

    private async Task RunProgressLoopAsync(CancellationToken ct)
    {
        try
        {
            var elapsed = (int)Math.Round(CurrentProgress * StoryDurationMs);
            if (elapsed < 0)
                elapsed = 0;
            if (elapsed > StoryDurationMs)
                elapsed = StoryDurationMs;

            for (; elapsed <= StoryDurationMs; elapsed += ProgressTickMs)
            {
                ct.ThrowIfCancellationRequested();
                CurrentProgress = Math.Min(1d, (double)elapsed / StoryDurationMs);
                await Task.Delay(ProgressTickMs, ct);
            }

            await MainThread.InvokeOnMainThreadAsync(MoveNextAsync);
        }
        catch (OperationCanceledException)
        {
            // Navigation/touch actions can interrupt the current progress loop.
        }
    }

    private void UpdateProgressSegments()
    {
        if (ProgressSegments.Count == 0)
            return;

        for (var i = 0; i < ProgressSegments.Count; i++)
        {
            var progress = i < CurrentIndex
                ? 1d
                : i == CurrentIndex
                    ? CurrentProgress
                    : 0d;

            ProgressSegments[i].Progress = progress;
        }
    }

    private Task CloseAsync()
    {
        PauseProgress();
        return CloseInternalAsync();
    }

    private static async Task CloseInternalAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..", true);
        }
        catch
        {
            await Shell.Current.GoToAsync("//home");
        }
    }

    private async Task MarkStorySeenSafeAsync(int storyId)
    {
        if (_seenSyncStoryIds.Contains(storyId))
            return;

        try
        {
            await _apiClient.MarkStorySeenAsync(storyId);
            _seenSyncStoryIds.Add(storyId);
        }
        catch
        {
            // Story seen bilgisi senkronize edilemese de story akışı devam etsin.
        }
    }
}

public partial class StoryProgressSegment(int storyId) : ObservableObject
{
    public int StoryId { get; } = storyId;

    [ObservableProperty] private double _progress;
}
