using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(NewsId), "id")]
public partial class NewsDetailViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;

    public NewsDetailViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
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
        await Shell.Current.GoToAsync("..", true);
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
            var news = await _apiClient.GetNewsAsync();

            if (news.Count == 0)
            {
                await GoBack();
                return;
            }

            NewsItem = news.FirstOrDefault(n => n.Id == newsId) ?? news[0];
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
}
