using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(NewsId), "id")]
public partial class NewsDetailViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly MockDataService _mockDataService;

    public NewsDetailViewModel(IPulsePollApiClient apiClient, MockDataService mockDataService)
    {
        _apiClient = apiClient;
        _mockDataService = mockDataService;
    }

    [ObservableProperty] private int _newsId;
    [ObservableProperty] private NewsModel? _newsItem;
    [ObservableProperty] private bool _isLoading;

    public bool HasLink => NewsItem?.HasLink ?? false;

    partial void OnNewsIdChanged(int value)
    {
        _ = LoadNewsAsync(value);
    }

    partial void OnNewsItemChanged(NewsModel? value)
    {
        OnPropertyChanged(nameof(HasLink));
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task OpenLink()
    {
        if (NewsItem is null || string.IsNullOrWhiteSpace(NewsItem.LinkUrl))
            return;

        if (!Uri.TryCreate(NewsItem.LinkUrl, UriKind.Absolute, out var uri))
            return;

        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }

    private async Task LoadNewsAsync(int newsId)
    {
        IsLoading = true;
        try
        {
            var news = await FetchOrFallbackAsync(
                () => _apiClient.GetNewsAsync(),
                () => _mockDataService.GetNews());

            if (news.Count == 0)
            {
                await GoBack();
                return;
            }

            NewsItem = news.FirstOrDefault(n => n.Id == newsId) ?? news[0];
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static async Task<List<T>> FetchOrFallbackAsync<T>(
        Func<Task<List<T>>> fetch,
        Func<List<T>> fallback)
    {
        try
        {
            return await fetch();
        }
        catch
        {
            return fallback();
        }
    }
}
