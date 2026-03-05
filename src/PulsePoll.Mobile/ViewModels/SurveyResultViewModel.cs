using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(ProjectId), "projectId")]
[QueryProperty(nameof(Status), "status")]
public partial class SurveyResultViewModel : ObservableObject
{
    private const string HomeRefreshRequiredKey = "home_refresh_required";
    private readonly IPulsePollApiClient _apiClient;
    private readonly MockDataService _mockDataService;

    public SurveyResultViewModel(IPulsePollApiClient apiClient, MockDataService mockDataService)
    {
        _apiClient = apiClient;
        _mockDataService = mockDataService;
    }

    [ObservableProperty] private int _projectId;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private SurveyModel? _survey;
    [ObservableProperty] private string _resultTitle = "Anketi Tamamladınız";
    [ObservableProperty] private string _resultMessage = "Katılımınız için teşekkür ederiz.";
    [ObservableProperty] private bool _isLoading;

    partial void OnProjectIdChanged(int value)
    {
        _ = LoadSurveyAsync(value);
    }

    partial void OnStatusChanged(string value)
    {
        ResultMessage = ResolveResultMessage(Survey, value);
    }

    [RelayCommand]
    private async Task Done()
    {
        Preferences.Default.Set(HomeRefreshRequiredKey, true);
        await Shell.Current.GoToAsync("//home");
    }

    [RelayCommand]
    private async Task GoBack()
    {
        Preferences.Default.Set(HomeRefreshRequiredKey, true);
        await Shell.Current.GoToAsync("//home");
    }

    private async Task LoadSurveyAsync(int projectId)
    {
        if (projectId <= 0)
            return;

        IsLoading = true;
        try
        {
            Survey = await FetchOrFallbackAsync(
                () => _apiClient.GetProjectByIdAsync(projectId),
                () => _mockDataService.GetSurveyDetail(projectId));
            ResultMessage = ResolveResultMessage(Survey, Status);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string ResolveResultMessage(SurveyModel? survey, string status)
    {
        if (survey is null)
            return "Katılımınız için teşekkür ederiz.";

        var message = status.Trim().ToLowerInvariant() switch
        {
            "completed" => survey.CompletedMessage,
            "disqualify" => survey.DisqualifyMessage,
            "quotafull" => survey.QuotaFullMessage,
            "screenout" => survey.ScreenOutMessage,
            _ => survey.CompletedMessage
        };

        return string.IsNullOrWhiteSpace(message)
            ? "Katılımınız için teşekkür ederiz."
            : message;
    }

    private static async Task<SurveyModel?> FetchOrFallbackAsync(
        Func<Task<SurveyModel?>> fetch,
        Func<SurveyModel> fallback)
    {
        try
        {
            return await fetch() ?? fallback();
        }
        catch
        {
            return fallback();
        }
    }
}
